using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ProyectoCamisetas.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCamisetaImagenes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Camisetas",
                table: "Camisetas");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "users");

            migrationBuilder.RenameTable(
                name: "Camisetas",
                newName: "camisetas");

            migrationBuilder.RenameColumn(
                name: "Usuario",
                table: "users",
                newName: "usuario");

            migrationBuilder.RenameColumn(
                name: "Rol",
                table: "users",
                newName: "rol");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "users",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "users",
                newName: "activo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "users",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UltimoAcceso",
                table: "users",
                newName: "ultimo_acceso");

            migrationBuilder.RenameColumn(
                name: "PasswordSalt",
                table: "users",
                newName: "password_salt");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "users",
                newName: "password_hash");

            migrationBuilder.RenameColumn(
                name: "IntentosFallidos",
                table: "users",
                newName: "intentos_fallidos");

            migrationBuilder.RenameColumn(
                name: "CreadoEn",
                table: "users",
                newName: "creado_en");

            migrationBuilder.RenameColumn(
                name: "BloqueadoHasta",
                table: "users",
                newName: "bloqueado_hasta");

            migrationBuilder.RenameColumn(
                name: "ActualizadoEn",
                table: "users",
                newName: "actualizado_en");

            migrationBuilder.RenameIndex(
                name: "IX_Users_Usuario",
                table: "users",
                newName: "ix_users_usuario");

            migrationBuilder.RenameIndex(
                name: "IX_Users_Email",
                table: "users",
                newName: "ix_users_email");

            migrationBuilder.RenameColumn(
                name: "Version",
                table: "camisetas",
                newName: "version");

            migrationBuilder.RenameColumn(
                name: "Tipo",
                table: "camisetas",
                newName: "tipo");

            migrationBuilder.RenameColumn(
                name: "Temporada",
                table: "camisetas",
                newName: "temporada");

            migrationBuilder.RenameColumn(
                name: "Talla",
                table: "camisetas",
                newName: "talla");

            migrationBuilder.RenameColumn(
                name: "Stock",
                table: "camisetas",
                newName: "stock");

            migrationBuilder.RenameColumn(
                name: "SKU",
                table: "camisetas",
                newName: "sku");

            migrationBuilder.RenameColumn(
                name: "Precio",
                table: "camisetas",
                newName: "precio");

            migrationBuilder.RenameColumn(
                name: "Patrocinador",
                table: "camisetas",
                newName: "patrocinador");

            migrationBuilder.RenameColumn(
                name: "Numero",
                table: "camisetas",
                newName: "numero");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "camisetas",
                newName: "nombre");

            migrationBuilder.RenameColumn(
                name: "Material",
                table: "camisetas",
                newName: "material");

            migrationBuilder.RenameColumn(
                name: "Marca",
                table: "camisetas",
                newName: "marca");

            migrationBuilder.RenameColumn(
                name: "Manga",
                table: "camisetas",
                newName: "manga");

            migrationBuilder.RenameColumn(
                name: "Liga",
                table: "camisetas",
                newName: "liga");

            migrationBuilder.RenameColumn(
                name: "Jugador",
                table: "camisetas",
                newName: "jugador");

            migrationBuilder.RenameColumn(
                name: "Equipo",
                table: "camisetas",
                newName: "equipo");

            migrationBuilder.RenameColumn(
                name: "Descripcion",
                table: "camisetas",
                newName: "descripcion");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "camisetas",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TipoCuello",
                table: "camisetas",
                newName: "tipo_cuello");

            migrationBuilder.RenameColumn(
                name: "ImagenUrl",
                table: "camisetas",
                newName: "imagen_url");

            migrationBuilder.RenameColumn(
                name: "FechaLanzamiento",
                table: "camisetas",
                newName: "fecha_lanzamiento");

            migrationBuilder.RenameColumn(
                name: "EsPersonalizada",
                table: "camisetas",
                newName: "es_personalizada");

            migrationBuilder.RenameColumn(
                name: "EsEdicionLimitada",
                table: "camisetas",
                newName: "es_edicion_limitada");

            migrationBuilder.RenameColumn(
                name: "ColorSecundario",
                table: "camisetas",
                newName: "color_secundario");

            migrationBuilder.RenameColumn(
                name: "ColorPrincipal",
                table: "camisetas",
                newName: "color_principal");

            migrationBuilder.RenameColumn(
                name: "CodigoBarras",
                table: "camisetas",
                newName: "codigo_barras");

            migrationBuilder.RenameIndex(
                name: "IX_Camisetas_Temporada",
                table: "camisetas",
                newName: "ix_camisetas_temporada");

            migrationBuilder.RenameIndex(
                name: "IX_Camisetas_SKU",
                table: "camisetas",
                newName: "ix_camisetas_sku");

            migrationBuilder.RenameIndex(
                name: "IX_Camisetas_Liga",
                table: "camisetas",
                newName: "ix_camisetas_liga");

            migrationBuilder.RenameIndex(
                name: "IX_Camisetas_Equipo",
                table: "camisetas",
                newName: "ix_camisetas_equipo");

            migrationBuilder.AlterColumn<short>(
                name: "rol",
                table: "users",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddPrimaryKey(
                name: "pk_users",
                table: "users",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_camisetas",
                table: "camisetas",
                column: "id");

            migrationBuilder.CreateTable(
                name: "camiseta_imagenes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    camiseta_id = table.Column<int>(type: "integer", nullable: false),
                    url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    orden = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_camiseta_imagenes", x => x.id);
                    table.ForeignKey(
                        name: "fk_camiseta_imagenes_camisetas_camiseta_id",
                        column: x => x.camiseta_id,
                        principalTable: "camisetas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_camiseta_imagenes_camiseta_id",
                table: "camiseta_imagenes",
                column: "camiseta_id");

            migrationBuilder.CreateIndex(
                name: "ix_camiseta_imagenes_camiseta_id_orden",
                table: "camiseta_imagenes",
                columns: new[] { "camiseta_id", "orden" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "camiseta_imagenes");

            migrationBuilder.DropPrimaryKey(
                name: "pk_users",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "pk_camisetas",
                table: "camisetas");

            migrationBuilder.RenameTable(
                name: "users",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "camisetas",
                newName: "Camisetas");

            migrationBuilder.RenameColumn(
                name: "usuario",
                table: "Users",
                newName: "Usuario");

            migrationBuilder.RenameColumn(
                name: "rol",
                table: "Users",
                newName: "Rol");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "Users",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "activo",
                table: "Users",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Users",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ultimo_acceso",
                table: "Users",
                newName: "UltimoAcceso");

            migrationBuilder.RenameColumn(
                name: "password_salt",
                table: "Users",
                newName: "PasswordSalt");

            migrationBuilder.RenameColumn(
                name: "password_hash",
                table: "Users",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "intentos_fallidos",
                table: "Users",
                newName: "IntentosFallidos");

            migrationBuilder.RenameColumn(
                name: "creado_en",
                table: "Users",
                newName: "CreadoEn");

            migrationBuilder.RenameColumn(
                name: "bloqueado_hasta",
                table: "Users",
                newName: "BloqueadoHasta");

            migrationBuilder.RenameColumn(
                name: "actualizado_en",
                table: "Users",
                newName: "ActualizadoEn");

            migrationBuilder.RenameIndex(
                name: "ix_users_usuario",
                table: "Users",
                newName: "IX_Users_Usuario");

            migrationBuilder.RenameIndex(
                name: "ix_users_email",
                table: "Users",
                newName: "IX_Users_Email");

            migrationBuilder.RenameColumn(
                name: "version",
                table: "Camisetas",
                newName: "Version");

            migrationBuilder.RenameColumn(
                name: "tipo",
                table: "Camisetas",
                newName: "Tipo");

            migrationBuilder.RenameColumn(
                name: "temporada",
                table: "Camisetas",
                newName: "Temporada");

            migrationBuilder.RenameColumn(
                name: "talla",
                table: "Camisetas",
                newName: "Talla");

            migrationBuilder.RenameColumn(
                name: "stock",
                table: "Camisetas",
                newName: "Stock");

            migrationBuilder.RenameColumn(
                name: "sku",
                table: "Camisetas",
                newName: "SKU");

            migrationBuilder.RenameColumn(
                name: "precio",
                table: "Camisetas",
                newName: "Precio");

            migrationBuilder.RenameColumn(
                name: "patrocinador",
                table: "Camisetas",
                newName: "Patrocinador");

            migrationBuilder.RenameColumn(
                name: "numero",
                table: "Camisetas",
                newName: "Numero");

            migrationBuilder.RenameColumn(
                name: "nombre",
                table: "Camisetas",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "material",
                table: "Camisetas",
                newName: "Material");

            migrationBuilder.RenameColumn(
                name: "marca",
                table: "Camisetas",
                newName: "Marca");

            migrationBuilder.RenameColumn(
                name: "manga",
                table: "Camisetas",
                newName: "Manga");

            migrationBuilder.RenameColumn(
                name: "liga",
                table: "Camisetas",
                newName: "Liga");

            migrationBuilder.RenameColumn(
                name: "jugador",
                table: "Camisetas",
                newName: "Jugador");

            migrationBuilder.RenameColumn(
                name: "equipo",
                table: "Camisetas",
                newName: "Equipo");

            migrationBuilder.RenameColumn(
                name: "descripcion",
                table: "Camisetas",
                newName: "Descripcion");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Camisetas",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "tipo_cuello",
                table: "Camisetas",
                newName: "TipoCuello");

            migrationBuilder.RenameColumn(
                name: "imagen_url",
                table: "Camisetas",
                newName: "ImagenUrl");

            migrationBuilder.RenameColumn(
                name: "fecha_lanzamiento",
                table: "Camisetas",
                newName: "FechaLanzamiento");

            migrationBuilder.RenameColumn(
                name: "es_personalizada",
                table: "Camisetas",
                newName: "EsPersonalizada");

            migrationBuilder.RenameColumn(
                name: "es_edicion_limitada",
                table: "Camisetas",
                newName: "EsEdicionLimitada");

            migrationBuilder.RenameColumn(
                name: "color_secundario",
                table: "Camisetas",
                newName: "ColorSecundario");

            migrationBuilder.RenameColumn(
                name: "color_principal",
                table: "Camisetas",
                newName: "ColorPrincipal");

            migrationBuilder.RenameColumn(
                name: "codigo_barras",
                table: "Camisetas",
                newName: "CodigoBarras");

            migrationBuilder.RenameIndex(
                name: "ix_camisetas_temporada",
                table: "Camisetas",
                newName: "IX_Camisetas_Temporada");

            migrationBuilder.RenameIndex(
                name: "ix_camisetas_sku",
                table: "Camisetas",
                newName: "IX_Camisetas_SKU");

            migrationBuilder.RenameIndex(
                name: "ix_camisetas_liga",
                table: "Camisetas",
                newName: "IX_Camisetas_Liga");

            migrationBuilder.RenameIndex(
                name: "ix_camisetas_equipo",
                table: "Camisetas",
                newName: "IX_Camisetas_Equipo");

            migrationBuilder.AlterColumn<int>(
                name: "Rol",
                table: "Users",
                type: "integer",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Camisetas",
                table: "Camisetas",
                column: "Id");
        }
    }
}
