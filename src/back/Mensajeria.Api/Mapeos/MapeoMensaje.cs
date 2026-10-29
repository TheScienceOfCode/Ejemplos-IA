using Mensajeria.Api.Dtos;
using Mensajeria.Api.Modelos;

namespace Mensajeria.Api.Mapeos;

public static class MapeoMensaje
{
    /// <summary>Convierte un Mensaje en su DTO. Necesita el Remitente y el Buzon.Usuario cargados (Include).</summary>
    public static MensajeRespuesta Respuesta(Mensaje mensaje)
    {
        if (mensaje is null) throw new ArgumentNullException(nameof(mensaje));
        if (mensaje.Remitente is null) throw new ArgumentNullException(nameof(mensaje.Remitente), "Falta cargar el remitente del mensaje.");
        if (mensaje.Buzon is null) throw new ArgumentNullException(nameof(mensaje.Buzon), "Falta cargar el buzón del mensaje.");
        if (mensaje.Buzon.Usuario is null) throw new ArgumentNullException(nameof(mensaje.Buzon.Usuario), "Falta cargar el usuario del buzón.");

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

    public static List<MensajeRespuesta> Respuesta(List<Mensaje> mensajes)
    {
        if (mensajes is null) throw new ArgumentNullException(nameof(mensajes));

        var respuesta = new List<MensajeRespuesta>();
        foreach (var mensaje in mensajes)
            respuesta.Add(Respuesta(mensaje));
        return respuesta;
    }
}
