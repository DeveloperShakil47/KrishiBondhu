using System;
using FluentAssertions;
using Xunit;
using KrishiBondhu.Domain.Entities;
using KrishiBondhu.Domain.Enums;
using KrishiBondhu.Domain.Exceptions;

namespace KrishiBondhu.UnitTests;

public class OrderLifecycleTests
{
    [Fact]
    public void Order_TransitionTo_AllowsValidLifecycleFlow()
    {
        // Arrange
        var order = new Order
        {
            Status = OrderStatus.OfferAccepted,
            AgreedPricePerKg = 35.00m,
            AgreedQuantityKg = 1000m,
            VerifiedPaidAmount = 35000m // Paid in full
        };

        // Act & Assert 1: OfferAccepted -> AwaitingDelivery
        order.TransitionTo(OrderStatus.AwaitingDelivery);
        order.Status.Should().Be(OrderStatus.AwaitingDelivery);

        // Act & Assert 2: AwaitingDelivery -> DeliveredPendingConfirmation
        order.TransitionTo(OrderStatus.DeliveredPendingConfirmation);
        order.Status.Should().Be(OrderStatus.DeliveredPendingConfirmation);

        // Act & Assert 3: DeliveredPendingConfirmation -> PaymentPending
        order.TransitionTo(OrderStatus.PaymentPending);
        order.Status.Should().Be(OrderStatus.PaymentPending);

        // Act & Assert 4: PaymentPending -> Completed (Since OutstandingBalance <= 0)
        order.TransitionTo(OrderStatus.Completed);
        order.Status.Should().Be(OrderStatus.Completed);
        order.CompletedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void Order_TransitionTo_PreventsCompletion_WhenBalanceIsOutstanding()
    {
        // Arrange
        var order = new Order
        {
            Status = OrderStatus.PaymentPending,
            AgreedPricePerKg = 35.00m,
            AgreedQuantityKg = 1000m,
            VerifiedPaidAmount = 20000m // ৳ 15,000 outstanding!
        };

        // Act & Assert
        order.OutstandingBalance.Should().Be(15000m);
        Action act = () => order.TransitionTo(OrderStatus.Completed);
        act.Should().Throw<InvalidOrderStateTransitionException>()
            .WithMessage("*not permitted*");
    }

    [Fact]
    public void Order_TransitionTo_ThrowsWhenAttemptingToAlterCompletedOrder()
    {
        // Arrange
        var order = new Order
        {
            Status = OrderStatus.Completed,
            CompletedAtUtc = DateTime.UtcNow
        };

        // Act & Assert
        Action act = () => order.TransitionTo(OrderStatus.AwaitingDelivery);
        act.Should().Throw<InvalidOrderStateTransitionException>()
            .WithMessage("*Completed orders cannot be transitioned*");
    }

    [Fact]
    public void Order_TransitionTo_AllowsDisputeAtAnyDeliveryOrPaymentStage()
    {
        // Arrange
        var order = new Order
        {
            Status = OrderStatus.DeliveredPendingConfirmation
        };

        // Act
        order.TransitionTo(OrderStatus.Disputed, "ওজনে ৩০ কেজি অমিল");

        // Assert
        order.Status.Should().Be(OrderStatus.Disputed);
    }
}
