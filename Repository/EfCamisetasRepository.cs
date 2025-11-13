using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using ProyectoCamisetas.Data;
using ProyectoCamisetas.Models;
using System.Linq.Expressions;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using System.Collections.Generic;
using System.Text.Json;

namespace ProyectoCamisetas.Repository
{
    public class EfCamisetasRepository : ICamisetasRepository
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public EfCamisetasRepository(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public async Task<IReadOnlyList<Camiseta>> GetAllAsync(string? q, string? liga, string? equipo, string? temporada, CancellationToken ct = default)
        {
            IQueryable<Camiseta> query = _db.Camisetas
                .AsNoTracking()
                .Include(c => c.Imagenes!.OrderBy(i => i.Orden))
                .Include(c => c.TallesStock);
            if (!string.IsNullOrWhiteSpace(q))
            {
                var tokens = BuildSearchTokens(q);
                var predicate = BuildSearchPredicate(tokens);
                query = query.Where(predicate);

                var s = q.Trim();
                query = query
                    .OrderByDescending(c => EF.Functions.ILike(c.Equipo!, "%" + s + "%"))
                    .ThenByDescending(c => EF.Functions.ILike(c.Nombre!, "%" + s + "%"))
                    .ThenByDescending(c => EF.Functions.ILike(c.Liga!, "%" + s + "%"))
                    .ThenByDescending(c => c.Stock > 0)
                    .ThenBy(c => c.Equipo)
                    .ThenBy(c => c.Temporada)
                    .ThenBy(c => c.Tipo);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(liga)) query = query.Where(c => c.Liga == liga);
                if (!string.IsNullOrWhiteSpace(equipo)) query = query.Where(c => c.Equipo == equipo);
                if (!string.IsNullOrWhiteSpace(temporada)) query = query.Where(c => c.Temporada == temporada);

                query = query
                    .OrderByDescending(c => c.Stock > 0)
                    .ThenBy(c => c.Equipo)
                    .ThenBy(c => c.Temporada)
                    .ThenBy(c => c.Tipo);
            }

            return await query.ToListAsync(ct);
        }

        public async Task<IReadOnlyList<Camiseta>> GetDestacadasAsync(string? q, string? liga, string? equipo, string? temporada, int take = 12, CancellationToken ct = default)
        {
            IQueryable<Camiseta> query = _db.Camisetas
                .AsNoTracking()
                .Include(c => c.Imagenes!.OrderBy(i => i.Orden))
                .Include(c => c.TallesStock);
            if (!string.IsNullOrWhiteSpace(q))
            {
                var tokens = BuildSearchTokens(q);
                var predicate = BuildSearchPredicate(tokens);
                query = query.Where(predicate);

                var s = q.Trim();
                query = query
                    .OrderByDescending(c => EF.Functions.ILike(c.Equipo!, "%" + s + "%"))
                    .ThenByDescending(c => EF.Functions.ILike(c.Nombre!, "%" + s + "%"))
                    .ThenByDescending(c => EF.Functions.ILike(c.Liga!, "%" + s + "%"))
                    .ThenByDescending(c => c.EsEdicionLimitada)
                    .ThenByDescending(c => c.Precio);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(liga)) query = query.Where(c => c.Liga == liga);
                if (!string.IsNullOrWhiteSpace(equipo)) query = query.Where(c => c.Equipo == equipo);
                if (!string.IsNullOrWhiteSpace(temporada)) query = query.Where(c => c.Temporada == temporada);

                query = query
                    .OrderByDescending(c => c.EsEdicionLimitada)
                    .ThenByDescending(c => c.Precio);
            }

            return await query
                .Take(take)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<Camiseta>> GetAllAdminAsync(CancellationToken ct = default)
        {
            // Include Imagenes to allow admin lists to render first photo thumbnails
            return await _db.Camisetas.AsNoTracking()
                .Include(c => c.Imagenes!.OrderBy(i => i.Orden))
                .Include(c => c.TallesStock)
                .OrderByDescending(c => c.Id)
                .ToListAsync(ct);
        }

