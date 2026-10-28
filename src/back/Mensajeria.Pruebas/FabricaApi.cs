using System.Net.Http.Headers;
using System.Net.Http.Json;
using Mensajeria.Api.Dtos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;

namespace Mensajeria.Pruebas;

/// <summary>
/// Arranca la API completa en memoria con una base SQLite temporal propia
/// (se aplican las migraciones reales y se cargan Alice y Bob, igual que en Docker).
/// </summary>
public class FabricaApi : WebApplicationFactory<Program>
{
    private readonly string _archivoBd = Path.Combine(Path.GetTempPath(), $"mensajeria-pruebas-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:Mensajeria", $"Data Source={_archivoBd}");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        SqliteConnection.ClearAllPools();
        if (File.Exists(_archivoBd)) File.Delete(_archivoBd);
    }

    /// <summary>Cliente HTTP ya autenticado como ese usuario (hace login real y usa el JWT).</summary>
    public async Task<HttpClient> ClienteComoAsync(string usuario, string contrasena = "123456")
    {
        var cliente = CreateClient();
        var respuesta = await cliente.PostAsJsonAsync("/api/auth/login",
            new LoginSolicitud { NombreUsuario = usuario, Contrasena = contrasena });
        respuesta.EnsureSuccessStatusCode();

        var token = (await respuesta.Content.ReadFromJsonAsync<TokenRespuesta>())!.Token;
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return cliente;
    }

    /// <summary>Registra un usuario nuevo con un nombre único (para no depender de otras pruebas).</summary>
    public async Task<string> RegistrarNuevoAsync(string prefijo)
    {
        var nombre = $"{prefijo}{Guid.NewGuid():N}"[..20];
        var respuesta = await CreateClient().PostAsJsonAsync("/api/auth/registro", new RegistroSolicitud
        {
            NombreUsuario = nombre,
            Correo = $"{nombre}@ejemplo.com",
            Contrasena = "123456"
        });
        respuesta.EnsureSuccessStatusCode();
        return nombre;
    }
}
