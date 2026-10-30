using Microsoft.EntityFrameworkCore;
using TareaApi.Models;

namespace TareaApi;

public class TareaContext : DbContext
{
    public TareaContext(DbContextOptions<TareaContext> options) : base(options) { }

    public DbSet<Tarea> Tareas => Set<Tarea>();
}
