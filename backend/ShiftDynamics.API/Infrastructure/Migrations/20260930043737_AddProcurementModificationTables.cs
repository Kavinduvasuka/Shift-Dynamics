using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShiftDynamics.API.Migrations
{
    /// <inheritdoc />
    public partial class AddProcurementModificationTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "modification_requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Request = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ReviewedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReviewNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_modification_requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_modification_requests_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_modification_requests_vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vendor_quote_requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Specifications = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vendor_quote_requests", x => x.Id);
                    table.CheckConstraint("CK_vendor_quote_requests_qty", "\"Quantity\" > 0");
                    table.ForeignKey(
                        name: "FK_vendor_quote_requests_parts_PartId",
                        column: x => x.PartId,
                        principalTable: "parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vendor_quotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuoteRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VendorProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DeliveryDays = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vendor_quotes", x => x.Id);
                    table.CheckConstraint("CK_vendor_quotes_price", "\"UnitPrice\" >= 0 AND \"DeliveryDays\" >= 0");
                    table.ForeignKey(
                        name: "FK_vendor_quotes_vendor_profiles_VendorProfileId",
                        column: x => x.VendorProfileId,
                        principalTable: "vendor_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_vendor_quotes_vendor_quote_requests_QuoteRequestId",
                        column: x => x.QuoteRequestId,
                        principalTable: "vendor_quote_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "purchase_orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    QuoteRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VendorQuoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VendorProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchase_orders", x => x.Id);
                    table.CheckConstraint("CK_purchase_orders_qty", "\"Quantity\" > 0 AND \"UnitPrice\" >= 0 AND \"TotalAmount\" >= 0");
                    table.ForeignKey(
                        name: "FK_purchase_orders_parts_PartId",
                        column: x => x.PartId,
                        principalTable: "parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_purchase_orders_vendor_profiles_VendorProfileId",
                        column: x => x.VendorProfileId,
                        principalTable: "vendor_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_purchase_orders_vendor_quote_requests_QuoteRequestId",
                        column: x => x.QuoteRequestId,
                        principalTable: "vendor_quote_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_purchase_orders_vendor_quotes_VendorQuoteId",
                        column: x => x.VendorQuoteId,
                        principalTable: "vendor_quotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_modification_requests_CustomerId",
                table: "modification_requests",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_modification_requests_Status",
                table: "modification_requests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_modification_requests_VehicleId",
                table: "modification_requests",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_orders_OrderNumber",
                table: "purchase_orders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_purchase_orders_PartId",
                table: "purchase_orders",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_orders_QuoteRequestId",
                table: "purchase_orders",
                column: "QuoteRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_orders_VendorProfileId",
                table: "purchase_orders",
                column: "VendorProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_orders_VendorQuoteId",
                table: "purchase_orders",
                column: "VendorQuoteId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vendor_quote_requests_PartId",
                table: "vendor_quote_requests",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_vendor_quote_requests_Status",
                table: "vendor_quote_requests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_vendor_quotes_QuoteRequestId_VendorProfileId",
                table: "vendor_quotes",
                columns: new[] { "QuoteRequestId", "VendorProfileId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vendor_quotes_VendorProfileId",
                table: "vendor_quotes",
                column: "VendorProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "modification_requests");

            migrationBuilder.DropTable(
                name: "purchase_orders");

            migrationBuilder.DropTable(
                name: "vendor_quotes");

            migrationBuilder.DropTable(
                name: "vendor_quote_requests");
        }
    }
}
