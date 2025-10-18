using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AgroMarketApi.Data;
using AgroMarketApi.Models;

namespace AgroMarketApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatsController(AppDbContext db) : ControllerBase
{
    // GET: /api/chats?empresaId=2&productorId=1&productoId=3
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Chat>>> Get(int? empresaId = null, int? productorId = null, int? productoId = null)
    {
        IQueryable<Chat> q = db.Chats.AsNoTracking();

        if (empresaId is not null) q = q.Where(c => c.EmpresaId == empresaId);
        if (productorId is not null) q = q.Where(c => c.ProductorId == productorId);
        if (productoId is not null) q = q.Where(c => c.ProductoId == productoId);

        var list = await q
            .Include(c => c.Producto) // Include al final
            .OrderByDescending(c => c.Id)
            .ToListAsync();

        return Ok(list);
    }

    // GET: /api/chats/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Chat>> GetById(int id)
    {
        var c = await db.Chats.AsNoTracking()
            .Include(x => x.Producto)
            .FirstOrDefaultAsync(x => x.Id == id);

        return c is null ? NotFound() : Ok(c);
    }

    // POST: /api/chats  (UNIQUE producto_id + empresa_id) ⇒ si no existe lo crea
    [HttpPost]
    public async Task<ActionResult<Chat>> Create(Chat input)
    {
        // Si no viene productor, lo inferimos del producto
        if (input.ProductorId == 0)
        {
            var prod = await db.Productos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == input.ProductoId);
            if (prod is null) return NotFound("Producto no encontrado.");
            input.ProductorId = prod.ProductorId;
        }

        var dup = await db.Chats
            .FirstOrDefaultAsync(c => c.ProductoId == input.ProductoId && c.EmpresaId == input.EmpresaId);

        if (dup is not null) return Conflict("Ya existe chat para ese producto y empresa.");

        input.FechaCreacion = input.FechaCreacion ?? DateTime.UtcNow;
        db.Chats.Add(input);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = input.Id }, input);
    }

    // PUT: /api/chats/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Chat input)
    {
        if (id != input.Id) return BadRequest("El id del cuerpo no coincide con la ruta.");
        db.Entry(input).State = EntityState.Modified;
        await db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: /api/chats/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var c = await db.Chats.FindAsync(id);
        if (c is null) return NotFound();
        db.Chats.Remove(c);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
