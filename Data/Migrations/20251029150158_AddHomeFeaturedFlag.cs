using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ProyectoCamisetas.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHomeFeaturedFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "destacada_inicio",
                table: "camisetas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // No other schema changes
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "destacada_inicio",
                table: "camisetas");
        }
    }
}
