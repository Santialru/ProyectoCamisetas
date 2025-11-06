using Microsoft.EntityFrameworkCore;
using ProyectoCamisetas.Models;

namespace ProyectoCamisetas.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Camiseta> Camisetas => Set<Camiseta>();
        public DbSet<CamisetaImagen> CamisetaImagenes => Set<CamisetaImagen>();
        public DbSet<CamisetaTalleStock> CamisetaTalles => Set<CamisetaTalleStock>();
        public DbSet<HomeFeaturedCard> HomeFeatured => Set<HomeFeaturedCard>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Users -> tabla 'users' (minúsculas, evitar palabra reservada 'user')
            modelBuilder.Entity<User>(e =>
            {
                e.ToTable("users");
                e.HasIndex(x => x.Email).IsUnique();
                e.HasIndex(x => x.Usuario).IsUnique();
                e.Property(x => x.Rol).HasConversion<short>();
            });

            // Camisetas -> tabla 'camisetas' (minúsculas)
            modelBuilder.Entity<Camiseta>(e =>
            {
                e.ToTable("camisetas");
                e.HasIndex(x => x.SKU).IsUnique();
                e.HasIndex(x => x.Equipo);
                e.HasIndex(x => x.Liga);
                e.HasIndex(x => x.Temporada);
                e.HasMany(c => c.Imagenes)
                 .WithOne(i => i.Camiseta!)
                 .HasForeignKey(i => i.CamisetaId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasMany(c => c.TallesStock)
                 .WithOne(ts => ts.Camiseta!)
                 .HasForeignKey(ts => ts.CamisetaId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // Home featured grid -> 'home_featured'
            modelBuilder.Entity<HomeFeaturedCard>(e =>
            {
                e.ToTable("home_featured");
                e.Property(h => h.Orden).HasColumnType("smallint");
                e.HasIndex(h => h.Orden).IsUnique();
                e.HasIndex(h => h.CamisetaId).IsUnique();
                e.HasOne(h => h.Camiseta!)
                    .WithMany()
                    .HasForeignKey(h => h.CamisetaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // CamisetaImagenes -> 'camiseta_imagenes'
            modelBuilder.Entity<CamisetaImagen>(e =>
            {
                e.ToTable("camiseta_imagenes");
                e.Property(i => i.Orden).HasColumnType("smallint");
                e.HasIndex(i => i.CamisetaId);
                e.HasIndex(i => new { i.CamisetaId, i.Orden }).IsUnique();
            });

            // CamisetaTalleStock -> 'camiseta_talles'
            modelBuilder.Entity<CamisetaTalleStock>(e =>
            {
                e.ToTable("camiseta_talles");
                e.Property(x => x.Talla).HasConversion<short>();
                e.HasIndex(x => x.CamisetaId);
                e.HasIndex(x => new { x.CamisetaId, x.Talla }).IsUnique();
            });
        }
    }
}
