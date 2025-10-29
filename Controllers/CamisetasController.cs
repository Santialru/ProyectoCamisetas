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
        public async Task<IActionResult> Index(string? q, string? liga, string? equipo, string? temporada, bool? enStock, CancellationToken ct)
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
