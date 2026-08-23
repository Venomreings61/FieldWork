using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FieldWork.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConstraintsAndTenantScoping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_employees_EmployeeCode",
                table: "employees");

            migrationBuilder.DropIndex(
                name: "IX_employee_beats_EmployeeId_BeatId",
                table: "employee_beats");

            migrationBuilder.DropIndex(
                name: "IX_attendance_EmployeeId_RecordedAt",
                table: "attendance");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "users",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "employees",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            // BACKFILL: set each employee's TenantId from their linked User's TenantId,
            // since the column previously didn't exist and everyone defaulted to Guid.Empty above.
            migrationBuilder.Sql(@"
        UPDATE employees e
        SET ""TenantId"" = u.""TenantId""
        FROM users u
        WHERE e.""UserId"" = u.""Id"";
    ");

            migrationBuilder.CreateIndex(
                name: "IX_users_TenantId_Email",
                table: "users",
                columns: new[] { "TenantId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_employees_TenantId_EmployeeCode",
                table: "employees",
                columns: new[] { "TenantId", "EmployeeCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_attendance_EmployeeId_ReceivedAt",
                table: "attendance",
                columns: new[] { "EmployeeId", "ReceivedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_employees_tenants_TenantId",
                table: "employees",
                column: "TenantId",
                principalTable: "tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_employees_tenants_TenantId",
                table: "employees");

            migrationBuilder.DropIndex(
                name: "IX_users_TenantId_Email",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_employees_TenantId_EmployeeCode",
                table: "employees");

            migrationBuilder.DropIndex(
                name: "IX_attendance_EmployeeId_ReceivedAt",
                table: "attendance");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "employees");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "users",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.CreateIndex(
                name: "IX_employees_EmployeeCode",
                table: "employees",
                column: "EmployeeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_employee_beats_EmployeeId_BeatId",
                table: "employee_beats",
                columns: new[] { "EmployeeId", "BeatId" });

            migrationBuilder.CreateIndex(
                name: "IX_attendance_EmployeeId_RecordedAt",
                table: "attendance",
                columns: new[] { "EmployeeId", "RecordedAt" });
        }
    }
}
