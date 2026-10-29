using Mensajeria.Api.Dtos;
using Mensajeria.Api.Modelos;

namespace Mensajeria.Api.Mapeos;

public static class MapeoMensaje
{
    /// <summary>Requiere que el mensaje venga con Remitente y Buzon.Usuario cargados.</summary>
    public static MensajeRespuesta MapearA(Mensaje mensaje)
    {
        return new MensajeRespuesta
        {
            Id = mensaje.Id,
            Remitente = mensaje.Remitente.NombreUsuario,
            Destinatario = mensaje.Buzon.Usuario.NombreUsuario,
            Asunto = mensaje.Asunto,
            Contenido = mensaje.Contenido,
            EstaLeido = mensaje.EstaLeido,
            CreadoEn = mensaje.CreadoEn
        };
    }

    public static List<MensajeRespuesta> MapearA(List<Mensaje> mensajes)
    {
        var respuesta = new List<MensajeRespuesta>();
        foreach (var mensaje in mensajes)
            respuesta.Add(MapearA(mensaje));
        return respuesta;
    }
}
