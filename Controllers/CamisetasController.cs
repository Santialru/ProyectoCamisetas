using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoCamisetas.Data;
using ProyectoCamisetas.Models;
using ProyectoCamisetas.ViewModels;
using ProyectoCamisetas.Repository;

namespace ProyectoCamisetas.Controllers
{
    // Catálogo público de camisetas
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class CamisetasController : Controller
    {
        private readonly ICamisetasRepository _repo;

        public CamisetasController(ICamisetasRepository repo)
        { _repo = repo; }

        [HttpGet]
        public async Task<IActionResult> Index(string? q, string? liga, string? equipo, string? temporada, bool? enStock, string? version, string? other, CancellationToken ct)
        {
            var destacadas = await _repo.GetDestacadasAsync(q, liga, equipo, temporada, 12, ct);
            var camisetas = await _repo.GetAllAsync(q, liga, equipo, temporada, ct);

            // Filtro de disponibilidad si se solicita
            if (enStock == true)
            {
                destacadas = destacadas.Where(c => c.EnStock).ToList();
                camisetas = camisetas.Where(c => c.EnStock).ToList();
                ViewBag.EnStock = true;
            }

            // Filtro por versión (Aficionado/Jugador/Retro)
            if (!string.IsNullOrWhiteSpace(version))
            {
                VersionCamiseta? v = null;
                var vNorm = version.Trim().ToLowerInvariant();
                if (int.TryParse(vNorm, out var vInt))
                {
                    if (Enum.IsDefined(typeof(VersionCamiseta), vInt)) v = (VersionCamiseta)vInt;
                }
                else
                {
                    if (vNorm is "aficionado" or "stadium") v = VersionCamiseta.Aficionado;
                    else if (vNorm is "jugador" or "player" or "match") v = VersionCamiseta.Jugador;
                    else if (vNorm is "retro" or "clasica" or "clásica") v = VersionCamiseta.Retro;
                }
                if (v.HasValue)
                {
                    destacadas = destacadas.Where(c => c.Version == v.Value).ToList();
                    camisetas = camisetas.Where(c => c.Version == v.Value).ToList();
                    ViewBag.VersionFiltro = v.Value;
                }
            }

            // Filtro "Otro" en catálogo (club/seleccion/liga)
            if (!string.IsNullOrWhiteSpace(other))
            {
                var type = other.Trim().ToLowerInvariant();
                var knownClubs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { "River", "Boca", "Barcelona", "Real Madrid", "Chelsea", "Manchester United" };
                var knownNations = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { "Argentina", "Brasil", "España", "Espana", "Portugal", "Francia" };
                var knownLeagues = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { "La Liga", "Premier League", "Serie A", "Bundesliga", "Ligue 1" };

                Func<Camiseta, bool> predicate = type switch
                {
                    "club" => c => !knownClubs.Contains(c.Equipo ?? string.Empty),
                    "seleccion" or "selección" => c => !knownNations.Contains(c.Equipo ?? string.Empty),
                    "liga" => c => !knownLeagues.Contains(c.Liga ?? string.Empty),
                    _ => c => true
                };
                destacadas = destacadas.Where(predicate).ToList();
                camisetas = camisetas.Where(predicate).ToList();
                ViewBag.Other = type;
            }

            // Priorizar en stock en memoria usando la propiedad EnStock
            destacadas = destacadas
                .Where(c => c.EnStock)
                .OrderByDescending(c => c.EnStock)
                .ThenBy(c => c.Equipo)
                .ThenBy(c => c.Temporada)
                .ToList();
            camisetas = camisetas
                .OrderByDescending(c => c.EnStock)
                .ThenBy(c => c.Equipo)
                .ThenBy(c => c.Temporada)
                .ThenBy(c => c.Tipo)
                .ToList();

            var vm = new CatalogoViewModel
            {
                Destacadas = destacadas,
                Camisetas = camisetas
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, CancellationToken ct)
        {
            var camiseta = await _repo.GetByIdAsync(id, ct);
            if (camiseta == null) return NotFound();
            return View(camiseta);
        }
    }
}
