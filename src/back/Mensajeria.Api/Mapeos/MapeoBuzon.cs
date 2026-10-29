using Mensajeria.Api.Dtos;
using Mensajeria.Api.Modelos;

namespace Mensajeria.Api.Mapeos;

public static class MapeoBuzon
{
    /// <summary>Convierte un Buzon en su DTO. Necesita el Usuario y los MensajesRecibidos cargados (Include).</summary>
    public static BuzonRespuesta Respuesta(Buzon buzon)
    {
        if (buzon is null) throw new ArgumentNullException(nameof(buzon));
        if (buzon.Usuario is null) throw new ArgumentNullException(nameof(buzon.Usuario), "Falta cargar el usuario del buzón.");
        if (buzon.MensajesRecibidos is null) throw new ArgumentNullException(nameof(buzon.MensajesRecibidos), "Faltan cargar los mensajes del buzón.");

        var mensajes = MapeoMensaje.Respuesta(buzon.MensajesRecibidos);

        return new BuzonRespuesta
        {
            BuzonId = buzon.Id,
            Propietario = buzon.Usuario.NombreUsuario,
            NoLeidos = mensajes.Count(m => !m.EstaLeido),
            Mensajes = mensajes
        };
    }
}
