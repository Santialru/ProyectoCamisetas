using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        private readonly ILogger<AdminCamisetasController> _logger;

        public AdminCamisetasController(
            ICamisetasRepository repo,
            IWebHostEnvironment env,
            ILogger<AdminCamisetasController> logger)
        {
            _repo = repo;
            _env = env;
            _logger = logger;
        }

        // --------------------------- INDEX ---------------------------

        [HttpGet]
        public async Task<IActionResult> Index(
            string? q,
            string? version,
            string? equipo,
            string? temporada,
            bool? enStock,
            string? talla,
            string? sort,
            string? producto,
            string? orden,
            int page = 1,
            int pageSize = 10,
            CancellationToken ct = default)
        {
            var list = await _repo.GetAllAdminAsync(ct);

            // Búsqueda libre
            if (!string.IsNullOrWhiteSpace(q))
            {
                var tokens = q.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                              .Select(t => t.ToLowerInvariant())
                              .ToList();

                bool MatchAll(Camiseta c)
                {
                    var pool = string.Join(' ',
                                  new[] { c.Nombre, c.Equipo, c.Liga, c.Temporada, c.Marca, c.Descripcion }
                                  .Where(s => !string.IsNullOrWhiteSpace(s)))
                                  .ToLowerInvariant();
                    return tokens.All(t => pool.Contains(t));
                }

                list = list.Where(MatchAll).ToList();
            }

            // Filtros
            if (!string.IsNullOrWhiteSpace(equipo))
                list = list.Where(c => c.Equipo == equipo).ToList();

            if (!string.IsNullOrWhiteSpace(temporada))
                list = list.Where(c => c.Temporada == temporada).ToList();

            // Versión
            if (!string.IsNullOrWhiteSpace(version))
            {
                var v = version.Trim().ToLowerInvariant();
                if (v is "aficionado" or "stadium")
                    list = list.Where(c => c.Version == VersionCamiseta.Aficionado).ToList();
                else if (v is "jugador" or "player" or "match")
                    list = list.Where(c => c.Version == VersionCamiseta.Jugador).ToList();
                else if (v is "retro" or "clasica" or "clásica")
                    list = list.Where(c => c.Version == VersionCamiseta.Retro).ToList();
            }

            // Disponibilidad
            if (enStock.HasValue)
                list = enStock.Value ? list.Where(c => c.EnStock).ToList()
                                     : list.Where(c => !c.EnStock).ToList();

            // Talla
            if (!string.IsNullOrWhiteSpace(talla)
                && Enum.TryParse<Talla>(talla, ignoreCase: true, out var tParsed))
            {
                list = list.Where(c => c.TallesStock != null &&
                                       c.TallesStock.Any(ts => ts.Talla == tParsed && ts.Cantidad > 0))
                           .ToList();
            }

            // Producto
            var prod = NormalizeProductoAlias(producto);
            if (!string.IsNullOrWhiteSpace(prod))
            {
                bool ProdMatch(Camiseta c)
                {
                    var name = (c.Nombre ?? string.Empty).ToLowerInvariant();
                    var desc = (c.Descripcion ?? string.Empty).ToLowerInvariant();
                    return ProductoMatches(prod!, name, desc);
                }

                list = list.Where(ProdMatch).ToList();
            }

            // Orden
            var ord = string.IsNullOrWhiteSpace(sort)
                        ? (orden ?? string.Empty).ToLowerInvariant()
                        : sort.ToLowerInvariant().Replace("price_", "precio_");

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

        // ---------------------- FEATURE HOME (GET) -------------------

        [HttpGet]
        public async Task<IActionResult> FeatureHome(string? q, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var list = await _repo.GetAllAdminAsync(ct);
            // Búsqueda libre similar a Index
            if (!string.IsNullOrWhiteSpace(q))
            {
                var tokens = q.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                              .Select(t => t.ToLowerInvariant())
                              .ToList();

                bool MatchAll(Camiseta c)
                {
                    var pool = string.Join(' ',
                                  new[] { c.Nombre, c.Equipo, c.Liga, c.Temporada, c.Marca, c.Descripcion }
                                  .Where(s => !string.IsNullOrWhiteSpace(s)))
                                  .ToLowerInvariant();
                    return tokens.All(t => pool.Contains(t));
                }

                list = list.Where(MatchAll).ToList();
            }
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
            ViewBag.Q = q;
            ViewBag.CarouselSlides = await _repo.GetHomeCarouselSlidesAsync(ct);

            return View(items);
        }

        // ---------------------- FEATURE HOME (POST) ------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FeatureHome(int camisetaId, CancellationToken ct = default)
        {
            await _repo.SetHomeFeaturedAsync(camisetaId, ct);
            TempData["Success"] = "Inicio actualizado.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateHomeGrid(CancellationToken ct = default)
        {
            var ids = Request.Form["SelectedIds"].ToArray();
            var picks = new List<(int camisetaId, short orden)>();

            foreach (var s in ids)
            {
                if (int.TryParse(s, out var id))
                {
                    var ordStr = Request.Form[$"Order_{id}"].FirstOrDefault();
                    if (short.TryParse(ordStr, out var ord) && ord >= 1 && ord <= 3)
                        picks.Add((id, ord));
                }
            }

            await _repo.SetHomeFeaturedGridAsync(picks, ct);
            TempData["Success"] = "Tarjetas de inicio actualizadas.";
            return RedirectToAction(nameof(FeatureHome));
        }

        // ---------------------- HOME CAROUSEL ------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveHomeCarousel(
            IFormFile? Slide1File, IFormFile? Slide2File, IFormFile? Slide3File,
            [FromForm] string? Slide1Url, [FromForm] string? Slide2Url, [FromForm] string? Slide3Url,
            [FromForm] string? Slide1Title, [FromForm] string? Slide1Desc, [FromForm] string? Slide1BtnText, [FromForm] string? Slide1BtnUrl,
            [FromForm] string? Slide2Title, [FromForm] string? Slide2Desc, [FromForm] string? Slide2BtnText, [FromForm] string? Slide2BtnUrl,
            [FromForm] string? Slide3Title, [FromForm] string? Slide3Desc, [FromForm] string? Slide3BtnText, [FromForm] string? Slide3BtnUrl,
            [FromForm] bool? Slide1Delete, [FromForm] bool? Slide2Delete, [FromForm] bool? Slide3Delete,
            CancellationToken ct = default)
        {
            async Task<string> SaveFileAsync(IFormFile f, CancellationToken token)
            {
                var baseRoot = !string.IsNullOrWhiteSpace(_env.WebRootPath)
                    ? _env.WebRootPath!
                    : Path.Combine(_env.ContentRootPath ?? Directory.GetCurrentDirectory(), "wwwroot");

                var dir = Path.Combine(baseRoot, "uploads", "hero");
                Directory.CreateDirectory(dir);

                var ext = Path.GetExtension(f.FileName);
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".avif" };
                if (!allowed.Contains(ext.ToLowerInvariant())) ext = ".jpg";

                var fileName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}{ext}";
                var fullPath = Path.Combine(dir, fileName);

                await using (var stream = System.IO.File.Create(fullPath))
                    await f.CopyToAsync(stream, token);

                return $"/uploads/hero/{fileName}";
            }

            var existing = await _repo.GetHomeCarouselSlidesAsync(ct);
            string existing1 = existing.FirstOrDefault(s => s.Orden == 1)?.ImageUrl ?? string.Empty;
            string existing2 = existing.FirstOrDefault(s => s.Orden == 2)?.ImageUrl ?? string.Empty;
            string existing3 = existing.FirstOrDefault(s => s.Orden == 3)?.ImageUrl ?? string.Empty;

            string s1 = existing1;
            string s2 = existing2;
            string s3 = existing3;

            if (!string.IsNullOrWhiteSpace(Slide1Url)) s1 = Slide1Url.Trim();
            if (!string.IsNullOrWhiteSpace(Slide2Url)) s2 = Slide2Url.Trim();
            if (!string.IsNullOrWhiteSpace(Slide3Url)) s3 = Slide3Url.Trim();

            if (Slide1File != null && Slide1File.Length > 0) s1 = await SaveFileAsync(Slide1File, ct);
            if (Slide2File != null && Slide2File.Length > 0) s2 = await SaveFileAsync(Slide2File, ct);
            if (Slide3File != null && Slide3File.Length > 0) s3 = await SaveFileAsync(Slide3File, ct);

            if (string.IsNullOrWhiteSpace(s1) && (!string.IsNullOrWhiteSpace(Slide1Title) || !string.IsNullOrWhiteSpace(Slide1Desc) || !string.IsNullOrWhiteSpace(Slide1BtnText) || !string.IsNullOrWhiteSpace(Slide1BtnUrl)))
                s1 = "/img/Diseño sin título.png";
            if (string.IsNullOrWhiteSpace(s2) && (!string.IsNullOrWhiteSpace(Slide2Title) || !string.IsNullOrWhiteSpace(Slide2Desc) || !string.IsNullOrWhiteSpace(Slide2BtnText) || !string.IsNullOrWhiteSpace(Slide2BtnUrl)))
                s2 = "/img/Diseño sin título.png";
            if (string.IsNullOrWhiteSpace(s3) && (!string.IsNullOrWhiteSpace(Slide3Title) || !string.IsNullOrWhiteSpace(Slide3Desc) || !string.IsNullOrWhiteSpace(Slide3BtnText) || !string.IsNullOrWhiteSpace(Slide3BtnUrl)))
                s3 = "/img/Diseño sin título.png";

            _logger.LogInformation("SaveHomeCarousel prepared URLs s1={s1} s2={s2} s3={s3}");

            // Eliminar si se solicitó
            if (Slide1Delete == true) { s1 = string.Empty; Slide1Title = Slide1Desc = Slide1BtnText = Slide1BtnUrl = null; }
            if (Slide2Delete == true) { s2 = string.Empty; Slide2Title = Slide2Desc = Slide2BtnText = Slide2BtnUrl = null; }
            if (Slide3Delete == true) { s3 = string.Empty; Slide3Title = Slide3Desc = Slide3BtnText = Slide3BtnUrl = null; }

            var slides = new List<HomeCarouselSlide>
            {
                new HomeCarouselSlide { Orden = 1, ImageUrl = s1, Title = Slide1Title, Description = Slide1Desc, ButtonText = Slide1BtnText, ButtonUrl = Slide1BtnUrl },
                new HomeCarouselSlide { Orden = 2, ImageUrl = s2, Title = Slide2Title, Description = Slide2Desc, ButtonText = Slide2BtnText, ButtonUrl = Slide2BtnUrl },
                new HomeCarouselSlide { Orden = 3, ImageUrl = s3, Title = Slide3Title, Description = Slide3Desc, ButtonText = Slide3BtnText, ButtonUrl = Slide3BtnUrl }
            };

            var toPersist = slides.Where(x => !string.IsNullOrWhiteSpace(x.ImageUrl)).ToList();
            if (toPersist.Count == 0)
            {
                TempData["Error"] = "Debe quedar al menos un slide visible.";
                return RedirectToAction(nameof(FeatureHome));
            }

            // Si hay texto/carga nueva, asegurar imagen placeholder
            if ((Slide1File != null && Slide1File.Length > 0) ||
                (Slide2File != null && Slide2File.Length > 0) ||
                (Slide3File != null && Slide3File.Length > 0) ||
                !string.IsNullOrWhiteSpace(Slide1Title) || !string.IsNullOrWhiteSpace(Slide1Desc) || !string.IsNullOrWhiteSpace(Slide1BtnText) || !string.IsNullOrWhiteSpace(Slide1BtnUrl) ||
                !string.IsNullOrWhiteSpace(Slide2Title) || !string.IsNullOrWhiteSpace(Slide2Desc) || !string.IsNullOrWhiteSpace(Slide2BtnText) || !string.IsNullOrWhiteSpace(Slide2BtnUrl) ||
                !string.IsNullOrWhiteSpace(Slide3Title) || !string.IsNullOrWhiteSpace(Slide3Desc) || !string.IsNullOrWhiteSpace(Slide3BtnText) || !string.IsNullOrWhiteSpace(Slide3BtnUrl))
            {
                foreach (var sld in slides)
                    if (string.IsNullOrWhiteSpace(sld.ImageUrl))
                        sld.ImageUrl = "/img/Diseño sin título.png";
            }

            await _repo.SaveHomeCarouselSlidesAsync(toPersist, ct);

            var url1 = toPersist.FirstOrDefault(s => s.Orden == 1)?.ImageUrl;
            var url2 = toPersist.FirstOrDefault(s => s.Orden == 2)?.ImageUrl;
            var url3 = toPersist.FirstOrDefault(s => s.Orden == 3)?.ImageUrl;

            await _repo.SaveHomeCarouselAsync(new HomeCarouselConfig
            {
                Slide1Url = string.IsNullOrWhiteSpace(url1) ? null : url1,
                Slide2Url = string.IsNullOrWhiteSpace(url2) ? null : url2,
                Slide3Url = string.IsNullOrWhiteSpace(url3) ? null : url3,
                Title = !string.IsNullOrWhiteSpace(Slide1Title) ? Slide1Title
                      : (!string.IsNullOrWhiteSpace(Slide2Title) ? Slide2Title : Slide3Title),
                Description = !string.IsNullOrWhiteSpace(Slide1Desc) ? Slide1Desc
                            : (!string.IsNullOrWhiteSpace(Slide2Desc) ? Slide2Desc : Slide3Desc),
                ButtonText = !string.IsNullOrWhiteSpace(Slide1BtnText) ? Slide1BtnText
                           : (!string.IsNullOrWhiteSpace(Slide2BtnText) ? Slide2BtnText : Slide3BtnText),
                ButtonUrl = !string.IsNullOrWhiteSpace(Slide1BtnUrl) ? Slide1BtnUrl
                         : (!string.IsNullOrWhiteSpace(Slide2BtnUrl) ? Slide2BtnUrl : Slide3BtnUrl),
            }, ct);

            TempData["Success"] = "Carrusel de inicio actualizado.";
            return RedirectToAction(nameof(FeatureHome));
        }

        // --------------------------- CRUD ----------------------------

        [HttpGet]
        public async Task<IActionResult> Details(int id, CancellationToken ct = default)
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
        public async Task<IActionResult> Create(Camiseta model, CancellationToken ct = default)
        {
            // Normalizar SKU opcional
            model.SKU = string.IsNullOrWhiteSpace(model.SKU) ? null : model.SKU!.Trim();

            if (!ModelState.IsValid)
            {
                PopulateSelects();
                return View(model);
            }

            await _repo.AddAsync(model, ct);

            // Imágenes
            var imgs = Request.Form["ImageUrls"].ToArray();
            await _repo.SetImagesAsync(model.Id, imgs.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s!).Take(5), ct);

            // Talles
            var talles = Request.Form["Tallas[]"].ToArray();
            var cantidades = Request.Form["Cantidades[]"].ToArray();
            var pairs = new List<(Talla talla, int cant)>();

            for (int i = 0; i < Math.Min(talles.Length, cantidades.Length); i++)
            {
                if (int.TryParse(talles[i], out var tVal) && int.TryParse(cantidades[i], out var cVal))
                    pairs.Add(((Talla)tVal, cVal));
            }

            if (pairs.Count > 0)
                await _repo.SetTallesAsync(model.Id, pairs, ct);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
        {
            var entity = await _repo.GetByIdAsync(id, ct);
            if (entity == null) return NotFound();
            PopulateSelects();
            return View(entity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Camiseta model, CancellationToken ct = default)
        {
            if (id != model.Id) return BadRequest();

            // Normalizar SKU opcional
            model.SKU = string.IsNullOrWhiteSpace(model.SKU) ? null : model.SKU!.Trim();

            if (!ModelState.IsValid)
            {
                PopulateSelects();
                return View(model);
            }

            await _repo.UpdateAsync(model, ct);

            // Imágenes
            var imgs = Request.Form["ImageUrls"].ToArray();
            await _repo.SetImagesAsync(model.Id, imgs.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s!).Take(5), ct);

            // Talles
            var talles = Request.Form["Tallas[]"].ToArray();
            var cantidades = Request.Form["Cantidades[]"].ToArray();
            var pairs = new List<(Talla talla, int cant)>();

            for (int i = 0; i < Math.Min(talles.Length, cantidades.Length); i++)
            {
                if (int.TryParse(talles[i], out var tVal) && int.TryParse(cantidades[i], out var cVal))
                    pairs.Add(((Talla)tVal, cVal));
            }

            await _repo.SetTallesAsync(model.Id, pairs, ct);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
        {
            var entity = await _repo.GetByIdAsync(id, ct);
            if (entity == null) return NotFound();
            return View(entity);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct = default)
        {
            var ok = await _repo.DeleteAsync(id, ct);
            if (!ok) return NotFound();
            return RedirectToAction(nameof(Index));
        }

        // ---------------------- PRECIOS / VENTA ----------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkPriceAdjust(decimal porcentaje, string modo, int[] ids, CancellationToken ct = default)
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

                if (!subir)
                {
                    if (!ent.PrecioAnterior.HasValue || ent.PrecioAnterior.Value <= ent.Precio)
                        ent.PrecioAnterior = ent.Precio;
                }
                else
                {
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
        public async Task<IActionResult> Venta(int id, Talla? talla, CancellationToken ct = default)
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

        // ------------------------- HELPERS ---------------------------

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
        public async Task<IActionResult> UploadImage(IFormFile file, CancellationToken ct = default)
        {
            if (file is null || file.Length == 0)
                return BadRequest("Archivo vacío");

            if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Debe ser una imagen");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".jpg", ".jpeg", ".png", ".webp", ".gif"
            };
            if (!allowed.Contains(ext))
                return BadRequest("Extensión inválida");

            var today = DateTime.UtcNow;
            var relDir = Path.Combine("uploads", today.ToString("yyyy"), today.ToString("MM"));
            var absDir = Path.Combine(_env.WebRootPath, relDir);
            Directory.CreateDirectory(absDir);

            var fileName = $"{Guid.NewGuid():N}{ext}";
            var absPath = Path.Combine(absDir, fileName);

            await using (var fs = System.IO.File.Create(absPath))
                await file.CopyToAsync(fs, ct);

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
            bool Has(params string[] tokens) =>
                tokens.Any(t => (nameKey?.Contains(t) ?? false) || (descKey?.Contains(t) ?? false));

            return prodCanon switch
            {
                "camiseta" => Has("camiseta", "remera", "jersey"),
                "campera" => Has("campera", "chaqueta", "buzo", "abrigo", "hoodie", "jacket"),
                "short" => Has("short", "pantalon corto", "pantaloncorto", "pantalon-corto"),
                "conjunto" => Has("conjunto", "set", "kit"),
                _ => true
            };
        }
    }
}
