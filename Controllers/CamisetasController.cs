using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoCamisetas.Data;
using ProyectoCamisetas.Models;
using ProyectoCamisetas.ViewModels;
using ProyectoCamisetas.Repository;

namespace ProyectoCamisetas.Controllers
{
    // Catálogo público de camisetas
    public class CamisetasController : Controller
    {
        private readonly ICamisetasRepository _repo;

        public CamisetasController(ICamisetasRepository repo)
        { _repo = repo; }

        [HttpGet]
        public async Task<IActionResult> Index(string? q, string? liga, string? equipo, string? temporada, CancellationToken ct)
        {
            var destacadas = await _repo.GetDestacadasAsync(q, liga, equipo, temporada, 12, ct);
            var camisetas = await _repo.GetAllAsync(q, liga, equipo, temporada, ct);

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
