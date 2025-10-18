using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AgroMarketApi.Data;
using AgroMarketApi.Models;

namespace AgroMarketApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MensajesController(AppDbContext db) : ControllerBase
{
    // GET: /api/mensajes?chatId=1
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Mensaje>>> Get(int? chatId = null)
    {
        IQueryable<Mensaje> q = db.Mensajes.AsNoTracking();
        if (chatId is not null) q = q.Where(m => m.ChatId == chatId);

        var list = await q
            .OrderBy(m => m.Id)
            .ToListAsync();

        return Ok(list);
    }

    // GET: /api/mensajes/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Mensaje>> GetById(int id)
    {
        var m = await db.Mensajes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return m is null ? NotFound() : Ok(m);
    }

    // POST: /api/mensajes
    [HttpPost]
    public async Task<ActionResult<Mensaje>> Create(Mensaje input)
    {
        // Validar chat existente
        var exists = await db.Chats.AnyAsync(c => c.Id == input.ChatId);
        if (!exists) return NotFound("Chat no encontrado.");

        input.FechaEnvio = input.FechaEnvio ?? DateTime.UtcNow;
        db.Mensajes.Add(input);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = input.Id }, input);
    }

    // PUT: /api/mensajes/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Mensaje input)
    {
        if (id != input.Id) return BadRequest("El id del cuerpo no coincide con la ruta.");
        db.Entry(input).State = EntityState.Modified;
        await db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: /api/mensajes/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var m = await db.Mensajes.FindAsync(id);
        if (m is null) return NotFound();
        db.Mensajes.Remove(m);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
