using ProyectoCamisetas.Models;

namespace ProyectoCamisetas.ViewModels
{
    public class CatalogoViewModel
    {
        public IReadOnlyList<Camiseta> Destacadas { get; init; } = Array.Empty<Camiseta>();
        public IReadOnlyList<Camiseta> Camisetas { get; init; } = Array.Empty<Camiseta>();
    }
}

