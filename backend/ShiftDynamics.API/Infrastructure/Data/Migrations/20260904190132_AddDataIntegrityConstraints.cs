using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShiftDynamics.API.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDataIntegrityConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_invoices_non_negative",
                table: "invoices",
                sql: "\"LaborCost\" >= 0 AND \"PartsCost\" >= 0 AND \"TaxAmount\" >= 0 AND \"DiscountAmount\" >= 0 AND \"Subtotal\" >= 0 AND \"TotalAmount\" >= 0 AND \"AmountPaid\" >= 0 AND \"BalanceDue\" >= 0 AND \"AmountPaid\" <= \"TotalAmount\"");

            migrationBuilder.AddCheckConstraint(
                name: "CK_inventory_items_non_negative",
                table: "inventory_items",
                sql: "\"OnHandQty\" >= 0 AND \"ReservedQty\" >= 0 AND \"ReorderLevel\" >= 0 AND \"UnitCost\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_estimates_non_negative",
                table: "estimates",
                sql: "\"LaborCost\" >= 0 AND \"PartsCost\" >= 0 AND \"TaxAmount\" >= 0 AND \"DiscountAmount\" >= 0 AND \"Subtotal\" >= 0 AND \"TotalAmount\" >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_invoices_non_negative",
                table: "invoices");

            migrationBuilder.DropCheckConstraint(
                name: "CK_inventory_items_non_negative",
                table: "inventory_items");

            migrationBuilder.DropCheckConstraint(
                name: "CK_estimates_non_negative",
                table: "estimates");
        }
    }
}
