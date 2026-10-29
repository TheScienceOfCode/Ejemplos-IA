using Mensajeria.Api.Dtos;
using Mensajeria.Api.Modelos;

namespace Mensajeria.Api.Mapeos;

public static class MapeoBuzon
{
    /// <summary>Requiere que el buzón venga con Usuario y MensajesRecibidos (con su Remitente) cargados.</summary>
    public static BuzonRespuesta MapearA(Buzon buzon)
    {
        return new BuzonRespuesta
        {
            BuzonId = buzon.Id,
            Propietario = buzon.Usuario.NombreUsuario,
            NoLeidos = buzon.MensajesRecibidos.Count(m => !m.EstaLeido),
            Mensajes = MapeoMensaje.MapearA(buzon.MensajesRecibidos)
        };
    }
}
