using Mensajeria.Api.Dtos;

namespace Mensajeria.Api.Servicios;

/// <summary>
/// Operaciones de mensajería. Todas reciben el Id del usuario autenticado (sacado del token).
/// Los métodos que devuelven "?" dan null cuando no se encontró lo pedido o no pertenece a ese usuario.
/// </summary>
public interface IServicioMensajes
{
    Task<MensajeRespuesta?> EnviarAsync(int remitenteId, EnviarMensajeSolicitud solicitud);
    Task<BuzonRespuesta> ObtenerBuzonPorUsuarioIdAsync(int usuarioId);
    Task<List<MensajeRespuesta>> ObtenerEnviadosAsync(int usuarioId);
    Task<MensajeRespuesta?> ObtenerMensajeAsync(int mensajeId, int usuarioId);
    Task<MensajeRespuesta?> MarcarComoLeidoAsync(int mensajeId, int usuarioId);
}
