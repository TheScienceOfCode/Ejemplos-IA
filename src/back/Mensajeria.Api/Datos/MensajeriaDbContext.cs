using Mensajeria.Api.Modelos;
using Microsoft.EntityFrameworkCore;

namespace Mensajeria.Api.Datos;

/// <summary>Puente entre las clases C# y las tablas de SQLite.</summary>
public class MensajeriaDbContext : DbContext
{
    public MensajeriaDbContext(DbContextOptions<MensajeriaDbContext> opciones) : base(opciones) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Buzon> Buzones => Set<Buzon>();
    public DbSet<Mensaje> Mensajes => Set<Mensaje>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(e =>
        {
            // NOCASE: "alice" y "Alice" son el mismo usuario (y el índice único también lo respeta)
            e.Property(u => u.NombreUsuario).HasMaxLength(50).UseCollation("NOCASE");
            e.Property(u => u.Correo).HasMaxLength(200).UseCollation("NOCASE");
            e.Property(u => u.HashContrasena).HasMaxLength(500);

            e.HasIndex(u => u.NombreUsuario).IsUnique();
            e.HasIndex(u => u.Correo).IsUnique();
        });

        modelBuilder.Entity<Buzon>(e =>
        {
            // Relación 1 a 1: un usuario tiene un único buzón
            e.HasOne(b => b.Usuario)
                .WithOne()
                .HasForeignKey<Buzon>(b => b.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Mensaje>(e =>
        {
            e.Property(m => m.Asunto).HasMaxLength(200);
            e.Property(m => m.Contenido).HasMaxLength(4000);

            // Un buzón tiene muchos mensajes recibidos
            e.HasOne(m => m.Buzon)
                .WithMany(b => b.MensajesRecibidos)
                .HasForeignKey(m => m.BuzonId)
                .OnDelete(DeleteBehavior.Cascade);

            // Un usuario puede ser remitente de muchos mensajes (no se borran en cascada)
            e.HasOne(m => m.Remitente)
                .WithMany()
                .HasForeignKey(m => m.RemitenteId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
