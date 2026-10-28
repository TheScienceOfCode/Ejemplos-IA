using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Mensajeria.Api.Datos;

/// <summary>
/// Solo la usan las herramientas de "dotnet ef" para crear migraciones
/// (ver run/nueva-migracion.sh). La aplicación en ejecución no la necesita.
/// </summary>
public class FabricaDbContextDiseno : IDesignTimeDbContextFactory<MensajeriaDbContext>
{
    public MensajeriaDbContext CreateDbContext(string[] args)
    {
        var opciones = new DbContextOptionsBuilder<MensajeriaDbContext>()
            .UseSqlite("Data Source=diseno.db")
            .Options;
        return new MensajeriaDbContext(opciones);
    }
}
