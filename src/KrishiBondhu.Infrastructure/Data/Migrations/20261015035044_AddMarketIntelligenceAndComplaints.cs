using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KrishiBondhu.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMarketIntelligenceAndComplaints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Markets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MarketCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NameBn = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    MarketType = table.Column<int>(type: "integer", nullable: false),
                    District = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Upazila = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AddressDetails = table.Column<string>(type: "text", nullable: true),
                    Latitude = table.Column<decimal>(type: "numeric(10,7)", precision: 10, scale: 7, nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric(10,7)", precision: 10, scale: 7, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Markets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OfficialPriceDirectives",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DirectiveNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TitleBn = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    IssuingAuthority = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SourceReference = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    CropId = table.Column<Guid>(type: "uuid", nullable: false),
                    MarketType = table.Column<int>(type: "integer", nullable: false),
                    ApplicableDistrict = table.Column<string>(type: "text", nullable: true),
                    ApplicableUpazila = table.Column<string>(type: "text", nullable: true),
                    EffectiveFromUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EffectiveToUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OfficialBenchmarkPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    LegalMaxPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    StandardUnit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ApplicableConditions = table.Column<string>(type: "text", nullable: true),
                    VerificationStatus = table.Column<int>(type: "integer", nullable: false),
                    VerifiedByUserId = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfficialPriceDirectives", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OfficialPriceDirectives_Crops_CropId",
                        column: x => x.CropId,
                        principalTable: "Crops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PriceSources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NameBn = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    SourceType = table.Column<int>(type: "integer", nullable: false),
                    OrganizationName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsOfficialAuthority = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceSources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConsumerComplaints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TrackingNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ComplainantUserId = table.Column<string>(type: "text", nullable: true),
                    ComplainantName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ComplainantPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ComplainantEmail = table.Column<string>(type: "text", nullable: true),
                    ShopOrBusinessName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ShopAddress = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    MarketId = table.Column<Guid>(type: "uuid", nullable: true),
                    District = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Upazila = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SellerType = table.Column<int>(type: "integer", nullable: false),
                    CropId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityPurchased = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ReportedUnit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ActualPriceCharged = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    NormalizedPricePerUnit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    StandardUnit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PurchaseDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ReceiptEvidencePath = table.Column<string>(type: "text", nullable: true),
                    ReceiptFileName = table.Column<string>(type: "text", nullable: true),
                    ReceiptFileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    ReceiptContentType = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AssignedReviewerUserId = table.Column<string>(type: "text", nullable: true),
                    ReviewerNotes = table.Column<string>(type: "text", nullable: true),
                    DecisionReason = table.Column<string>(type: "text", nullable: true),
                    ReviewedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MatchedMarketAveragePrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    MatchedMarketMinPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    MatchedMarketMaxPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    PriceVariancePercentage = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    OfficialPriceDirectiveId = table.Column<Guid>(type: "uuid", nullable: true),
                    OfficialLegalPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    IsOfficialPriceExceeded = table.Column<bool>(type: "boolean", nullable: false),
                    ComparisonAnalysisNotes = table.Column<string>(type: "text", nullable: true),
                    IsPubliclyVisible = table.Column<bool>(type: "boolean", nullable: false),
                    PublicSummary = table.Column<string>(type: "text", nullable: true),
                    ClientIpHash = table.Column<string>(type: "text", nullable: true),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumerComplaints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsumerComplaints_Crops_CropId",
                        column: x => x.CropId,
                        principalTable: "Crops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsumerComplaints_Markets_MarketId",
                        column: x => x.MarketId,
                        principalTable: "Markets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ConsumerComplaints_OfficialPriceDirectives_OfficialPriceDir~",
                        column: x => x.OfficialPriceDirectiveId,
                        principalTable: "OfficialPriceDirectives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MarketPriceReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MarketId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PriceSourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmittedByUserId = table.Column<string>(type: "text", nullable: true),
                    VerificationStatus = table.Column<int>(type: "integer", nullable: false),
                    VerifiedByUserId = table.Column<string>(type: "text", nullable: true),
                    VerifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerificationNotes = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketPriceReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketPriceReports_Markets_MarketId",
                        column: x => x.MarketId,
                        principalTable: "Markets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MarketPriceReports_PriceSources_PriceSourceId",
                        column: x => x.PriceSourceId,
                        principalTable: "PriceSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MarketPriceObservations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CropId = table.Column<Guid>(type: "uuid", nullable: false),
                    MarketId = table.Column<Guid>(type: "uuid", nullable: false),
                    MarketPriceReportId = table.Column<Guid>(type: "uuid", nullable: true),
                    MarketType = table.Column<int>(type: "integer", nullable: false),
                    ObservationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PriceSourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObservedPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ObservedQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ObservedUnit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NormalizedPricePerUnit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    NormalizedUnit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MinPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    MaxPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    NormalizedMinPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    NormalizedMaxPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Grade = table.Column<int>(type: "integer", nullable: false),
                    VerificationStatus = table.Column<int>(type: "integer", nullable: false),
                    VerifiedByUserId = table.Column<string>(type: "text", nullable: true),
                    VerifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketPriceObservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketPriceObservations_Crops_CropId",
                        column: x => x.CropId,
                        principalTable: "Crops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MarketPriceObservations_MarketPriceReports_MarketPriceRepor~",
                        column: x => x.MarketPriceReportId,
                        principalTable: "MarketPriceReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MarketPriceObservations_Markets_MarketId",
                        column: x => x.MarketId,
                        principalTable: "Markets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MarketPriceObservations_PriceSources_PriceSourceId",
                        column: x => x.PriceSourceId,
                        principalTable: "PriceSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsumerComplaints_ClientIpHash",
                table: "ConsumerComplaints",
                column: "ClientIpHash");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumerComplaints_ComplainantPhone",
                table: "ConsumerComplaints",
                column: "ComplainantPhone");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumerComplaints_CreatedAtUtc",
                table: "ConsumerComplaints",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumerComplaints_CropId",
                table: "ConsumerComplaints",
                column: "CropId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumerComplaints_MarketId",
                table: "ConsumerComplaints",
                column: "MarketId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumerComplaints_OfficialPriceDirectiveId",
                table: "ConsumerComplaints",
                column: "OfficialPriceDirectiveId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumerComplaints_Status",
                table: "ConsumerComplaints",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumerComplaints_TrackingNumber",
                table: "ConsumerComplaints",
                column: "TrackingNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MarketPriceObservations_CropId_ObservationDate_MarketType_V~",
                table: "MarketPriceObservations",
                columns: new[] { "CropId", "ObservationDate", "MarketType", "VerificationStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_MarketPriceObservations_MarketId_ObservationDate",
                table: "MarketPriceObservations",
                columns: new[] { "MarketId", "ObservationDate" });

            migrationBuilder.CreateIndex(
                name: "IX_MarketPriceObservations_MarketPriceReportId",
                table: "MarketPriceObservations",
                column: "MarketPriceReportId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketPriceObservations_PriceSourceId",
                table: "MarketPriceObservations",
                column: "PriceSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketPriceReports_MarketId",
                table: "MarketPriceReports",
                column: "MarketId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketPriceReports_PriceSourceId",
                table: "MarketPriceReports",
                column: "PriceSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Markets_MarketCode",
                table: "Markets",
                column: "MarketCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OfficialPriceDirectives_CropId_EffectiveFromUtc_EffectiveTo~",
                table: "OfficialPriceDirectives",
                columns: new[] { "CropId", "EffectiveFromUtc", "EffectiveToUtc", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_OfficialPriceDirectives_DirectiveNumber",
                table: "OfficialPriceDirectives",
                column: "DirectiveNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsumerComplaints");

            migrationBuilder.DropTable(
                name: "MarketPriceObservations");

            migrationBuilder.DropTable(
                name: "OfficialPriceDirectives");

            migrationBuilder.DropTable(
                name: "MarketPriceReports");

            migrationBuilder.DropTable(
                name: "Markets");

            migrationBuilder.DropTable(
                name: "PriceSources");
        }
    }
}
