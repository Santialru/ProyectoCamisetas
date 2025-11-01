using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoCamisetas.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeSkuOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Normalizar datos existentes: convertir cadenas vacías a NULL
            migrationBuilder.Sql("UPDATE camisetas SET sku = NULL WHERE sku = ''");

            migrationBuilder.AlterColumn<string>(
                name: "sku",
                table: "camisetas",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "sku",
                table: "camisetas",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}

