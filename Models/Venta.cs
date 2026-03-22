using System.ComponentModel.DataAnnotations;

namespace ProyectoCamisetas.Models
{
    public class Venta
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CamisetaId { get; set; }
        public Camiseta? Camiseta { get; set; }

        [Required]
        public DateTime FechaVenta { get; set; } = DateTime.UtcNow;

        [Required]
        [Range(0, 999999)]
        public decimal Precio { get; set; }

        public Talla Talla { get; set; }

        [StringLength(120)]
        public string? Comprador { get; set; }

        [StringLength(500)]
        public string? Observaciones { get; set; }

        // Datos de denormalización para reportes rápidos
        [StringLength(120)]
        public string? ProductoNombre { get; set; }

        [StringLength(80)]
        public string? Equipo { get; set; }

        [StringLength(15)]
        public string? Temporada { get; set; }
    }

    public class VentasSummary
    {
        public decimal TotalRecaudado { get; set; }
        public int Cantidad { get; set; }
        public decimal TicketPromedio { get; set; }
        public decimal PrecioMax { get; set; }
        public decimal PrecioMin { get; set; }
        public IReadOnlyList<(Talla talla, int cantidad)> TopTalles { get; set; } = Array.Empty<(Talla, int)>();
        public IReadOnlyList<(string equipo, int cantidad)> TopEquipos { get; set; } = Array.Empty<(string, int)>();
        public IReadOnlyList<(DateOnly fecha, int cantidad)> VentasPorDia { get; set; } = Array.Empty<(DateOnly, int)>();
    }
}
