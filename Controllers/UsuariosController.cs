using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AgroMarketApi.Data;
using AgroMarketApi.Models;

namespace AgroMarketApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController(AppDbContext db) : ControllerBase
{
    // GET: /api/usuarios?tipo=productor
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Usuario>>> Get(string? tipo = null)
    {
        IQueryable<Usuario> q = db.Usuarios.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(tipo)) q = q.Where(u => u.Tipo == tipo);
        var list = await q.OrderBy(u => u.Id).ToListAsync();
        return Ok(list);
    }

    // GET: /api/usuarios/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Usuario>> GetById(int id)
    {
        var u = await db.Usuarios.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return u is null ? NotFound() : Ok(u);
    }

    // POST: /api/usuarios
    [HttpPost]
    public async Task<ActionResult<Usuario>> Create(Usuario input)
    {
        db.Usuarios.Add(input);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = input.Id }, input);
    }

    // PUT: /api/usuarios/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Usuario input)
    {
        if (id != input.Id) return BadRequest("El id del cuerpo no coincide con la ruta.");
        db.Entry(input).State = EntityState.Modified;
        await db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: /api/usuarios/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var u = await db.Usuarios.FindAsync(id);
        if (u is null) return NotFound();
        db.Usuarios.Remove(u);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
