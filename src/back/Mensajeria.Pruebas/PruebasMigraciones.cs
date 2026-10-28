using Mensajeria.Api.Datos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace Mensajeria.Pruebas;

public class PruebasMigraciones : IClassFixture<FabricaApi>
{
    private readonly FabricaApi _fabrica;

    public PruebasMigraciones(FabricaApi fabrica)
    {
        _fabrica = fabrica;
    }

    [Fact]
    public async Task AlArrancar_SeAplicanLasTresMigracionesEnOrden()
    {
        using var alcance = _fabrica.Services.CreateScope();
        var db = alcance.ServiceProvider.GetRequiredService<MensajeriaDbContext>();

        var aplicadas = (await db.Database.GetAppliedMigrationsAsync()).Select(m => m[(m.IndexOf('_') + 1)..]).ToList();
        var pendientes = await db.Database.GetPendingMigrationsAsync();

        Assert.Equal(new[] { "InitialUsers", "AddInboxesAndMessages", "AddMessageReadStatus" }, aplicadas);
        Assert.Empty(pendientes);
    }

    [Fact]
    public async Task LosUsuariosDePrueba_EstanCargadosConSuBuzon()
    {
        using var alcance = _fabrica.Services.CreateScope();
        var db = alcance.ServiceProvider.GetRequiredService<MensajeriaDbContext>();

        var conBuzon = await db.Buzones.Select(b => b.Usuario.NombreUsuario).ToListAsync();

        Assert.Contains("Alice", conBuzon);
        Assert.Contains("Bob", conBuzon);
    }
}
