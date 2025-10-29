using ProyectoCamisetas.Models;

namespace ProyectoCamisetas.Repository
{
    public interface ICamisetasRepository
    {
        Task<IReadOnlyList<Camiseta>> GetAllAsync(string? q, string? liga, string? equipo, string? temporada, CancellationToken ct = default);
        Task<IReadOnlyList<Camiseta>> GetDestacadasAsync(string? q, string? liga, string? equipo, string? temporada, int take = 12, CancellationToken ct = default);
        Task<IReadOnlyList<Camiseta>> GetAllAdminAsync(CancellationToken ct = default);
        Task<Camiseta?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<Camiseta>> GetRandomAsync(int take = 12, bool onlyAvailable = true, CancellationToken ct = default);
        Task<Camiseta?> GetHomeFeaturedAsync(CancellationToken ct = default);

        Task<Camiseta> AddAsync(Camiseta entity, CancellationToken ct = default);
        Task UpdateAsync(Camiseta entity, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
        Task SetImagesAsync(int camisetaId, IEnumerable<string> urls, CancellationToken ct = default);
        Task SetTallesAsync(int camisetaId, IEnumerable<(Talla talla, int cantidad)> talles, CancellationToken ct = default);
        Task<bool> RegisterSaleAsync(int camisetaId, Talla talla, CancellationToken ct = default);

        Task SetHomeFeaturedAsync(int camisetaId, CancellationToken ct = default);
    }
}
