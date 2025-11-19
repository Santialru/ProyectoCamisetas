using System.Collections.Generic;

namespace ProyectoCamisetas.Models
{
    public class CrearVentaViewModel
    {
        public string? Comprador { get; set; }
        public string? Observaciones { get; set; }
        public List<Camiseta> Disponibles { get; set; } = new List<Camiseta>();
        public List<Linea> Lineas { get; set; } = new List<Linea>();
        public int? PreselectId { get; set; }
        public Talla? PreselectTalla { get; set; }

        public class Linea
        {
            public int CamisetaId { get; set; }
            public Talla Talla { get; set; }
        }
    }
}
