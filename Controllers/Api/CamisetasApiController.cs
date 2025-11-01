using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoCamisetas.Data;
using ProyectoCamisetas.Models;

namespace ProyectoCamisetas.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class CamisetasController : ControllerBase
    {
        private readonly AppDbContext _db;

        public CamisetasController(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Lista camisetas (con filtros opcionales).
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Camiseta>), 200)]
        public async Task<IActionResult> GetAll([FromQuery] string? q, [FromQuery] string? liga, [FromQuery] string? equipo, [FromQuery] string? temporada, CancellationToken ct)
        {
            IQueryable<Camiseta> query = _db.Camisetas.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = $"%{q.Trim()}%";
                query = query.Where(c => EF.Functions.ILike(c.Nombre, term) || EF.Functions.ILike(c.Equipo, term) || EF.Functions.ILike(c.Liga, term) || EF.Functions.ILike(c.SKU, term));
            }
            if (!string.IsNullOrWhiteSpace(liga)) query = query.Where(c => c.Liga == liga);
            if (!string.IsNullOrWhiteSpace(equipo)) query = query.Where(c => c.Equipo == equipo);
            if (!string.IsNullOrWhiteSpace(temporada)) query = query.Where(c => c.Temporada == temporada);

            var result = await query.OrderBy(c => c.Equipo).ThenBy(c => c.Temporada).ThenBy(c => c.Tipo).ToListAsync(ct);
            return Ok(result);
        }

        /// <summary>
        /// Lista todas las camisetas sin filtros.
        /// </summary>
        [HttpGet("all")]
        [ProducesResponseType(typeof(IEnumerable<Camiseta>), 200)]
        public async Task<IActionResult> GetAllNoFilters(CancellationToken ct)
        {
            var result = await _db.Camisetas
                .AsNoTracking()
                .OrderBy(c => c.Equipo)
                .ThenBy(c => c.Temporada)
                .ThenBy(c => c.Tipo)
                .ToListAsync(ct);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene una camiseta por id.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(Camiseta), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct)
        {
            var entity = await _db.Camisetas.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
            if (entity == null) return NotFound();
            return Ok(entity);
        }

        /// <summary>
        /// Crea una nueva camiseta.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Owner")]
        [ProducesResponseType(typeof(Camiseta), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] Camiseta model, CancellationToken ct)
        {
            // Normalizar SKU opcional: convertir vacío/espacios a null
            model.SKU = string.IsNullOrWhiteSpace(model.SKU) ? null : model.SKU!.Trim();
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            _db.Camisetas.Add(model);
            await _db.SaveChangesAsync(ct);
            return CreatedAtAction(nameof(GetById), new { id = model.Id }, model);
        }

        /// <summary>
        /// Actualiza una camiseta existente.
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Owner")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] Camiseta model, CancellationToken ct)
        {
            if (id != model.Id) return BadRequest();
            var exists = await _db.Camisetas.AnyAsync(c => c.Id == id, ct);
            if (!exists) return NotFound();
            // Normalizar SKU opcional: convertir vacío/espacios a null
            model.SKU = string.IsNullOrWhiteSpace(model.SKU) ? null : model.SKU!.Trim();
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            _db.Entry(model).State = EntityState.Modified;
            await _db.SaveChangesAsync(ct);
            return NoContent();
        }

        /// <summary>
        /// Elimina una camiseta existente.
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Owner")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
        {
            var entity = await _db.Camisetas.FindAsync([id], ct);
            if (entity == null) return NotFound();
            _db.Camisetas.Remove(entity);
            await _db.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}
