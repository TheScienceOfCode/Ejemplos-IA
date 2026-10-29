using Mensajeria.Api.Mapeos;
using Mensajeria.Api.Modelos;

namespace Mensajeria.Pruebas;

public class PruebasMapeos
{
    [Fact]
    public void Mensaje_Null_DevuelveNull()
    {
        Assert.Null(MapeoMensaje.MapearA((Mensaje?)null));
    }

    [Fact]
    public void Mensaje_SinRemitenteNiBuzonCargados_DevuelveNull()
    {
        var mensaje = new Mensaje { Id = 1, Asunto = "Hola" }; // sin Include: las relaciones están vacías

        Assert.Null(MapeoMensaje.MapearA(mensaje));
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

        var dto = MapeoMensaje.MapearA(mensaje);

        Assert.NotNull(dto);
        Assert.Equal("Alice", dto.Remitente);
        Assert.Equal("Bob", dto.Destinatario);
    }

    [Fact]
    public void Lista_ConUnMensajeIncompleto_DevuelveNull()
    {
        var incompleto = new Mensaje { Id = 1 };

        Assert.Null(MapeoMensaje.MapearA(new List<Mensaje> { incompleto }));
    }

    [Fact]
    public void Buzon_SinUsuarioCargado_DevuelveNull()
    {
        Assert.Null(MapeoBuzon.MapearA(new Buzon { Id = 1 }));
        Assert.Null(MapeoBuzon.MapearA(null));
    }

    [Fact]
    public void Usuario_NoCopiaElHashDeLaContrasena()
    {
        var dto = MapeoUsuario.MapearA(new Usuario { NombreUsuario = "Alice", HashContrasena = "secreto" });

        Assert.NotNull(dto);
        Assert.DoesNotContain("secreto", System.Text.Json.JsonSerializer.Serialize(dto));
        Assert.Null(MapeoUsuario.MapearA(null));
        Assert.Null(MapeoUsuario.MapearA(new Usuario(), "", DateTime.UtcNow));
    }
}
