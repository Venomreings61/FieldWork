using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FieldWork.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceCon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_attendance_sync_attempts_attendances_AttendanceId",
                table: "attendance_sync_attempts");

            migrationBuilder.DropForeignKey(
                name: "FK_attendances_beats_BeatId",
                table: "attendances");

            migrationBuilder.DropForeignKey(
                name: "FK_attendances_employees_EmployeeId",
                table: "attendances");

            migrationBuilder.DropPrimaryKey(
                name: "PK_attendances",
                table: "attendances");

            migrationBuilder.DropIndex(
                name: "IX_attendances_BeatId_RecordedAt",
                table: "attendances");

            migrationBuilder.RenameTable(
                name: "attendances",
                newName: "attendance");

            migrationBuilder.RenameIndex(
                name: "IX_attendances_EmployeeId_RecordedAt",
                table: "attendance",
                newName: "IX_attendance_EmployeeId_RecordedAt");

            migrationBuilder.RenameIndex(
                name: "IX_attendances_ClientAttendanceId",
                table: "attendance",
                newName: "IX_attendance_ClientAttendanceId");

            migrationBuilder.AlterColumn<string>(
                name: "SyncStatus",
                table: "attendance",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Source",
                table: "attendance",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "attendance",
                type: "numeric(10,7)",
                precision: 10,
                scale: 7,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(9,6)",
                oldPrecision: 9,
                oldScale: 6);

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "attendance",
                type: "numeric(10,7)",
                precision: 10,
                scale: 7,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(9,6)",
                oldPrecision: 9,
                oldScale: 6);

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                table: "attendance",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddPrimaryKey(
                name: "PK_attendance",
                table: "attendance",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_BeatId",
                table: "attendance",
                column: "BeatId");

            migrationBuilder.AddForeignKey(
                name: "FK_attendance_beats_BeatId",
                table: "attendance",
                column: "BeatId",
                principalTable: "beats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_attendance_employees_EmployeeId",
                table: "attendance",
                column: "EmployeeId",
                principalTable: "employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_attendance_sync_attempts_attendance_AttendanceId",
                table: "attendance_sync_attempts",
                column: "AttendanceId",
                principalTable: "attendance",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_attendance_beats_BeatId",
                table: "attendance");

            migrationBuilder.DropForeignKey(
                name: "FK_attendance_employees_EmployeeId",
                table: "attendance");

            migrationBuilder.DropForeignKey(
                name: "FK_attendance_sync_attempts_attendance_AttendanceId",
                table: "attendance_sync_attempts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_attendance",
                table: "attendance");

            migrationBuilder.DropIndex(
                name: "IX_attendance_BeatId",
                table: "attendance");

            migrationBuilder.RenameTable(
                name: "attendance",
                newName: "attendances");

            migrationBuilder.RenameIndex(
                name: "IX_attendance_EmployeeId_RecordedAt",
                table: "attendances",
                newName: "IX_attendances_EmployeeId_RecordedAt");

            migrationBuilder.RenameIndex(
                name: "IX_attendance_ClientAttendanceId",
                table: "attendances",
                newName: "IX_attendances_ClientAttendanceId");

            migrationBuilder.AlterColumn<string>(
                name: "SyncStatus",
                table: "attendances",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "Source",
                table: "attendances",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "attendances",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,7)",
                oldPrecision: 10,
                oldScale: 7);

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "attendances",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,7)",
                oldPrecision: 10,
                oldScale: 7);

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                table: "attendances",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AddPrimaryKey(
                name: "PK_attendances",
                table: "attendances",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_attendances_BeatId_RecordedAt",
                table: "attendances",
                columns: new[] { "BeatId", "RecordedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_attendance_sync_attempts_attendances_AttendanceId",
                table: "attendance_sync_attempts",
                column: "AttendanceId",
                principalTable: "attendances",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_attendances_beats_BeatId",
                table: "attendances",
                column: "BeatId",
                principalTable: "beats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_attendances_employees_EmployeeId",
                table: "attendances",
                column: "EmployeeId",
                principalTable: "employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
