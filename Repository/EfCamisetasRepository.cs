using Microsoft.EntityFrameworkCore;
using ProyectoCamisetas.Data;
using ProyectoCamisetas.Models;
using System.Linq.Expressions;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using System.Collections.Generic;

namespace ProyectoCamisetas.Repository
{
    public class EfCamisetasRepository : ICamisetasRepository
    {
        private readonly AppDbContext _db;
        public EfCamisetasRepository(AppDbContext db) { _db = db; }

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
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(liga)) query = query.Where(c => c.Liga == liga);
                if (!string.IsNullOrWhiteSpace(equipo)) query = query.Where(c => c.Equipo == equipo);
                if (!string.IsNullOrWhiteSpace(temporada)) query = query.Where(c => c.Temporada == temporada);
            }

            return await query
                .OrderByDescending(c => c.Stock > 0)
                .ThenBy(c => c.Equipo)
                .ThenBy(c => c.Temporada)
                .ThenBy(c => c.Tipo)
                .ToListAsync(ct);
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
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(liga)) query = query.Where(c => c.Liga == liga);
                if (!string.IsNullOrWhiteSpace(equipo)) query = query.Where(c => c.Equipo == equipo);
                if (!string.IsNullOrWhiteSpace(temporada)) query = query.Where(c => c.Temporada == temporada);
            }

            return await query
                .OrderByDescending(c => c.EsEdicionLimitada)
                .ThenByDescending(c => c.Precio)
                .Take(take)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<Camiseta>> GetAllAdminAsync(CancellationToken ct = default)
        {
            return await _db.Camisetas.AsNoTracking()
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
