using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Mensajeria.Api.Dtos;

namespace Mensajeria.Pruebas;

/// <summary>Nadie puede ver ni modificar lo que no le pertenece.</summary>
public class PruebasAutorizacion : IClassFixture<FabricaApi>
{
    private readonly FabricaApi _fabrica;

    public PruebasAutorizacion(FabricaApi fabrica)
    {
        _fabrica = fabrica;
    }

    [Theory]
    [InlineData("GET", "/api/buzon")]
    [InlineData("GET", "/api/mensajes/enviados")]
    [InlineData("GET", "/api/mensajes/1")]
    [InlineData("PUT", "/api/mensajes/1/leido")]
    [InlineData("POST", "/api/mensajes")]
    public async Task SinToken_Responde401(string metodo, string ruta)
    {
        var respuesta = await _fabrica.CreateClient().SendAsync(new HttpRequestMessage(new HttpMethod(metodo), ruta));

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task ConTokenInvalido_Responde401()
    {
        var cliente = _fabrica.CreateClient();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "esto.no.es.un.jwt");

        var respuesta = await cliente.GetAsync("/api/buzon");

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task Alice_NoVeEnSuBuzonLosMensajesDeBob()
    {
        var alice = await _fabrica.ClienteComoAsync("Alice");
        var bob = await _fabrica.ClienteComoAsync("Bob");
        var asunto = "Solo para Bob " + Guid.NewGuid().ToString("N")[..8];
        var envio = await alice.PostAsJsonAsync("/api/mensajes", new EnviarMensajeSolicitud
        {
            DestinatarioNombreUsuario = "Bob",
            Asunto = asunto,
            Contenido = "Privado"
        });
        Assert.Equal(HttpStatusCode.Created, envio.StatusCode);

        // No hay forma de pedir el buzón de otro: cada quien solo obtiene el suyo, según su token
        var buzonAlice = (await alice.GetFromJsonAsync<BuzonRespuesta>("/api/buzon"))!;
        var buzonBob = (await bob.GetFromJsonAsync<BuzonRespuesta>("/api/buzon"))!;

        Assert.Equal("Alice", buzonAlice.Propietario);
        Assert.Equal("Bob", buzonBob.Propietario);
        Assert.DoesNotContain(buzonAlice.Mensajes, m => m.Asunto == asunto);
        Assert.Contains(buzonBob.Mensajes, m => m.Asunto == asunto);
    }

    [Fact]
    public async Task UnTercero_NoPuedeVerNiMarcarComoLeidoUnMensajeAjeno()
    {
        var intruso = await _fabrica.RegistrarNuevoAsync("intruso");
        var alice = await _fabrica.ClienteComoAsync("Alice");
        var envio = await alice.PostAsJsonAsync("/api/mensajes", new EnviarMensajeSolicitud
        {
            DestinatarioNombreUsuario = "Bob",
            Asunto = "Privado",
            Contenido = "Solo para Bob"
        });
        var mensaje = (await envio.Content.ReadFromJsonAsync<MensajeRespuesta>())!;

        var clienteIntruso = await _fabrica.ClienteComoAsync(intruso);

        Assert.Equal(HttpStatusCode.NotFound, (await clienteIntruso.GetAsync($"/api/mensajes/{mensaje.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await clienteIntruso.PutAsync($"/api/mensajes/{mensaje.Id}/leido", null)).StatusCode);

        // Y el mensaje sigue sin leer
        var bob = await _fabrica.ClienteComoAsync("Bob");
        var visto = (await bob.GetFromJsonAsync<MensajeRespuesta>($"/api/mensajes/{mensaje.Id}"))!;
        Assert.False(visto.EstaLeido);
    }

    [Fact]
    public async Task ElRemitente_NoPuedeMarcarComoLeidoUnMensajeQueEnvio()
    {
        var alice = await _fabrica.ClienteComoAsync("Alice");
        var envio = await alice.PostAsJsonAsync("/api/mensajes", new EnviarMensajeSolicitud
        {
            DestinatarioNombreUsuario = "Bob",
            Asunto = "Prueba",
            Contenido = "Hola"
        });
        var mensaje = (await envio.Content.ReadFromJsonAsync<MensajeRespuesta>())!;

        var respuesta = await alice.PutAsync($"/api/mensajes/{mensaje.Id}/leido", null);

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task MensajeInexistente_Responde404()
    {
        var alice = await _fabrica.ClienteComoAsync("Alice");

        Assert.Equal(HttpStatusCode.NotFound, (await alice.GetAsync("/api/mensajes/999999")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await alice.PutAsync("/api/mensajes/999999/leido", null)).StatusCode);
    }
}
