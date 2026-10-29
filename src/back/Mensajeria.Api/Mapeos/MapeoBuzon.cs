using Mensajeria.Api.Dtos;
using Mensajeria.Api.Modelos;

namespace Mensajeria.Api.Mapeos;

public static class MapeoBuzon
{
    /// <summary>
    /// Devuelve null si el buzón es null o si no trae cargados su usuario y sus mensajes recibidos
    /// (es decir, si faltó un Include en la consulta del servicio).
    /// </summary>
    public static BuzonRespuesta? MapearA(Buzon? buzon)
    {
        if (buzon is null) return null;
        if (buzon.Usuario is null) return null;

        var mensajes = MapeoMensaje.MapearA(buzon.MensajesRecibidos);
        if (mensajes is null) return null;

        return new BuzonRespuesta
        {
            BuzonId = buzon.Id,
            Propietario = buzon.Usuario.NombreUsuario,
            NoLeidos = mensajes.Count(m => !m.EstaLeido),
            Mensajes = mensajes
        };
    }
}
