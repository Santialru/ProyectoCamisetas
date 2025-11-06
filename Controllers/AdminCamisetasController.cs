using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyectoCamisetas.Models;
using ProyectoCamisetas.Repository;

namespace ProyectoCamisetas.Controllers
{
    [Authorize(Roles = "Owner")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
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
        public async Task<IActionResult> Index(string? q, string? version, string? equipo, string? temporada, bool? enStock, string? talla, string? sort, string? producto, string? orden, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var list = await _repo.GetAllAdminAsync(ct);

            // Búsqueda libre
            if (!string.IsNullOrWhiteSpace(q))
            {
                var tokens = q.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                              .Select(t => t.ToLowerInvariant()).ToList();
                bool MatchAny(ProyectoCamisetas.Models.Camiseta c)
                {
                    var pool = string.Join(' ', new[] { c.Nombre, c.Equipo, c.Liga, c.Temporada, c.Marca, c.Descripcion }.Where(s => !string.IsNullOrWhiteSpace(s))).ToLowerInvariant();
                    return tokens.All(t => pool.Contains(t));
                }
                list = list.Where(MatchAny).ToList();
            }

            // Filtros
            if (!string.IsNullOrWhiteSpace(equipo))
            {
                list = list.Where(c => c.Equipo == equipo).ToList();
            }
            if (!string.IsNullOrWhiteSpace(temporada))
            {
                list = list.Where(c => c.Temporada == temporada).ToList();
            }
            // Version
            if (!string.IsNullOrWhiteSpace(version))
            {
                var v = version.Trim().ToLowerInvariant();
                if (v is "aficionado" or "stadium") list = list.Where(c => c.Version == ProyectoCamisetas.Models.VersionCamiseta.Aficionado).ToList();
                else if (v is "jugador" or "player" or "match") list = list.Where(c => c.Version == ProyectoCamisetas.Models.VersionCamiseta.Jugador).ToList();
                else if (v is "retro" or "clasica" or "clásica") list = list.Where(c => c.Version == ProyectoCamisetas.Models.VersionCamiseta.Retro).ToList();
            }
            // Disponibilidad
            if (enStock.HasValue)
            {
                if (enStock.Value) list = list.Where(c => c.EnStock).ToList(); else list = list.Where(c => !c.EnStock).ToList();
            }
            // Talla
            if (!string.IsNullOrWhiteSpace(talla))
            {
                if (Enum.TryParse<ProyectoCamisetas.Models.Talla>(talla, ignoreCase: true, out var tParsed))
                {
                    list = list.Where(c => c.TallesStock != null && c.TallesStock.Any(ts => ts.Talla == tParsed && ts.Cantidad > 0)).ToList();
                }
            }
            // Producto
            var prod = NormalizeProductoAlias(producto);
            if (!string.IsNullOrWhiteSpace(prod))
            {
                bool ProdMatch(ProyectoCamisetas.Models.Camiseta c)
                {
                    var name = (c.Nombre ?? string.Empty).ToLowerInvariant();
                    var desc = (c.Descripcion ?? string.Empty).ToLowerInvariant();
                    return ProductoMatches(prod!, name, desc);
                }
                list = list.Where(ProdMatch).ToList();
            }

            // Orden
            var ord = string.IsNullOrWhiteSpace(sort) ? (orden ?? string.Empty).ToLowerInvariant() : sort.ToLowerInvariant().Replace("price_", "precio_");
            switch (ord)
            {
                case "equipo_asc":
                    list = list.OrderBy(c => c.Equipo).ThenBy(c => c.Temporada).ThenBy(c => c.Tipo).ToList();
                    break;
                case "equipo_desc":
                    list = list.OrderByDescending(c => c.Equipo).ThenByDescending(c => c.Temporada).ThenBy(c => c.Tipo).ToList();
                    break;
                case "temporada_asc":
                    list = list.OrderBy(c => c.Temporada).ThenBy(c => c.Equipo).ThenBy(c => c.Tipo).ToList();
                    break;
                case "temporada_desc":
                    list = list.OrderByDescending(c => c.Temporada).ThenBy(c => c.Equipo).ThenBy(c => c.Tipo).ToList();
                    break;
                case "precio_asc":
                    list = list.OrderBy(c => c.Precio).ThenBy(c => c.Equipo).ToList();
                    break;
                case "precio_desc":
                    list = list.OrderByDescending(c => c.Precio).ThenBy(c => c.Equipo).ToList();
                    break;
                default:
                    list = list.OrderByDescending(c => c.Id).ToList();
                    break;
            }

            // Paginado
            pageSize = Math.Clamp(pageSize, 1, 100);
            var total = list.Count;
            var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
            page = Math.Clamp(page, 1, totalPages);
            var items = list.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.SelectedEquipo = equipo;
            ViewBag.SelectedTemporada = temporada;
            ViewBag.Orden = ord;
            ViewBag.Q = q;
            ViewBag.Version = version;
            ViewBag.Talla = talla;
            ViewBag.EnStock = enStock;
            ViewBag.Producto = prod;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = total;
            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> FeatureHome(int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var list = await _repo.GetAllAdminAsync(ct);
            var current = list.FirstOrDefault(c => c.DestacadaInicio)?.Id;
            var gridList = await _repo.GetHomeFeaturedGridAsync(ct);
            var grid = gridList.Select((c, idx) => new { c.Id, Orden = (short)(idx + 1) }).ToList();
            pageSize = Math.Clamp(pageSize, 1, 100);
            var total = list.Count;
            var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
            page = Math.Clamp(page, 1, totalPages);
            var items = list.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentFeaturedId = current;
            ViewBag.CurrentGrid = grid;
            ViewBag.CurrentGridMap = grid.ToDictionary(x => x.Id, x => x.Orden);
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = total;
            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FeatureHome(int camisetaId, CancellationToken ct)
        {
            await _repo.SetHomeFeaturedAsync(camisetaId, ct);
            TempData["Success"] = "Inicio actualizado.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateHomeGrid(CancellationToken ct)
        {
            var ids = Request.Form["SelectedIds"].ToArray();
            var picks = new List<(int camisetaId, short orden)>();
            foreach (var s in ids)
            {
                if (int.TryParse(s, out var id))
                {
                    var ordStr = Request.Form[$"Order_{id}"].FirstOrDefault();
                    if (short.TryParse(ordStr, out var ord) && ord >= 1 && ord <= 3)
                    {
                        picks.Add((id, ord));
                    }
                }
            }

            await _repo.SetHomeFeaturedGridAsync(picks, ct);
            TempData["Success"] = "Tarjetas de inicio actualizadas.";
            return RedirectToAction(nameof(FeatureHome));
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
            // Normalizar SKU opcional: convertir vacío/espacios a null
            model.SKU = string.IsNullOrWhiteSpace(model.SKU) ? null : model.SKU!.Trim();

            if (!ModelState.IsValid)
            {
                PopulateSelects();
                return View(model);
            }

            await _repo.AddAsync(model, ct);
            var imgs = Request.Form["ImageUrls"].ToArray();
            await _repo.SetImagesAsync(model.Id, imgs.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s!).Take(5), ct);
            // Talles
            var talles = Request.Form["Tallas[]"].ToArray();
            var cantidades = Request.Form["Cantidades[]"].ToArray();
            var pairs = new List<(Talla talla, int cant)>();
            for (int i = 0; i < Math.Min(talles.Length, cantidades.Length); i++)
            {
                if (int.TryParse(talles[i], out var tVal) && int.TryParse(cantidades[i], out var cVal))
                {
                    pairs.Add(((Talla)tVal, cVal));
                }
            }
            if (pairs.Count > 0)
            {
                await _repo.SetTallesAsync(model.Id, pairs, ct);
            }
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
            // Normalizar SKU opcional: convertir vacío/espacios a null
            model.SKU = string.IsNullOrWhiteSpace(model.SKU) ? null : model.SKU!.Trim();

            if (!ModelState.IsValid)
            {
                PopulateSelects();
                return View(model);
            }

            await _repo.UpdateAsync(model, ct);
            var imgs = Request.Form["ImageUrls"].ToArray();
            await _repo.SetImagesAsync(model.Id, imgs.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s!).Take(5), ct);
            // Talles
            var talles = Request.Form["Tallas[]"].ToArray();
            var cantidades = Request.Form["Cantidades[]"].ToArray();
            var pairs = new List<(Talla talla, int cant)>();
            for (int i = 0; i < Math.Min(talles.Length, cantidades.Length); i++)
            {
                if (int.TryParse(talles[i], out var tVal) && int.TryParse(cantidades[i], out var cVal))
                {
                    pairs.Add(((Talla)tVal, cVal));
                }
            }
            await _repo.SetTallesAsync(model.Id, pairs, ct);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkPriceAdjust(decimal porcentaje, string modo, int[] ids, CancellationToken ct)
        {
            if (ids == null || ids.Length == 0)
            {
                TempData["Error"] = "Selecciona al menos una camiseta.";
                return RedirectToAction(nameof(Index));
            }
            if (porcentaje <= 0)
            {
                TempData["Error"] = "El porcentaje debe ser mayor que 0.";
                return RedirectToAction(nameof(Index));
            }

            var factor = (double)porcentaje / 100.0;
            var subir = string.Equals(modo, "up", StringComparison.OrdinalIgnoreCase);

            foreach (var id in ids.Distinct())
            {
                var ent = await _repo.GetByIdAsync(id, ct);
                if (ent is null) continue;
                var precio = (double)ent.Precio;
                var nuevo = subir ? precio * (1.0 + factor) : precio * (1.0 - factor);
                if (nuevo < 0) nuevo = 0;
                // Si es una baja de precio, guardar precio anterior para mostrar descuento
                if (!subir)
                {
                    if (!ent.PrecioAnterior.HasValue || ent.PrecioAnterior.Value <= ent.Precio)
                    {
                        ent.PrecioAnterior = ent.Precio;
                    }
                }
                else
                {
                    // Si sube precio, limpiamos precio anterior para no mostrar descuento
                    ent.PrecioAnterior = null;
                }
                ent.Precio = (decimal)Math.Round(nuevo, 2, MidpointRounding.AwayFromZero);
                await _repo.UpdateAsync(ent, ct);
            }

            TempData["Success"] = "Precios actualizados.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Venta(int id, Talla? talla, CancellationToken ct)
        {
            var entity = await _repo.GetByIdAsync(id, ct);
            if (entity is null) return NotFound();

            if (!entity.EnStock)
            {
                TempData["Error"] = "No hay stock disponible.";
                return RedirectToAction(nameof(Index));
            }

            if (!talla.HasValue)
            {
                TempData["Error"] = "Selecciona un talle para registrar la venta.";
                return RedirectToAction(nameof(Index));
            }

            var ok = await _repo.RegisterSaleAsync(entity.Id, talla.Value, ct);
            if (!ok)
            {
                TempData["Error"] = "No hay stock en el talle seleccionado.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = $"Venta registrada (talle {talla.Value}).";
            return RedirectToAction(nameof(Index));
        }

        private void PopulateSelects()
        {
            ViewBag.Tipos = new SelectList(Enum.GetValues(typeof(TipoKit)));
            ViewBag.Versiones = new SelectList(Enum.GetValues(typeof(VersionCamiseta)));
            ViewBag.Tallas = new SelectList(Enum.GetValues(typeof(Talla)));
            ViewBag.TallasEnum = Enum.GetValues(typeof(Talla));
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
        private static string? NormalizeProductoAlias(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            var key = raw.Trim().ToLowerInvariant();
            if (key is "camiseta" or "remera" or "jersey") return "camiseta";
            if (key is "campera" or "chaqueta" or "buzo" or "abrigo" or "hoodie" or "jacket") return "campera";
            if (key is "short" or "pantalon corto" or "pantaloncorto" or "pantalon-corto") return "short";
            if (key is "conjunto" or "set" or "kit") return "conjunto";
            return null;
        }

        private static bool ProductoMatches(string prodCanon, string nameKey, string descKey)
        {
            bool Has(params string[] tokens) => tokens.Any(t => (nameKey?.Contains(t) ?? false) || (descKey?.Contains(t) ?? false));
            return prodCanon switch
            {
                "camiseta" => Has("camiseta", "remera", "jersey"),
                "campera"  => Has("campera", "chaqueta", "buzo", "abrigo", "hoodie", "jacket"),
                "short"    => Has("short", "pantalon corto", "pantaloncorto", "pantalon-corto"),
                "conjunto" => Has("conjunto", "set", "kit"),
                _ => true
            };
        }
    }
}
