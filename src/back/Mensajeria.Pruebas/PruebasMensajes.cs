using System.Net;
using System.Net.Http.Json;
using Mensajeria.Api.Dtos;

namespace Mensajeria.Pruebas;

/// <summary>El flujo principal: Alice le escribe a Bob, Bob lo lee y lo marca como leído.</summary>
public class PruebasMensajes : IClassFixture<FabricaApi>
{
    private readonly FabricaApi _fabrica;

    public PruebasMensajes(FabricaApi fabrica)
    {
        _fabrica = fabrica;
    }

    [Fact]
    public async Task FlujoCompleto_EnviarVerLeerYMarcarComoLeido()
    {
        var asunto = "Hola " + Guid.NewGuid().ToString("N")[..8];
        var alice = await _fabrica.ClienteComoAsync("Alice");
        var bob = await _fabrica.ClienteComoAsync("Bob");

        // 1. Alice envía un mensaje a Bob
        var envio = await alice.PostAsJsonAsync("/api/mensajes", new EnviarMensajeSolicitud
        {
            DestinatarioNombreUsuario = "Bob",
            Asunto = asunto,
            Contenido = "¿Almorzamos?"
        });
        Assert.Equal(HttpStatusCode.Created, envio.StatusCode);
        var enviado = (await envio.Content.ReadFromJsonAsync<MensajeRespuesta>())!;
        Assert.Equal("Alice", enviado.Remitente);
        Assert.Equal("Bob", enviado.Destinatario);
        Assert.False(enviado.EstaLeido);

        // 2. Aparece en los enviados de Alice
        var enviados = await alice.GetFromJsonAsync<List<MensajeRespuesta>>("/api/mensajes/enviados");
        Assert.Contains(enviados!, m => m.Id == enviado.Id);

        // 3. Aparece en el buzón de Bob, sin leer, y NO en el de Alice
        var buzonBob = (await bob.GetFromJsonAsync<BuzonRespuesta>("/api/buzon"))!;
        Assert.Contains(buzonBob.Mensajes, m => m.Id == enviado.Id && !m.EstaLeido);
        Assert.True(buzonBob.NoLeidos >= 1);
        var buzonAlice = (await alice.GetFromJsonAsync<BuzonRespuesta>("/api/buzon"))!;
        Assert.DoesNotContain(buzonAlice.Mensajes, m => m.Id == enviado.Id);

        // 4. Bob lo abre: leer no lo marca como leído por sí solo
        var detalle = (await bob.GetFromJsonAsync<MensajeRespuesta>($"/api/mensajes/{enviado.Id}"))!;
        Assert.Equal("¿Almorzamos?", detalle.Contenido);
        Assert.False(detalle.EstaLeido);

        // 5. Bob lo marca como leído (y repetirlo no da error)
        var marcar = await bob.PutAsync($"/api/mensajes/{enviado.Id}/leido", null);
        Assert.Equal(HttpStatusCode.OK, marcar.StatusCode);
        Assert.True((await marcar.Content.ReadFromJsonAsync<MensajeRespuesta>())!.EstaLeido);
        Assert.Equal(HttpStatusCode.OK, (await bob.PutAsync($"/api/mensajes/{enviado.Id}/leido", null)).StatusCode);

        // 6. El cambio quedó guardado y Alice también lo ve como leído
        var buzonFinal = (await bob.GetFromJsonAsync<BuzonRespuesta>("/api/buzon"))!;
        Assert.True(buzonFinal.Mensajes.Single(m => m.Id == enviado.Id).EstaLeido);
        var vistoPorAlice = (await alice.GetFromJsonAsync<MensajeRespuesta>($"/api/mensajes/{enviado.Id}"))!;
        Assert.True(vistoPorAlice.EstaLeido);
    }

    [Fact]
    public async Task Enviar_AUsuarioNuevoRegistrado_LoRecibeEnSuBuzon()
    {
        var nombre = await _fabrica.RegistrarNuevoAsync("carla");
        var alice = await _fabrica.ClienteComoAsync("Alice");

        var envio = await alice.PostAsJsonAsync("/api/mensajes", new EnviarMensajeSolicitud
        {
            DestinatarioNombreUsuario = nombre,
            Asunto = "Bienvenida",
            Contenido = "Hola"
        });
        Assert.Equal(HttpStatusCode.Created, envio.StatusCode);

        var carla = await _fabrica.ClienteComoAsync(nombre);
        var buzon = (await carla.GetFromJsonAsync<BuzonRespuesta>("/api/buzon"))!;
        Assert.Single(buzon.Mensajes);
        Assert.Equal("Alice", buzon.Mensajes[0].Remitente);
    }

    [Fact]
    public async Task Enviar_ADestinatarioInexistente_Responde404()
    {
        var alice = await _fabrica.ClienteComoAsync("Alice");

        var respuesta = await alice.PostAsJsonAsync("/api/mensajes", new EnviarMensajeSolicitud
        {
            DestinatarioNombreUsuario = "Fantasma",
            Asunto = "Hola",
            Contenido = "¿Hay alguien?"
        });

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task Enviar_SinAsuntoOContenido_Responde400()
    {
        var alice = await _fabrica.ClienteComoAsync("Alice");

        var respuesta = await alice.PostAsJsonAsync("/api/mensajes", new EnviarMensajeSolicitud
        {
            DestinatarioNombreUsuario = "Bob",
            Asunto = "",
            Contenido = ""
        });

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }
}
