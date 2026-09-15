using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace FieldWork.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBeatBoundaryPolygon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            // Ensure PostGIS extension is active before creating spatial columns
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS postgis;");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.AddColumn<Polygon>(
                name: "boundary_polygon",
                table: "beats",
                type: "geometry(Polygon, 4326)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_beats_boundary_polygon",
                table: "beats",
                column: "boundary_polygon")
                .Annotation("Npgsql:IndexMethod", "GIST");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_beats_boundary_polygon",
                table: "beats");

            migrationBuilder.DropColumn(
                name: "boundary_polygon",
                table: "beats");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:postgis", ",,");
        }
    }
}
