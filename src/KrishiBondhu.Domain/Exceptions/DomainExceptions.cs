using System;

namespace KrishiBondhu.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
    public DomainException(string message, Exception innerException) : base(message, innerException) { }
}

public class InvalidWeightException : DomainException
{
    public InvalidWeightException(string message) : base(message) { }
}

public class InvalidOrderStateTransitionException : DomainException
{
    public InvalidOrderStateTransitionException(string fromState, string toState, string reason)
        : base($"Cannot transition order from '{fromState}' to '{toState}'. Reason: {reason}")
    {
    }
}

public class UnverifiedPaymentException : DomainException
{
    public UnverifiedPaymentException(string message) : base(message) { }
}

public class OfferExpiredException : DomainException
{
    public OfferExpiredException(string message) : base(message) { }
}

public class UnauthorizedResourceAccessException : DomainException
{
    public UnauthorizedResourceAccessException(string message) : base(message) { }
}
