CREATE TABLE IF NOT EXISTS public.__efmigrationshistory (
    migration_id character varying(150) NOT NULL,
    product_version character varying(32) NOT NULL,
    CONSTRAINT pk___efmigrationshistory PRIMARY KEY (migration_id)
);

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909011222_InitialCreate') THEN
    INSERT INTO public.__efmigrationshistory (migration_id, product_version)
    VALUES ('20250909011222_InitialCreate', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER TABLE "Users" DROP CONSTRAINT "PK_Users";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER TABLE "Camisetas" DROP CONSTRAINT "PK_Camisetas";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER TABLE "Users" RENAME TO users;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER TABLE "Camisetas" RENAME TO camisetas;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER TABLE users RENAME COLUMN "Usuario" TO usuario;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER TABLE users RENAME COLUMN "Rol" TO rol;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER TABLE users RENAME COLUMN "Email" TO email;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER TABLE users RENAME COLUMN "Activo" TO activo;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER TABLE users RENAME COLUMN "Id" TO id;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER TABLE users RENAME COLUMN "UltimoAcceso" TO ultimo_acceso;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER TABLE users RENAME COLUMN "PasswordSalt" TO password_salt;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER TABLE users RENAME COLUMN "PasswordHash" TO password_hash;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER TABLE users RENAME COLUMN "IntentosFallidos" TO intentos_fallidos;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER TABLE users RENAME COLUMN "CreadoEn" TO creado_en;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER TABLE users RENAME COLUMN "BloqueadoHasta" TO bloqueado_hasta;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER TABLE users RENAME COLUMN "ActualizadoEn" TO actualizado_en;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER INDEX "IX_Users_Usuario" RENAME TO ix_users_usuario;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER INDEX "IX_Users_Email" RENAME TO ix_users_email;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER TABLE camisetas RENAME COLUMN "Version" TO version;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER TABLE camisetas RENAME COLUMN "Tipo" TO tipo;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER TABLE camisetas RENAME COLUMN "Temporada" TO temporada;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER TABLE camisetas RENAME COLUMN "Talla" TO talla;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER TABLE camisetas RENAME COLUMN "Stock" TO stock;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER TABLE camisetas RENAME COLUMN "SKU" TO sku;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER TABLE camisetas RENAME COLUMN "Precio" TO precio;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER TABLE camisetas RENAME COLUMN "Patrocinador" TO patrocinador;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER TABLE camisetas RENAME COLUMN "Numero" TO numero;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER TABLE camisetas RENAME COLUMN "Nombre" TO nombre;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER TABLE camisetas RENAME COLUMN "Material" TO material;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER TABLE camisetas RENAME COLUMN "Marca" TO marca;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER TABLE camisetas RENAME COLUMN "Manga" TO manga;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER TABLE camisetas RENAME COLUMN "Liga" TO liga;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER TABLE camisetas RENAME COLUMN "Jugador" TO jugador;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER TABLE camisetas RENAME COLUMN "Equipo" TO equipo;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER TABLE camisetas RENAME COLUMN "Descripcion" TO descripcion;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER TABLE camisetas RENAME COLUMN "Id" TO id;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER TABLE camisetas RENAME COLUMN "TipoCuello" TO tipo_cuello;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER TABLE camisetas RENAME COLUMN "ImagenUrl" TO imagen_url;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER TABLE camisetas RENAME COLUMN "FechaLanzamiento" TO fecha_lanzamiento;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER TABLE camisetas RENAME COLUMN "EsPersonalizada" TO es_personalizada;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER TABLE camisetas RENAME COLUMN "EsEdicionLimitada" TO es_edicion_limitada;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER TABLE camisetas RENAME COLUMN "ColorSecundario" TO color_secundario;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER TABLE camisetas RENAME COLUMN "ColorPrincipal" TO color_principal;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER TABLE camisetas RENAME COLUMN "CodigoBarras" TO codigo_barras;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER INDEX "IX_Camisetas_Temporada" RENAME TO ix_camisetas_temporada;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER INDEX "IX_Camisetas_SKU" RENAME TO ix_camisetas_sku;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER INDEX "IX_Camisetas_Liga" RENAME TO ix_camisetas_liga;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER INDEX "IX_Camisetas_Equipo" RENAME TO ix_camisetas_equipo;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER TABLE users ALTER COLUMN rol TYPE smallint;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER TABLE users ADD CONSTRAINT pk_users PRIMARY KEY (id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    ALTER TABLE camisetas ADD CONSTRAINT pk_camisetas PRIMARY KEY (id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    CREATE TABLE camiseta_imagenes (
        id integer GENERATED BY DEFAULT AS IDENTITY,
        camiseta_id integer NOT NULL,
        url character varying(2048) NOT NULL,
        orden smallint NOT NULL,
        CONSTRAINT pk_camiseta_imagenes PRIMARY KEY (id),
        CONSTRAINT fk_camiseta_imagenes_camisetas_camiseta_id FOREIGN KEY (camiseta_id) REFERENCES camisetas (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    CREATE INDEX ix_camiseta_imagenes_camiseta_id ON camiseta_imagenes (camiseta_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    CREATE UNIQUE INDEX ix_camiseta_imagenes_camiseta_id_orden ON camiseta_imagenes (camiseta_id, orden);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public.__efmigrationshistory WHERE "migration_id" = '20250909180722_AddCamisetaImagenes') THEN
    INSERT INTO public.__efmigrationshistory (migration_id, product_version)
    VALUES ('20250909180722_AddCamisetaImagenes', '8.0.8');
    END IF;
END $EF$;
COMMIT;

