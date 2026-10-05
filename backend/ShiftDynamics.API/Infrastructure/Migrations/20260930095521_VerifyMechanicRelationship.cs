using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShiftDynamics.API.Migrations
{
    /// <inheritdoc />
    public partial class VerifyMechanicRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiagnosticFindings_staff_MechanicId",
                table: "DiagnosticFindings");

            migrationBuilder.DropForeignKey(
                name: "FK_MechanicRecommendations_staff_MechanicId",
                table: "MechanicRecommendations");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairActions_staff_MechanicId",
                table: "RepairActions");

            migrationBuilder.DropIndex(
                name: "IX_RepairActions_MechanicId",
                table: "RepairActions");

            migrationBuilder.DropIndex(
                name: "IX_MechanicRecommendations_MechanicId",
                table: "MechanicRecommendations");

            migrationBuilder.DropIndex(
                name: "IX_DiagnosticFindings_MechanicId",
                table: "DiagnosticFindings");

            migrationBuilder.DropColumn(
                name: "MechanicId",
                table: "RepairActions");

            migrationBuilder.DropColumn(
                name: "MechanicId",
                table: "MechanicRecommendations");

            migrationBuilder.DropColumn(
                name: "MechanicId",
                table: "DiagnosticFindings");

            migrationBuilder.CreateIndex(
                name: "IX_RepairActions_MechanicStaffId",
                table: "RepairActions",
                column: "MechanicStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_MechanicRecommendations_MechanicStaffId",
                table: "MechanicRecommendations",
                column: "MechanicStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_DiagnosticFindings_MechanicStaffId",
                table: "DiagnosticFindings",
                column: "MechanicStaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_DiagnosticFindings_staff_MechanicStaffId",
                table: "DiagnosticFindings",
                column: "MechanicStaffId",
                principalTable: "staff",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MechanicRecommendations_staff_MechanicStaffId",
                table: "MechanicRecommendations",
                column: "MechanicStaffId",
                principalTable: "staff",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RepairActions_staff_MechanicStaffId",
                table: "RepairActions",
                column: "MechanicStaffId",
                principalTable: "staff",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiagnosticFindings_staff_MechanicStaffId",
                table: "DiagnosticFindings");

            migrationBuilder.DropForeignKey(
                name: "FK_MechanicRecommendations_staff_MechanicStaffId",
                table: "MechanicRecommendations");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairActions_staff_MechanicStaffId",
                table: "RepairActions");

            migrationBuilder.DropIndex(
                name: "IX_RepairActions_MechanicStaffId",
                table: "RepairActions");

            migrationBuilder.DropIndex(
                name: "IX_MechanicRecommendations_MechanicStaffId",
                table: "MechanicRecommendations");

            migrationBuilder.DropIndex(
                name: "IX_DiagnosticFindings_MechanicStaffId",
                table: "DiagnosticFindings");

            migrationBuilder.AddColumn<Guid>(
                name: "MechanicId",
                table: "RepairActions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "MechanicId",
                table: "MechanicRecommendations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "MechanicId",
                table: "DiagnosticFindings",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_RepairActions_MechanicId",
                table: "RepairActions",
                column: "MechanicId");

            migrationBuilder.CreateIndex(
                name: "IX_MechanicRecommendations_MechanicId",
                table: "MechanicRecommendations",
                column: "MechanicId");

            migrationBuilder.CreateIndex(
                name: "IX_DiagnosticFindings_MechanicId",
                table: "DiagnosticFindings",
                column: "MechanicId");

            migrationBuilder.AddForeignKey(
                name: "FK_DiagnosticFindings_staff_MechanicId",
                table: "DiagnosticFindings",
                column: "MechanicId",
                principalTable: "staff",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MechanicRecommendations_staff_MechanicId",
                table: "MechanicRecommendations",
                column: "MechanicId",
                principalTable: "staff",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RepairActions_staff_MechanicId",
                table: "RepairActions",
                column: "MechanicId",
                principalTable: "staff",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