        public async Task<Camiseta?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.Camisetas
                .AsNoTracking()
                .Include(c => c.Imagenes!.OrderBy(i => i.Orden))
                .Include(c => c.TallesStock)
                .FirstOrDefaultAsync(c => c.Id == id, ct);
        }

        public async Task<IReadOnlyList<Camiseta>> GetRandomAsync(int take = 12, bool onlyAvailable = true, CancellationToken ct = default)
        {
            IQueryable<Camiseta> q = _db.Camisetas
                .AsNoTracking()
                .Include(c => c.Imagenes!.OrderBy(i => i.Orden))
                .Include(c => c.TallesStock);
            if (onlyAvailable)
            {
                // Disponible si hay stock global o por talles
                q = q.Where(c => c.Stock > 0 || c.TallesStock!.Sum(ts => (int?)ts.Cantidad) > 0);
            }

            // PostgreSQL random ordering
            return await q.OrderBy(_ => EF.Functions.Random())
                .Take(take)
                .ToListAsync(ct);
        }

        public async Task<Camiseta?> GetHomeFeaturedAsync(CancellationToken ct = default)
        {
            return await _db.Camisetas.AsNoTracking()
                .Include(c => c.Imagenes!.OrderBy(i => i.Orden))
                .FirstOrDefaultAsync(c => c.DestacadaInicio, ct);
        }

        public async Task<IReadOnlyList<Camiseta>> GetHomeFeaturedGridAsync(CancellationToken ct = default)
        {
            var query = _db.HomeFeatured
                .AsNoTracking()
                .Include(h => h.Camiseta)!
                    .ThenInclude(c => c!.Imagenes)
                .OrderBy(h => h.Orden)
                .Take(3);

            var list = await query.ToListAsync(ct);
            return list.Select(h => h.Camiseta!).Where(c => c != null).ToList();
        }

        public async Task<Camiseta> AddAsync(Camiseta entity, CancellationToken ct = default)
        {
            _db.Camisetas.Add(entity);
            await _db.SaveChangesAsync(ct);
            return entity;
        }

        public async Task UpdateAsync(Camiseta entity, CancellationToken ct = default)
        {
            _db.Entry(entity).State = EntityState.Modified;
            await _db.SaveChangesAsync(ct);
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var entity = await _db.Camisetas.FindAsync([id], ct);
            if (entity is null) return false;
            _db.Camisetas.Remove(entity);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task SetHomeFeaturedAsync(int camisetaId, CancellationToken ct = default)
        {
            // Limpiar anteriores y marcar la nueva
            await _db.Camisetas.Where(c => c.DestacadaInicio).ExecuteUpdateAsync(s => s.SetProperty(c => c.DestacadaInicio, false), ct);

            var entity = await _db.Camisetas.FirstOrDefaultAsync(c => c.Id == camisetaId, ct);
            if (entity is null) return;
            entity.DestacadaInicio = true;
            await _db.SaveChangesAsync(ct);
        }

        public async Task SetHomeFeaturedGridAsync(IEnumerable<(int camisetaId, short orden)> featured, CancellationToken ct = default)
        {
            var normalized = featured
                .Where(f => f.camisetaId > 0 && f.orden >= 1 && f.orden <= 3)
                .GroupBy(f => f.orden)
                .Select(g => g.First())
                .OrderBy(f => f.orden)
                .Take(3)
                .ToList();

            // Limpiar y reinsertar
            var existing = await _db.HomeFeatured.ToListAsync(ct);
            if (existing.Count > 0)
            {
                _db.HomeFeatured.RemoveRange(existing);
            }

            foreach (var it in normalized)
            {
                _db.HomeFeatured.Add(new HomeFeaturedCard
                {
                    CamisetaId = it.camisetaId,
                    Orden = it.orden
                });
            }

            await _db.SaveChangesAsync(ct);
        }

        public async Task<int> RestoreDiscountsAsync(IEnumerable<int> ids, CancellationToken ct = default)
        {
            var idList = ids?.Distinct().ToList() ?? new List<int>();
            if (idList.Count == 0) return 0;
            var affected = await _db.Camisetas
                .Where(c => idList.Contains(c.Id) && c.PrecioAnterior != null)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(c => c.Precio, c => c.PrecioAnterior!.Value)
                    .SetProperty(c => c.PrecioAnterior, c => null), ct);
            return affected;
        }

        public async Task<int> RestoreAllDiscountsAsync(CancellationToken ct = default)
        {
            var affected = await _db.Camisetas
                .Where(c => c.PrecioAnterior != null)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(c => c.Precio, c => c.PrecioAnterior!.Value)
                    .SetProperty(c => c.PrecioAnterior, c => null), ct);
            return affected;
        }

        // ---------------- Home carousel (file-backed JSON) ----------------

        private string GetWebRoot()
        {
            if (!string.IsNullOrWhiteSpace(_env.WebRootPath)) return _env.WebRootPath!;
            return Path.Combine(_env.ContentRootPath ?? Directory.GetCurrentDirectory(), "wwwroot");
        }

        private string GetHeroDir()
        {
            var dir = Path.Combine(GetWebRoot(), "uploads", "hero");
            Directory.CreateDirectory(dir);
            return dir;
        }

        public async Task<IReadOnlyList<HomeCarouselSlide>> GetHomeCarouselSlidesAsync(CancellationToken ct = default)
        {
            var file = Path.Combine(GetHeroDir(), "slides.json");
            if (!System.IO.File.Exists(file)) return new List<HomeCarouselSlide>();
            try
            {
                await using var fs = System.IO.File.OpenRead(file);
                var slides = await JsonSerializer.DeserializeAsync<List<HomeCarouselSlide>>(fs, cancellationToken: ct) ?? new List<HomeCarouselSlide>();
                return slides.OrderBy(s => s.Orden).Take(3).ToList();
            }
            catch
            {
                return new List<HomeCarouselSlide>();
            }
        }

        public async Task SaveHomeCarouselSlidesAsync(IEnumerable<HomeCarouselSlide> slides, CancellationToken ct = default)
        {
            var file = Path.Combine(GetHeroDir(), "slides.json");
            var norm = slides
                .Where(s => !string.IsNullOrWhiteSpace(s.ImageUrl))
                .Select(s => new HomeCarouselSlide
                {
                    Orden = (short)(s.Orden < 1 ? 1 : (s.Orden > 3 ? 3 : s.Orden)),
                    ImageUrl = s.ImageUrl!.Trim(),
                    Title = string.IsNullOrWhiteSpace(s.Title) ? null : s.Title!.Trim(),
                    Description = string.IsNullOrWhiteSpace(s.Description) ? null : s.Description!.Trim(),
                    ButtonText = string.IsNullOrWhiteSpace(s.ButtonText) ? null : s.ButtonText!.Trim(),
                    ButtonUrl = string.IsNullOrWhiteSpace(s.ButtonUrl) ? null : s.ButtonUrl!.Trim()
                })
                .GroupBy(s => s.Orden)
                .Select(g => g.First())
                .OrderBy(s => s.Orden)
                .Take(3)
                .ToList();

            var opts = new JsonSerializerOptions { WriteIndented = true };
            await using var fs = System.IO.File.Create(file);
            await JsonSerializer.SerializeAsync(fs, norm, opts, ct);
        }

        public async Task SaveHomeCarouselAsync(HomeCarouselConfig config, CancellationToken ct = default)
        {
            var file = Path.Combine(GetHeroDir(), "config.json");
            var norm = new HomeCarouselConfig
            {
                Slide1Url = string.IsNullOrWhiteSpace(config.Slide1Url) ? null : config.Slide1Url!.Trim(),
                Slide2Url = string.IsNullOrWhiteSpace(config.Slide2Url) ? null : config.Slide2Url!.Trim(),
                Slide3Url = string.IsNullOrWhiteSpace(config.Slide3Url) ? null : config.Slide3Url!.Trim(),
                Title = string.IsNullOrWhiteSpace(config.Title) ? null : config.Title!.Trim(),
                Description = string.IsNullOrWhiteSpace(config.Description) ? null : config.Description!.Trim(),
                ButtonText = string.IsNullOrWhiteSpace(config.ButtonText) ? null : config.ButtonText!.Trim(),
                ButtonUrl = string.IsNullOrWhiteSpace(config.ButtonUrl) ? null : config.ButtonUrl!.Trim()
            };

            var opts = new JsonSerializerOptions { WriteIndented = true };
            await using var fs = System.IO.File.Create(file);
            await JsonSerializer.SerializeAsync(fs, norm, opts, ct);
        }

        public async Task SetImagesAsync(int camisetaId, IEnumerable<string> urls, CancellationToken ct = default)
        {
            var list = urls
                .Where(u => !string.IsNullOrWhiteSpace(u))
                .Select(u => u.Trim())
                .Distinct()
                .Take(5)
                .ToList();

            var existing = await _db.CamisetaImagenes.Where(i => i.CamisetaId == camisetaId).ToListAsync(ct);
            if (existing.Count > 0)
            {
                _db.CamisetaImagenes.RemoveRange(existing);
                await _db.SaveChangesAsync(ct);
            }

            if (list.Count == 0) return;

            short orden = 0;
            foreach (var url in list)
            {
                _db.CamisetaImagenes.Add(new CamisetaImagen
                {
                    CamisetaId = camisetaId,
                    Url = url,
                    Orden = orden++
                });
            }
            await _db.SaveChangesAsync(ct);
        }

        public async Task SetTallesAsync(int camisetaId, IEnumerable<(Talla talla, int cantidad)> talles, CancellationToken ct = default)
        {
            var list = talles
                .Where(t => t.cantidad >= 0)
                .GroupBy(t => t.talla)
                .Select(g => new { Talla = g.Key, Cantidad = g.Sum(x => x.cantidad) })
                .Where(x => x.Cantidad > 0)
                .ToList();

            var existing = await _db.CamisetaTalles.Where(t => t.CamisetaId == camisetaId).ToListAsync(ct);
            if (existing.Count > 0)
            {
                _db.CamisetaTalles.RemoveRange(existing);
                await _db.SaveChangesAsync(ct);
            }

            foreach (var x in list)
            {
                _db.CamisetaTalles.Add(new CamisetaTalleStock
                {
                    CamisetaId = camisetaId,
                    Talla = x.Talla,
                    Cantidad = x.Cantidad
                });
            }
            await _db.SaveChangesAsync(ct);
        }

        public async Task<bool> RegisterSaleAsync(int camisetaId, Talla talla, CancellationToken ct = default)
        {
            var ts = await _db.CamisetaTalles
                .FirstOrDefaultAsync(t => t.CamisetaId == camisetaId && t.Talla == talla, ct);
            if (ts is null || ts.Cantidad <= 0) return false;
            ts.Cantidad -= 1;
            _db.CamisetaTalles.Update(ts);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> RegisterSaleAsync(int camisetaId, Talla talla, string? comprador, string? observaciones, CancellationToken ct = default)
        {
            var ts = await _db.CamisetaTalles
                .FirstOrDefaultAsync(t => t.CamisetaId == camisetaId && t.Talla == talla, ct);
            if (ts is null || ts.Cantidad <= 0) return false;
            ts.Cantidad -= 1;
            _db.CamisetaTalles.Update(ts);

            var cam = await _db.Camisetas.AsNoTracking().FirstOrDefaultAsync(c => c.Id == camisetaId, ct);
            if (cam is null)
            {
                await _db.SaveChangesAsync(ct);
                return true;
            }

            _db.Ventas.Add(new Venta
            {
                CamisetaId = cam.Id,
                FechaVenta = DateTime.UtcNow,
                Precio = cam.Precio,
                Talla = talla,
                Comprador = string.IsNullOrWhiteSpace(comprador) ? null : comprador!.Trim(),
                Observaciones = string.IsNullOrWhiteSpace(observaciones) ? null : observaciones!.Trim(),
                ProductoNombre = cam.Nombre,
                Equipo = cam.Equipo,
                Temporada = cam.Temporada
            });

            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<(IReadOnlyList<Venta> items, int total)> GetVentasAsync(
            DateOnly? desde,
            DateOnly? hasta,
            string? equipo,
            string? temporada,
            Talla? talla,
            string? comprador,
            string? sort,
            int page,
            int pageSize,
            CancellationToken ct = default)
        {
            IQueryable<Venta> q = _db.Ventas.AsNoTracking().Include(v => v.Camiseta);
            // Rango de fechas
            if (desde.HasValue)
            {
                var d = desde.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
                q = q.Where(v => v.FechaVenta >= d);
            }
            if (hasta.HasValue)
            {
                var h = hasta.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
                q = q.Where(v => v.FechaVenta <= h);
            }

            // Filtros adicionales
            if (!string.IsNullOrWhiteSpace(equipo))
            {
                var s = equipo.Trim();
                q = q.Where(v => EF.Functions.ILike(v.Equipo!, "%" + s + "%"));
            }
            if (!string.IsNullOrWhiteSpace(temporada))
            {
                var s = temporada.Trim();
                q = q.Where(v => EF.Functions.ILike(v.Temporada!, "%" + s + "%"));
            }
            if (talla.HasValue)
            {
                var t = talla.Value;
                q = q.Where(v => v.Talla == t);
            }
            if (!string.IsNullOrWhiteSpace(comprador))
            {
                var s = comprador.Trim();
                q = q.Where(v => v.Comprador != null && EF.Functions.ILike(v.Comprador!, "%" + s + "%"));
            }

            // Ordenamiento
            sort = (sort ?? "fecha_desc").ToLowerInvariant();
            q = sort switch
            {
                "fecha_asc" => q.OrderBy(v => v.FechaVenta),
                "precio_asc" => q.OrderBy(v => v.Precio),
                "precio_desc" => q.OrderByDescending(v => v.Precio),
                "equipo_asc" => q.OrderBy(v => v.Equipo),
                "equipo_desc" => q.OrderByDescending(v => v.Equipo),
                "talla_asc" => q.OrderBy(v => v.Talla),
                "talla_desc" => q.OrderByDescending(v => v.Talla),
                "comprador_asc" => q.OrderBy(v => v.Comprador),
                "comprador_desc" => q.OrderByDescending(v => v.Comprador),
                _ => q.OrderByDescending(v => v.FechaVenta)
            };

            var total = await q.CountAsync(ct);
            pageSize = Math.Clamp(pageSize, 1, 200);
            page = Math.Max(page, 1);
            var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
            return (items, total);
        }

        public async Task<VentasSummary> GetVentasSummaryAsync(
            DateOnly? desde,
            DateOnly? hasta,
            string? equipo,
            string? temporada,
            Talla? talla,
            string? comprador,
            CancellationToken ct = default)
        {
            IQueryable<Venta> q = _db.Ventas.AsNoTracking();
            // Rango de fechas
            if (desde.HasValue)
            {
                var d = desde.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
                q = q.Where(v => v.FechaVenta >= d);
            }
            if (hasta.HasValue)
            {
                var h = hasta.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
                q = q.Where(v => v.FechaVenta <= h);
            }
            // Filtros adicionales
            if (!string.IsNullOrWhiteSpace(equipo))
            {
                var s = equipo.Trim();
                q = q.Where(v => EF.Functions.ILike(v.Equipo!, "%" + s + "%"));
            }
            if (!string.IsNullOrWhiteSpace(temporada))
            {
                var s = temporada.Trim();
                q = q.Where(v => EF.Functions.ILike(v.Temporada!, "%" + s + "%"));
            }
            if (talla.HasValue)
            {
                var t = talla.Value;
                q = q.Where(v => v.Talla == t);
            }
            if (!string.IsNullOrWhiteSpace(comprador))
            {
                var s = comprador.Trim();
                q = q.Where(v => v.Comprador != null && EF.Functions.ILike(v.Comprador!, "%" + s + "%"));
            }

            var totalRecaudado = await q.SumAsync(v => (decimal?)v.Precio, ct) ?? 0m;
            var cantidad = await q.CountAsync(ct);
            var precioMax = await q.MaxAsync(v => (decimal?)v.Precio, ct) ?? 0m;
            var precioMin = await q.MinAsync(v => (decimal?)v.Precio, ct) ?? 0m;

            var topTalles = await q.GroupBy(v => v.Talla)
                                   .Select(g => new { Talla = g.Key, Cant = g.Count() })
                                   .OrderByDescending(x => x.Cant).Take(5).ToListAsync(ct);
            var topEquipos = await q.GroupBy(v => v.Equipo ?? "")
                                    .Select(g => new { Eq = g.Key, Cant = g.Count() })
                                    .OrderByDescending(x => x.Cant).Take(5).ToListAsync(ct);
            var porDia = await q.GroupBy(v => DateOnly.FromDateTime(v.FechaVenta.Date))
                                 .Select(g => new { Dia = g.Key, Cant = g.Count() })
                                 .OrderBy(x => x.Dia)
                                 .ToListAsync(ct);

            return new VentasSummary
            {
                TotalRecaudado = totalRecaudado,
                Cantidad = cantidad,
                TicketPromedio = cantidad > 0 ? Math.Round(totalRecaudado / cantidad, 2) : 0m,
                PrecioMax = precioMax,
                PrecioMin = precioMin,
                TopTalles = topTalles.Select(x => (x.Talla, x.Cant)).ToList(),
                TopEquipos = topEquipos.Select(x => (x.Eq, x.Cant)).ToList(),
                VentasPorDia = porDia.Select(x => (x.Dia, x.Cant)).ToList()
            };
        }

        public async Task<bool> DeleteVentaAsync(int ventaId, CancellationToken ct = default)
        {
            var venta = await _db.Ventas.FirstOrDefaultAsync(v => v.Id == ventaId, ct);
            if (venta is null) return false;

            // Restaurar stock del talle correspondiente
            var ts = await _db.CamisetaTalles
                .FirstOrDefaultAsync(t => t.CamisetaId == venta.CamisetaId && t.Talla == venta.Talla, ct);
            if (ts is null)
            {
                ts = new CamisetaTalleStock
                {
                    CamisetaId = venta.CamisetaId,
                    Talla = venta.Talla,
                    Cantidad = 0
                };
                _db.CamisetaTalles.Add(ts);
            }
            ts.Cantidad += 1;
            _db.CamisetaTalles.Update(ts);

            _db.Ventas.Remove(venta);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<Venta?> GetVentaAsync(int id, CancellationToken ct = default)
        {
            return await _db.Ventas.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id, ct);
        }

        public async Task<bool> UpdateVentaAsync(int id, string? comprador, string? observaciones, CancellationToken ct = default)
        {
            var venta = await _db.Ventas.FirstOrDefaultAsync(v => v.Id == id, ct);
            if (venta is null) return false;
            venta.Comprador = string.IsNullOrWhiteSpace(comprador) ? null : comprador!.Trim();
            venta.Observaciones = string.IsNullOrWhiteSpace(observaciones) ? null : observaciones!.Trim();
            _db.Ventas.Update(venta);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> RecreateVentaAsync(Venta venta, CancellationToken ct = default)
        {
            // Restar 1 al stock del talle correspondiente
            var ts = await _db.CamisetaTalles.FirstOrDefaultAsync(t => t.CamisetaId == venta.CamisetaId && t.Talla == venta.Talla, ct);
            if (ts is null || ts.Cantidad <= 0)
            {
                return false;
            }
            ts.Cantidad -= 1;
            _db.CamisetaTalles.Update(ts);

            var nueva = new Venta
            {
                CamisetaId = venta.CamisetaId,
                FechaVenta = venta.FechaVenta,
                Precio = venta.Precio,
                Talla = venta.Talla,
                Comprador = venta.Comprador,
                Observaciones = venta.Observaciones,
                ProductoNombre = venta.ProductoNombre,
                Equipo = venta.Equipo,
                Temporada = venta.Temporada
            };
            _db.Ventas.Add(nueva);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        // Búsqueda unificada helpers
        private static List<string> BuildSearchTokens(string q)
        {
            var s = (q ?? string.Empty).Trim().ToLowerInvariant();
            var tokens = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(s)) return tokens.ToList();

            tokens.Add(s);
            // Sinónimos comunes (extensible)
            if (s.Contains("barca") || s.Contains("barsa") || s.Contains("barça")) tokens.Add("barcelona");
            if (s.Replace(" ", "").Contains("laliga")) tokens.Add("la liga");
            if (s.Contains("man united") || s.Contains("man utd")) tokens.Add("manchester united");

            return tokens.ToList();
        }

        private static Expression<Func<Camiseta, bool>> BuildSearchPredicate(IEnumerable<string> tokens)
        {
            var param = Expression.Parameter(typeof(Camiseta), "c");
            Expression? body = null;

            var functions = typeof(EF).GetProperty(nameof(EF.Functions))!;
            var ilike = typeof(NpgsqlDbFunctionsExtensions).GetMethods()
                .First(m => m.Name == nameof(NpgsqlDbFunctionsExtensions.ILike) && m.GetParameters().Length == 3);

            Expression OrIlike(string propName, string pattern)
            {
                var prop = Expression.Property(param, propName);
                return Expression.Call(ilike, Expression.Property(null, functions), prop, Expression.Constant(pattern));
            }

            foreach (var t in tokens)
            {
                var pattern = $"%{t}%";
                var tokenExpr = OrIlike(nameof(Camiseta.Nombre), pattern);
                tokenExpr = Expression.OrElse(tokenExpr, OrIlike(nameof(Camiseta.Equipo), pattern));
                tokenExpr = Expression.OrElse(tokenExpr, OrIlike(nameof(Camiseta.Liga), pattern));
                tokenExpr = Expression.OrElse(tokenExpr, OrIlike(nameof(Camiseta.Temporada), pattern));
                tokenExpr = Expression.OrElse(tokenExpr, OrIlike(nameof(Camiseta.SKU), pattern));
                tokenExpr = Expression.OrElse(tokenExpr, OrIlike(nameof(Camiseta.Marca), pattern));
                tokenExpr = Expression.OrElse(tokenExpr, OrIlike(nameof(Camiseta.Patrocinador), pattern));
                tokenExpr = Expression.OrElse(tokenExpr, OrIlike(nameof(Camiseta.Jugador), pattern));
                tokenExpr = Expression.OrElse(tokenExpr, OrIlike(nameof(Camiseta.Material), pattern));
                tokenExpr = Expression.OrElse(tokenExpr, OrIlike(nameof(Camiseta.TipoCuello), pattern));

                body = body == null ? tokenExpr : Expression.OrElse(body, tokenExpr);
            }

            body ??= Expression.Constant(true);
            return Expression.Lambda<Func<Camiseta, bool>>(body, param);
        }
    }
}
