using System.Net;
using System.Net.Http.Json;
using Mensajeria.Api.Dtos;

namespace Mensajeria.Pruebas;

public class PruebasAutenticacion : IClassFixture<FabricaApi>
{
    private readonly FabricaApi _fabrica;

    public PruebasAutenticacion(FabricaApi fabrica)
    {
        _fabrica = fabrica;
    }

    [Fact]
    public async Task Registro_CreaUsuarioConBuzonYNoDevuelveElHash()
    {
        var nombre = "nuevo" + Guid.NewGuid().ToString("N")[..8];
        var cliente = _fabrica.CreateClient();

        var respuesta = await cliente.PostAsJsonAsync("/api/auth/registro", new RegistroSolicitud
        {
            NombreUsuario = nombre,
            Correo = nombre + "@ejemplo.com",
            Contrasena = "secreta123"
        });

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var json = await respuesta.Content.ReadAsStringAsync();
        Assert.DoesNotContain("hash", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secreta123", json);

        // El buzón existe y está vacío: se creó automáticamente al registrarse
        var sesion = await _fabrica.ClienteComoAsync(nombre, "secreta123");
        var buzon = await sesion.GetFromJsonAsync<BuzonRespuesta>("/api/buzon");
        Assert.NotNull(buzon);
        Assert.Equal(nombre, buzon.Propietario);
        Assert.Empty(buzon.Mensajes);
    }

    [Fact]
    public async Task Registro_ConNombreRepetido_Responde409_AunqueCambieLaMayuscula()
    {
        var cliente = _fabrica.CreateClient();

        var respuesta = await cliente.PostAsJsonAsync("/api/auth/registro", new RegistroSolicitud
        {
            NombreUsuario = "ALICE",
            Correo = "otra@ejemplo.com",
            Contrasena = "123456"
        });

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
    }

    [Fact]
    public async Task Registro_ConDatosInvalidos_Responde400()
    {
        var cliente = _fabrica.CreateClient();

        var respuesta = await cliente.PostAsJsonAsync("/api/auth/registro", new RegistroSolicitud
        {
            NombreUsuario = "ab",            // muy corto
            Correo = "no-es-un-correo",
            Contrasena = "123"               // muy corta
        });

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Theory]
    [InlineData("Alice")]
    [InlineData("Bob")]
    [InlineData("alice")] // el nombre no distingue mayúsculas
    public async Task Login_ConUsuariosDePrueba_DevuelveToken(string usuario)
    {
        var respuesta = await _fabrica.CreateClient().PostAsJsonAsync("/api/auth/login",
            new LoginSolicitud { NombreUsuario = usuario, Contrasena = "123456" });

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var token = await respuesta.Content.ReadFromJsonAsync<TokenRespuesta>();
        Assert.False(string.IsNullOrWhiteSpace(token?.Token));
    }

    [Theory]
    [InlineData("Alice", "incorrecta")]
    [InlineData("NoExiste", "123456")]
    public async Task Login_ConCredencialesIncorrectas_Responde401(string usuario, string contrasena)
    {
        var respuesta = await _fabrica.CreateClient().PostAsJsonAsync("/api/auth/login",
            new LoginSolicitud { NombreUsuario = usuario, Contrasena = contrasena });

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }
}
