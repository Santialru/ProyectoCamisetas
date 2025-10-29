using ProyectoCamisetas.Models;

namespace ProyectoCamisetas.ViewModels
{
    public class HomeIndexViewModel
    {
        public IReadOnlyList<Camiseta> Productos { get; init; } = Array.Empty<Camiseta>();
        public Camiseta? Destacada { get; init; }
    }
}

