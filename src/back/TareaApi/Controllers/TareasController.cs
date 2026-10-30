using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TareaApi.Models;

namespace TareaApi.Controllers;

[ApiController]
[Route("api/tareas")]
public class TareasController : ControllerBase
{
    private readonly TareaContext _db;

    public TareasController(TareaContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<Tarea>>> GetAll()
    {
        return await _db.Tareas.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Tarea>> Get(int id)
    {
        var tarea = await _db.Tareas.FindAsync(id);
        if (tarea == null) return NotFound();
        return tarea;
    }

    [HttpPost]
    public async Task<ActionResult<Tarea>> Create(Tarea tarea)
    {
        tarea.Id = 0;
        _db.Tareas.Add(tarea);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = tarea.Id }, tarea);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Tarea tarea)
    {
        var existing = await _db.Tareas.FindAsync(id);
        if (existing == null) return NotFound();

        existing.Title = tarea.Title;
        existing.Description = tarea.Description;
        existing.Status = tarea.Status;
        existing.DueDate = tarea.DueDate;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var tarea = await _db.Tareas.FindAsync(id);
        if (tarea == null) return NotFound();

        _db.Tareas.Remove(tarea);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
