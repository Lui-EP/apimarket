using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AgroMarketApi.Data;
using AgroMarketApi.Models;

namespace AgroMarketApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InteresesController(AppDbContext db) : ControllerBase
{
    // GET: /api/intereses?productoId=1&empresaId=2
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Interes>>> Get(int? productoId = null, int? empresaId = null)
    {
        IQueryable<Interes> q = db.Intereses.AsNoTracking();

        if (productoId is not null) q = q.Where(i => i.ProductoId == productoId);
        if (empresaId is not null) q = q.Where(i => i.EmpresaId == empresaId);

        var list = await q
            .Include(i => i.Producto) // Include al final
            .OrderByDescending(i => i.Id)
            .ToListAsync();

        return Ok(list);
    }

    // GET: /api/intereses/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Interes>> GetById(int id)
    {
        var it = await db.Intereses.AsNoTracking()
            .Include(i => i.Producto)
            .FirstOrDefaultAsync(x => x.Id == id);

        return it is null ? NotFound() : Ok(it);
    }

    // POST: /api/intereses  (respeta UNIQUE producto_id + empresa_id)
    [HttpPost]
    public async Task<ActionResult<Interes>> Create(Interes input)
    {
        var dup = await db.Intereses
            .FirstOrDefaultAsync(i => i.ProductoId == input.ProductoId && i.EmpresaId == input.EmpresaId);

        if (dup is not null) return Conflict("Ya existe un interés de esa empresa para el producto.");

        input.FechaInteres = input.FechaInteres ?? DateTime.UtcNow;
        db.Intereses.Add(input);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = input.Id }, input);
    }

    // PUT: /api/intereses/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Interes input)
    {
        if (id != input.Id) return BadRequest("El id del cuerpo no coincide con la ruta.");
        db.Entry(input).State = EntityState.Modified;
        await db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: /api/intereses/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var it = await db.Intereses.FindAsync(id);
        if (it is null) return NotFound();
        db.Intereses.Remove(it);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
