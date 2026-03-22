using System.ComponentModel.DataAnnotations;

namespace ProyectoCamisetas.Models
{
    public class HomeFeaturedCard
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CamisetaId { get; set; }

        public Camiseta? Camiseta { get; set; }

        [Range(1, 3)]
        public short Orden { get; set; }
    }
}

