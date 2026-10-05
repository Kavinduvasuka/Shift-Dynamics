using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShiftDynamics.API.Migrations
{
    /// <inheritdoc />
    public partial class AddProcurementAndModificationWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DiagnosticFindings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MechanicStaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Finding = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MechanicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiagnosticFindings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiagnosticFindings_staff_MechanicId",
                        column: x => x.MechanicId,
                        principalTable: "staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiagnosticFindings_work_orders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "work_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MechanicRecommendations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MechanicStaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Recommendation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MechanicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MechanicRecommendations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MechanicRecommendations_staff_MechanicId",
                        column: x => x.MechanicId,
                        principalTable: "staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MechanicRecommendations_work_orders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "work_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RepairActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MechanicStaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MechanicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepairActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RepairActions_staff_MechanicId",
                        column: x => x.MechanicId,
                        principalTable: "staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RepairActions_work_orders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "work_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiagnosticFindings_MechanicId",
                table: "DiagnosticFindings",
                column: "MechanicId");

            migrationBuilder.CreateIndex(
                name: "IX_DiagnosticFindings_WorkOrderId",
                table: "DiagnosticFindings",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_MechanicRecommendations_MechanicId",
                table: "MechanicRecommendations",
                column: "MechanicId");

            migrationBuilder.CreateIndex(
                name: "IX_MechanicRecommendations_WorkOrderId",
                table: "MechanicRecommendations",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairActions_MechanicId",
                table: "RepairActions",
                column: "MechanicId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairActions_WorkOrderId",
                table: "RepairActions",
                column: "WorkOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiagnosticFindings");

            migrationBuilder.DropTable(
                name: "MechanicRecommendations");

            migrationBuilder.DropTable(
                name: "RepairActions");
        }
    }
}
