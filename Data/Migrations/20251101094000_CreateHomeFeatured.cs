using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoCamisetas.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateHomeFeatured : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "home_featured",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    camiseta_id = table.Column<int>(type: "integer", nullable: false),
                    orden = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_home_featured", x => x.id);
                    table.ForeignKey(
                        name: "fk_home_featured_camisetas_camiseta_id",
                        column: x => x.camiseta_id,
                        principalTable: "camisetas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_home_featured_camiseta_id",
                table: "home_featured",
                column: "camiseta_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_home_featured_orden",
                table: "home_featured",
                column: "orden",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "home_featured");
        }
    }
}

