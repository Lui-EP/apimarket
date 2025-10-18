using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AgroMarketApi.Data;
using AgroMarketApi.Models;

namespace AgroMarketApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductosController(AppDbContext db) : ControllerBase
{
    // GET: /api/productos?productorId=1&q=maiz&activos=true
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Producto>>> Get(
        int? productorId = null, string? q = null, bool? activos = null)
    {
        IQueryable<Producto> qry = db.Productos.AsNoTracking();

        if (productorId is not null) qry = qry.Where(p => p.ProductorId == productorId);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            qry = qry.Where(p => p.Nombre.ToLower().Contains(term) ||
                                 (p.Descripcion != null && p.Descripcion.ToLower().Contains(term)));
        }
        if (activos is not null) qry = qry.Where(p => p.Activo == activos);

        var list = await qry
            .OrderByDescending(p => p.Id)
            .ToListAsync();

        return Ok(list);
    }

    // GET: /api/productos/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Producto>> GetById(int id)
    {
        var p = await db.Productos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return p is null ? NotFound() : Ok(p);
    }

    // POST: /api/productos
    [HttpPost]
    public async Task<ActionResult<Producto>> Create(Producto input)
    {
        db.Productos.Add(input);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = input.Id }, input);
    }

    // PUT: /api/productos/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Producto input)
    {
        if (id != input.Id) return BadRequest("El id del cuerpo no coincide con la ruta.");
        db.Entry(input).State = EntityState.Modified;
        await db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: /api/productos/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await db.Productos.FindAsync(id);
        if (p is null) return NotFound();
        db.Productos.Remove(p);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
