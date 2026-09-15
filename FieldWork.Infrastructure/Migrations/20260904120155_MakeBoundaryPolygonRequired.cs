using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace FieldWork.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeBoundaryPolygonRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CenterLatitude",
                table: "beats");

            migrationBuilder.DropColumn(
                name: "CenterLongitude",
                table: "beats");

            migrationBuilder.DropColumn(
                name: "RadiusMeters",
                table: "beats");

            migrationBuilder.AlterColumn<Polygon>(
                name: "boundary_polygon",
                table: "beats",
                type: "geometry(Polygon, 4326)",
                nullable: false,
                oldClrType: typeof(Polygon),
                oldType: "geometry(Polygon, 4326)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Polygon>(
                name: "boundary_polygon",
                table: "beats",
                type: "geometry(Polygon, 4326)",
                nullable: true,
                oldClrType: typeof(Polygon),
                oldType: "geometry(Polygon, 4326)");

            migrationBuilder.AddColumn<decimal>(
                name: "CenterLatitude",
                table: "beats",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CenterLongitude",
                table: "beats",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "RadiusMeters",
                table: "beats",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
