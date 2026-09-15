using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FieldWork.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFaceVerificationRequiredToTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "FaceVerificationRequired",
                table: "tenants",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FaceVerificationRequired",
                table: "tenants");
        }
    }
}
