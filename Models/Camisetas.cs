using System.ComponentModel.DataAnnotations;

namespace ProyectoCamisetas.Models
{
    public class Camiseta
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(80)]
        [Display(Name = "Nombre del modelo")]
        public string Nombre { get; set; } = string.Empty;

        [Required, StringLength(80)]
        [Display(Name = "Equipo/Club")]
        public string Equipo { get; set; } = string.Empty;

        [Required, StringLength(60)]
        public string Liga { get; set; } = string.Empty;

        [Required, StringLength(15)]
        [Display(Name = "Temporada (ej. 2024/25)")]
        public string Temporada { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Tipo de kit")]
        public TipoKit Tipo { get; set; }

        [Display(Name = "Versión")]
        [Required]
        public VersionCamiseta Version { get; set; } = VersionCamiseta.Aficionado;

        [Required, StringLength(40)]
        [Display(Name = "Marca/Proveedor")]
        public string Marca { get; set; } = string.Empty;

        [StringLength(60)]
        [Display(Name = "Patrocinador principal")]
        public string? Patrocinador { get; set; }

        [Required, StringLength(30)]
        [Display(Name = "Color principal")]
        public string ColorPrincipal { get; set; } = string.Empty;

        [StringLength(30)]
        [Display(Name = "Color secundario")]
        public string? ColorSecundario { get; set; }

        [Required, StringLength(50)]
        public string Material { get; set; } = string.Empty;

        [Range(0, 999999)]
        [DataType(DataType.Currency)]
        public decimal Precio { get; set; }

        [Range(0, 100000)]
        public int Stock { get; set; }

        [Required, StringLength(30)]
        [Display(Name = "SKU/Código de producto")]
        public string SKU { get; set; } = string.Empty;

        [Display(Name = "Fecha de lanzamiento")]
        public DateOnly? FechaLanzamiento { get; set; }

        [Display(Name = "Edición limitada")]
        public bool EsEdicionLimitada { get; set; }

        [Display(Name = "Personalizada con nombre/número")]
        public bool EsPersonalizada { get; set; }

        [Display(Name = "Talla")]
        public Talla? Talla { get; set; }

        [Display(Name = "Manga")]
        public Manga? Manga { get; set; }

        [StringLength(25)]
        [Display(Name = "Tipo de cuello")]
        public string? TipoCuello { get; set; }

        [StringLength(25)]
        [Display(Name = "Jugador (si aplica)")]
        public string? Jugador { get; set; }

        [Range(0, 99)]
        [Display(Name = "Número (si aplica)")]
        public int? Numero { get; set; }

        [Url]
        [Display(Name = "Imagen (URL)")]
        public string? ImagenUrl { get; set; }

        [StringLength(500)]
        public string? Descripcion { get; set; }

        [StringLength(32)]
        [Display(Name = "Código de barras")]
        public string? CodigoBarras { get; set; }

        // Propiedad de conveniencia
        [Display(Name = "Disponible")]
        public bool EnStock => Stock > 0;

        // Relación: múltiples imágenes
        public ICollection<CamisetaImagen>? Imagenes { get; set; }
    }

    public enum TipoKit
    {
        Local = 0,
        Visitante = 1,
        Tercera = 2,
        Portero = 3,
        Especial = 4
    }

    public enum VersionCamiseta
    {
        Aficionado = 0, // Stadium
        Jugador = 1     // Match/Player Issue
    }

    public enum Talla
    {
        XS = 0,
        S = 1,
        M = 2,
        L = 3,
        XL = 4,
        XXL = 5,
        Niños = 6
    }

    public enum Manga
    {
        Corta = 0,
        Larga = 1
    }
}

namespace ProyectoCamisetas.Models
{
    public class CamisetaImagen
    {
        public int Id { get; set; }
        public int CamisetaId { get; set; }
        public Camiseta? Camiseta { get; set; }

        [Required]
        [Url]
        [StringLength(2048)]
        public string Url { get; set; } = string.Empty;

        [Range(0, 5)]
        public short Orden { get; set; }
    }
}
