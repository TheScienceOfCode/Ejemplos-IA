using Mensajeria.Api.Modelos;

namespace Mensajeria.Api.Servicios;

/// <summary>
/// Operaciones de mensajería. Todas reciben el Id del usuario autenticado (sacado del token).
/// Los métodos que devuelven "?" dan null cuando no se encontró lo pedido o no pertenece a ese usuario.
/// </summary>
public interface IServicioMensajes
{
    Task<Mensaje?> EnviarAsync(int remitenteId, string destinatarioNombreUsuario, string asunto, string contenido);
    Task<Buzon> ObtenerBuzonPorUsuarioIdAsync(int usuarioId);
    Task<List<Mensaje>> ObtenerEnviadosAsync(int usuarioId);
    Task<Mensaje?> ObtenerMensajeAsync(int mensajeId, int usuarioId);
    Task<Mensaje?> MarcarComoLeidoAsync(int mensajeId, int usuarioId);
}
