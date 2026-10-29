using Mensajeria.Api.Mapeos;
using Mensajeria.Api.Modelos;

namespace Mensajeria.Pruebas;

public class PruebasMapeos
{
    [Fact]
    public void Mensaje_Null_LanzaArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => MapeoMensaje.Respuesta((Mensaje)null!));
    }

    [Fact]
    public void Mensaje_SinRemitenteNiBuzonCargados_LanzaArgumentNullException()
    {
        var mensaje = new Mensaje { Id = 1, Asunto = "Hola" }; // sin Include: las relaciones están vacías

        Assert.Throws<ArgumentNullException>(() => MapeoMensaje.Respuesta(mensaje));
    }

    [Fact]
    public void Mensaje_Completo_SeMapeaConNombresDeRemitenteYDestinatario()
    {
        var mensaje = new Mensaje
        {
            Id = 7,
            Asunto = "Hola",
            Contenido = "Texto",
            Remitente = new Usuario { NombreUsuario = "Alice", HashContrasena = "secreto" },
            Buzon = new Buzon { Usuario = new Usuario { NombreUsuario = "Bob" } }
        };

        var dto = MapeoMensaje.Respuesta(mensaje);

        Assert.Equal("Alice", dto.Remitente);
        Assert.Equal("Bob", dto.Destinatario);
    }

    [Fact]
    public void Lista_ConUnMensajeIncompleto_LanzaArgumentNullException()
    {
        var incompleto = new Mensaje { Id = 1 };

        Assert.Throws<ArgumentNullException>(() => MapeoMensaje.Respuesta(new List<Mensaje> { incompleto }));
    }

    [Fact]
    public void Buzon_SinUsuarioCargado_LanzaArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => MapeoBuzon.Respuesta(new Buzon { Id = 1 }));
        Assert.Throws<ArgumentNullException>(() => MapeoBuzon.Respuesta(null!));
    }

    [Fact]
    public void Usuario_NoCopiaElHashDeLaContrasena()
    {
        var dto = MapeoUsuario.Respuesta(new Usuario { NombreUsuario = "Alice", HashContrasena = "secreto" });

        Assert.DoesNotContain("secreto", System.Text.Json.JsonSerializer.Serialize(dto));
        Assert.Throws<ArgumentNullException>(() => MapeoUsuario.Respuesta(null!));
        Assert.Throws<ArgumentNullException>(() => MapeoUsuario.RespuestaToken(new Usuario(), "", DateTime.UtcNow));
    }
}
