using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoCamisetas.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHomeGridOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "destacada_inicio_orden",
                table: "camisetas",
                type: "smallint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_camisetas_destacada_inicio_orden",
                table: "camisetas",
                column: "destacada_inicio_orden");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_camisetas_destacada_inicio_orden",
                table: "camisetas");

            migrationBuilder.DropColumn(
                name: "destacada_inicio_orden",
                table: "camisetas");
        }
    }
}

