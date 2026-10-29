using Mensajeria.Api.Datos;
using Mensajeria.Api.Modelos;
using Microsoft.EntityFrameworkCore;

namespace Mensajeria.Api.Servicios;

/// <summary>
/// Lógica de mensajería. Todos los métodos reciben el Id del usuario autenticado
/// (sacado del token) para comprobar que solo se accede a lo que le pertenece.
/// Devuelve entidades; convertirlas a DTO es trabajo de los controladores (ver Mapeos/).
/// </summary>
public class ServicioMensajes : IServicioMensajes
{
    private readonly MensajeriaDbContext _db;

    public ServicioMensajes(MensajeriaDbContext db)
    {
        _db = db;
    }

    public async Task<Mensaje?> EnviarAsync(int remitenteId, string destinatarioNombreUsuario, string asunto, string contenido)
    {
        var nombreDestino = destinatarioNombreUsuario.Trim();

        var buzonDestino = await _db.Buzones.FirstOrDefaultAsync(b => b.Usuario.NombreUsuario == nombreDestino);
        if (buzonDestino is null) return null;

        var mensaje = new Mensaje
        {
            RemitenteId = remitenteId,
            BuzonId = buzonDestino.Id,
            Asunto = asunto.Trim(),
            Contenido = contenido
        };
        _db.Mensajes.Add(mensaje);
        await _db.SaveChangesAsync();

        return await ConMensajesCompletos().FirstAsync(m => m.Id == mensaje.Id);
    }

    public async Task<Buzon> ObtenerBuzonPorUsuarioIdAsync(int usuarioId)
    {
        return await _db.Buzones
            .Include(b => b.Usuario)
            .Include(b => b.MensajesRecibidos.OrderByDescending(m => m.CreadoEn).ThenByDescending(m => m.Id))
                .ThenInclude(m => m.Remitente)
            .FirstAsync(b => b.UsuarioId == usuarioId);
    }

    public async Task<List<Mensaje>> ObtenerEnviadosAsync(int usuarioId)
    {
        return await ConMensajesCompletos()
            .Where(m => m.RemitenteId == usuarioId)
            .OrderByDescending(m => m.CreadoEn).ThenByDescending(m => m.Id)
            .ToListAsync();
    }

    public async Task<Mensaje?> ObtenerMensajeAsync(int mensajeId, int usuarioId)
    {
        // Solo pueden verlo quien lo envió y quien lo recibió
        return await ConMensajesCompletos()
            .FirstOrDefaultAsync(m => m.Id == mensajeId && (m.RemitenteId == usuarioId || m.Buzon.UsuarioId == usuarioId));
    }

    public async Task<Mensaje?> MarcarComoLeidoAsync(int mensajeId, int usuarioId)
    {
        // Solo el DESTINATARIO puede marcarlo como leído (ni siquiera el remitente)
        var mensaje = await ConMensajesCompletos()
            .FirstOrDefaultAsync(m => m.Id == mensajeId && m.Buzon.UsuarioId == usuarioId);
        if (mensaje is null) return null;

        mensaje.EstaLeido = true;
        await _db.SaveChangesAsync();

        return mensaje;
    }

    /// <summary>Consulta de mensajes con el remitente y el destinatario ya cargados.</summary>
    private IQueryable<Mensaje> ConMensajesCompletos()
    {
        return _db.Mensajes
            .Include(m => m.Remitente)
            .Include(m => m.Buzon).ThenInclude(b => b.Usuario);
    }
}
