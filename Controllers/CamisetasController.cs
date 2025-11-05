using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoCamisetas.Data;
using ProyectoCamisetas.Models;
using ProyectoCamisetas.ViewModels;
using ProyectoCamisetas.Repository;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ProyectoCamisetas.Controllers
{
    // CatÃ¡logo pÃºblico de camisetas
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class CamisetasController : Controller
    {
        private readonly ICamisetasRepository _repo;

        public CamisetasController(ICamisetasRepository repo)
        { _repo = repo; }

        [HttpGet]
        public async Task<IActionResult> Index(string? q, string? liga, string? equipo, string? temporada, bool? enStock, string? version, string? other, string? talla, string? sort, string? producto,
 int page = 1, CancellationToken ct = default)
        {
            // Normaliza alias de selecciones/equipos (ej: "seleccion argentina", "afa") a un nombre canónico
            var equipoCanon = NormalizeEquipoAlias(equipo ?? q);
            if (!string.IsNullOrWhiteSpace(equipoCanon))
            {
                equipo = equipoCanon;
                q = null;
            }

            var destacadas = await _repo.GetDestacadasAsync(q, liga, equipo, temporada, 12, ct);
            var camisetas = await _repo.GetAllAsync(q, liga, equipo, temporada, ct);
            // PÃ¡gina inicial paginada (9 items) para scroll infinito
                        // Filtro por tipo de producto (camiseta, campera, short, conjunto)
            var prodCanon = NormalizeProductoAlias(producto);
            if (!string.IsNullOrWhiteSpace(prodCanon))
            {
                Func<Camiseta, bool> prodPredicate = c => {
                    var name = ToSearchKey(c.Nombre ?? string.Empty);
                    var desc = ToSearchKey(c.Descripcion ?? string.Empty);
                    return ProductoMatches(prodCanon!, name, desc);
                };
                destacadas = destacadas.Where(prodPredicate).ToList();
                camisetas = camisetas.Where(prodPredicate).ToList();
                ViewBag.Producto = prodCanon;
            }            // Filtro de disponibilidad si se solicita
            if (enStock.HasValue)
            {
                if (enStock.Value)
                {
                    destacadas = destacadas.Where(c => c.EnStock).ToList();
                    camisetas = camisetas.Where(c => c.EnStock).ToList();
                    ViewBag.EnStock = true;
                }
                else
                {
                    destacadas = destacadas.Where(c => !c.EnStock).ToList();
                    camisetas = camisetas.Where(c => !c.EnStock).ToList();
                    ViewBag.EnStock = false;
                }
            }

            // Filtro por versiÃ³n (Aficionado/Jugador/Retro)
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
                    else if (vNorm is "retro" or "clasica" or "clÃ¡sica") v = VersionCamiseta.Retro;
                }
                if (v.HasValue)
                {
                    destacadas = destacadas.Where(c => c.Version == v.Value).ToList();
                    camisetas = camisetas.Where(c => c.Version == v.Value).ToList();
                    ViewBag.VersionFiltro = v.Value;
                }
            }

            // Filtro "Otro" en catÃ¡logo (club/seleccion/liga)
            if (!string.IsNullOrWhiteSpace(other))
            {
                var type = other.Trim().ToLowerInvariant();
                var knownClubs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { "River", "Boca", "Barcelona", "Real Madrid", "Chelsea", "Manchester United" };
                var knownNations = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { "Argentina", "Brasil", "EspaÃ±a", "Espana", "Portugal", "Francia" };
                var knownLeagues = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { "La Liga", "Premier League", "Serie A", "Bundesliga", "Ligue 1" };

                Func<Camiseta, bool> predicate = type switch
                {
                    "club" => c => !knownClubs.Contains(c.Equipo ?? string.Empty),
                    "seleccion" or "selecciÃ³n" => c => !knownNations.Contains(c.Equipo ?? string.Empty),
                    "liga" => c => !knownLeagues.Contains(c.Liga ?? string.Empty),
                    _ => c => true
                };
                destacadas = destacadas.Where(predicate).ToList();
                camisetas = camisetas.Where(predicate).ToList();
                ViewBag.Other = type;
            }

            // Filtro por talla (XS, S, M, L, XL, XXL, Niños)
            if (!string.IsNullOrWhiteSpace(talla))
            {
                Talla? tallaEnum = null;
                var tNorm = talla.Trim().ToLowerInvariant();
                if (Enum.TryParse<Talla>(talla, ignoreCase: true, out var tParsed))
                {
                    tallaEnum = tParsed;
                }
                else
                {
                    if (tNorm is "niños" or "ninos") tallaEnum = Talla.Ninos;
                    else if (tNorm == "xs") tallaEnum = Talla.XS;
                    else if (tNorm == "s") tallaEnum = Talla.S;
                    else if (tNorm == "m") tallaEnum = Talla.M;
                    else if (tNorm == "l") tallaEnum = Talla.L;
                    else if (tNorm == "xl") tallaEnum = Talla.XL;
                    else if (tNorm == "xxl") tallaEnum = Talla.XXL;
                }
                if (tallaEnum.HasValue)
                {
                    Func<Camiseta, bool> tallaPredicate = c => (c.TallesStock != null && c.TallesStock.Any(ts => ts.Talla == tallaEnum.Value));
                    destacadas = destacadas.Where(tallaPredicate).ToList();
                    camisetas = camisetas.Where(tallaPredicate).ToList();
                    ViewBag.TallaFiltro = tallaEnum.Value;
                }
            }

            // Priorizar en stock en memoria usando la propiedad EnStock
            destacadas = destacadas
                .Where(c => c.EnStock)
                .OrderByDescending(c => c.EnStock)
                .ThenBy(c => c.Equipo)
                .ThenBy(c => c.Temporada)
                .ToList();
            if (!string.IsNullOrWhiteSpace(sort))
            {
                var sNorm = sort.Trim().ToLowerInvariant();
                if (sNorm == "price_asc")
                {
                    camisetas = camisetas
                        .OrderBy(c => c.Precio)
                        .ThenBy(c => c.Equipo)
                        .ThenBy(c => c.Temporada)
                        .ToList();
                    ViewBag.Sort = "price_asc";
                }
                else if (sNorm == "price_desc")
                {
                    camisetas = camisetas
                        .OrderByDescending(c => c.Precio)
                        .ThenBy(c => c.Equipo)
                        .ThenBy(c => c.Temporada)
                        .ToList();
                    ViewBag.Sort = "price_desc";
                }
                else
                {
                    camisetas = camisetas
                        .OrderByDescending(c => c.EnStock)
                        .ThenBy(c => c.Equipo)
                        .ThenBy(c => c.Temporada)
                        .ThenBy(c => c.Tipo)
                        .ToList();
                }
            }
            else
            {
                camisetas = camisetas
                    .OrderByDescending(c => c.EnStock)
                    .ThenBy(c => c.Equipo)
                    .ThenBy(c => c.Temporada)
                    .ThenBy(c => c.Tipo)
                    .ToList();
            }

            // Paginación (9 por página)
            const int pageSize = 9;
            if (page < 1) page = 1;
            var totalCount = camisetas.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            if (page > 0 && totalPages > 0 && page > totalPages) page = totalPages;

            var paged = camisetas
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var vm = new CatalogoViewModel
            {
                Destacadas = destacadas,
                Camisetas = paged,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
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

        private static string? NormalizeEquipoAlias(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            string key = ToSearchKey(raw);
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // Argentina
                ["argentina"] = "Argentina",
                ["seleccion argentina"] = "Argentina",
                ["seleccion de argentina"] = "Argentina",
                ["la seleccion argentina"] = "Argentina",
                ["seleccion nacional argentina"] = "Argentina",
                ["AFA"] = "Argentina",
                ["afa"] = "Argentina",
                ["albiceleste"] = "Argentina",
                ["la albiceleste"] = "Argentina",


                // Brasil
                ["brasil"] = "Brasil",
                ["seleccion brasil"] = "Brasil",
                ["seleccion de brasil"] = "Brasil",
                ["seleccion brasilera"] = "Brasil",
                ["seleccion de brasilera"] = "Brasil",

                // España
                ["espana"] = "España",
                ["seleccion espana"] = "España",
                ["seleccion de espana"] = "España",
                ["españa"] = "España",
                ["seleccion españa"] = "España",
                ["seleccion de españa"] = "España",
                ["espania"] = "España",
                ["seleccion espania"] = "España",
                ["seleccion de espania"] = "España",


                // Portugal
                ["portugal"] = "Portugal",
                ["seleccion portugal"] = "Portugal",
                ["seleccion de portugal"] = "Portugal",


                // Francia
                ["francia"] = "Francia",
                ["seleccion francia"] = "Francia",
                ["seleccion de francia"] = "Francia",

            };
            return map.TryGetValue(key, out var canon) ? canon : null;
        }

        private static string ToSearchKey(string s)
        {
            s = (s ?? string.Empty).Trim().ToLowerInvariant();
            var nf = s.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in nf)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != UnicodeCategory.NonSpacingMark) sb.Append(c);
            }
            var noAccents = sb.ToString().Normalize(NormalizationForm.FormC);
            var cleaned = Regex.Replace(noAccents, @"[^a-z0-9]+", " ").Trim();
            cleaned = Regex.Replace(cleaned, @"\s+", " ");
            return cleaned;
        }

        private static string? NormalizeProductoAlias(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            var key = ToSearchKey(raw);
            if (key == "camiseta" || key == "remera" || key == "jersey") return "camiseta";
            if (key == "campera" || key == "chaqueta" || key == "buzo" || key == "abrigo" || key == "jacket" || key == "hoodie") return "campera";
            if (key == "short" || key == "pantalon corto" || key == "pantaloncorto" || key == "pantalon-corto") return "short";
            if (key == "conjunto" || key == "set" || key == "kit") return "conjunto";
            return null;
        }

        private static bool ProductoMatches(string prodCanon, string nameKey, string descKey)
        {
            bool HasAny(params string[] tokens)
            {
                foreach (var t in tokens)
                {
                    if ((nameKey?.Contains(t) ?? false) || (descKey?.Contains(t) ?? false)) return true;
                }
                return false;
            }

            switch (prodCanon)
            {
                case "camiseta": return HasAny("camiseta", "remera", "jersey");
                case "campera":  return HasAny("campera", "chaqueta", "buzo", "abrigo", "hoodie", "jacket");
                case "short":    return HasAny("short", "pantalon corto", "pantaloncorto", "pantalon-corto");
                case "conjunto": return HasAny("conjunto", "set", "kit");
                default: return true;
            }
        }

    }
}




