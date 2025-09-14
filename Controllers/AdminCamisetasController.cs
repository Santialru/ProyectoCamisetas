using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyectoCamisetas.Models;
using ProyectoCamisetas.Repository;

namespace ProyectoCamisetas.Controllers
{
    [Authorize(Roles = "Owner")]
    public class AdminCamisetasController : Controller
    {
        private readonly ICamisetasRepository _repo;
        private readonly IWebHostEnvironment _env;

        public AdminCamisetasController(ICamisetasRepository repo, IWebHostEnvironment env)
        {
            _repo = repo;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var list = await _repo.GetAllAdminAsync(ct);
            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, CancellationToken ct)
        {
            var entity = await _repo.GetByIdAsync(id, ct);
            if (entity == null) return NotFound();
            return View(entity);
        }

        [HttpGet]
        public IActionResult Create()
        {
            PopulateSelects();
            return View(new Camiseta());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Camiseta model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                PopulateSelects();
                return View(model);
            }

            await _repo.AddAsync(model, ct);
            var imgs = Request.Form["ImageUrls"].ToArray();
            await _repo.SetImagesAsync(model.Id, imgs.Where(s => !string.IsNullOrWhiteSpace(s))!.Select(s => s)!.Take(5), ct);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var entity = await _repo.GetByIdAsync(id, ct);
            if (entity == null) return NotFound();
            PopulateSelects();
            return View(entity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Camiseta model, CancellationToken ct)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid)
            {
                PopulateSelects();
                return View(model);
            }

            await _repo.UpdateAsync(model, ct);
            var imgs = Request.Form["ImageUrls"].ToArray();
            await _repo.SetImagesAsync(model.Id, imgs.Where(s => !string.IsNullOrWhiteSpace(s))!.Select(s => s)!.Take(5), ct);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var entity = await _repo.GetByIdAsync(id, ct);
            if (entity == null) return NotFound();
            return View(entity);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
        {
            var ok = await _repo.DeleteAsync(id, ct);
            if (!ok) return NotFound();
            return RedirectToAction(nameof(Index));
        }

        private void PopulateSelects()
        {
            ViewBag.Tipos = new SelectList(Enum.GetValues(typeof(TipoKit)));
            ViewBag.Versiones = new SelectList(Enum.GetValues(typeof(VersionCamiseta)));
            ViewBag.Tallas = new SelectList(Enum.GetValues(typeof(Talla)));
            ViewBag.Mangas = new SelectList(Enum.GetValues(typeof(Manga)));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadImage(IFormFile file, CancellationToken ct)
        {
            if (file is null || file.Length == 0)
                return BadRequest("Archivo vacío");
            if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Debe ser una imagen");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            if (!allowed.Contains(ext))
                return BadRequest("Extensión inválida");

            var today = DateTime.UtcNow;
            var relDir = Path.Combine("uploads", today.ToString("yyyy"), today.ToString("MM"));
            var absDir = Path.Combine(_env.WebRootPath, relDir);
            Directory.CreateDirectory(absDir);

            var fileName = $"{Guid.NewGuid():N}{ext}";
            var absPath = Path.Combine(absDir, fileName);
            await using (var fs = System.IO.File.Create(absPath))
            {
                await file.CopyToAsync(fs, ct);
            }
            var url = "/" + Path.Combine(relDir, fileName).Replace("\\", "/");
            return Ok(new { url });
        }
    }
}
