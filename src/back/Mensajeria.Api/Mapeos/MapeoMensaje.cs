using Mensajeria.Api.Dtos;
using Mensajeria.Api.Modelos;

namespace Mensajeria.Api.Mapeos;

public static class MapeoMensaje
{
    /// <summary>
    /// Devuelve null si el mensaje es null o si no trae cargados el remitente y el destinatario
    /// (es decir, si faltó un Include en la consulta del servicio).
    /// </summary>
    public static MensajeRespuesta? MapearA(Mensaje? mensaje)
    {
        if (mensaje is null) return null;
        if (mensaje.Remitente is null) return null;
        if (mensaje.Buzon is null || mensaje.Buzon.Usuario is null) return null;

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

    /// <summary>Devuelve null si la lista es null o si algún mensaje no se pudo mapear.</summary>
    public static List<MensajeRespuesta>? MapearA(List<Mensaje>? mensajes)
    {
        if (mensajes is null) return null;

        var respuesta = new List<MensajeRespuesta>();
        foreach (var mensaje in mensajes)
        {
            var mapeado = MapearA(mensaje);
            if (mapeado is null) return null;
            respuesta.Add(mapeado);
        }
        return respuesta;
    }
}
