using System.Linq.Expressions;
using Mensajeria.Api.Datos;
using Mensajeria.Api.Dtos;
using Mensajeria.Api.Modelos;
using Microsoft.EntityFrameworkCore;

namespace Mensajeria.Api.Servicios;

// Los métodos que devuelven "?" dan null cuando no se encontró lo pedido o no pertenece al usuario.
public interface IServicioMensajes
{
    Task<MensajeRespuesta?> EnviarAsync(int remitenteId, EnviarMensajeSolicitud solicitud);
    Task<BuzonRespuesta> ObtenerMiBuzonAsync(int usuarioId);
    Task<List<MensajeRespuesta>> ObtenerEnviadosAsync(int usuarioId);
    Task<MensajeRespuesta?> ObtenerMensajeAsync(int mensajeId, int usuarioId);
    Task<MensajeRespuesta?> MarcarComoLeidoAsync(int mensajeId, int usuarioId);
}

/// <summary>
/// Lógica de mensajería. Todos los métodos reciben el Id del usuario autenticado
/// (sacado del token) para comprobar que solo se accede a lo que le pertenece.
/// </summary>
public class ServicioMensajes : IServicioMensajes
{
    private readonly MensajeriaDbContext _db;

    public ServicioMensajes(MensajeriaDbContext db)
    {
        _db = db;
    }

    // Cómo convertir un Mensaje (entidad) en MensajeRespuesta (DTO). EF lo traduce a SQL.
    private static readonly Expression<Func<Mensaje, MensajeRespuesta>> ADto = m => new MensajeRespuesta
    {
        Id = m.Id,
        Remitente = m.Remitente.NombreUsuario,
        Destinatario = m.Buzon.Usuario.NombreUsuario,
        Asunto = m.Asunto,
        Contenido = m.Contenido,
        EstaLeido = m.EstaLeido,
        CreadoEn = m.CreadoEn
    };

    public async Task<MensajeRespuesta?> EnviarAsync(int remitenteId, EnviarMensajeSolicitud solicitud)
    {
        var nombreDestino = solicitud.DestinatarioNombreUsuario.Trim();

        var buzonDestino = await _db.Buzones.FirstOrDefaultAsync(b => b.Usuario.NombreUsuario == nombreDestino);
        if (buzonDestino is null) return null;

        var mensaje = new Mensaje
        {
            RemitenteId = remitenteId,
            BuzonId = buzonDestino.Id,
            Asunto = solicitud.Asunto.Trim(),
            Contenido = solicitud.Contenido
        };
        _db.Mensajes.Add(mensaje);
        await _db.SaveChangesAsync();

        return await _db.Mensajes.Where(m => m.Id == mensaje.Id).Select(ADto).SingleAsync();
    }

    public async Task<BuzonRespuesta> ObtenerMiBuzonAsync(int usuarioId)
    {
        var buzon = await _db.Buzones.FirstAsync(b => b.UsuarioId == usuarioId);
        return await ConstruirBuzonAsync(buzon.Id);
    }

    public async Task<List<MensajeRespuesta>> ObtenerEnviadosAsync(int usuarioId)
    {
        return await _db.Mensajes
            .Where(m => m.RemitenteId == usuarioId)
            .OrderByDescending(m => m.CreadoEn).ThenByDescending(m => m.Id)
            .Select(ADto)
            .ToListAsync();
    }

    public async Task<MensajeRespuesta?> ObtenerMensajeAsync(int mensajeId, int usuarioId)
    {
        // Solo pueden verlo quien lo envió y quien lo recibió
        return await _db.Mensajes
            .Where(m => m.Id == mensajeId && (m.RemitenteId == usuarioId || m.Buzon.UsuarioId == usuarioId))
            .Select(ADto)
            .FirstOrDefaultAsync();
    }

    public async Task<MensajeRespuesta?> MarcarComoLeidoAsync(int mensajeId, int usuarioId)
    {
        // Solo el DESTINATARIO puede marcarlo como leído (ni siquiera el remitente)
        var mensaje = await _db.Mensajes
            .FirstOrDefaultAsync(m => m.Id == mensajeId && m.Buzon.UsuarioId == usuarioId);
        if (mensaje is null) return null;

        mensaje.EstaLeido = true;
        await _db.SaveChangesAsync();

        return await _db.Mensajes.Where(m => m.Id == mensajeId).Select(ADto).SingleAsync();
    }

    private async Task<BuzonRespuesta> ConstruirBuzonAsync(int buzonId)
    {
        var propietario = await _db.Buzones
            .Where(b => b.Id == buzonId)
            .Select(b => b.Usuario.NombreUsuario)
            .SingleAsync();

        var mensajes = await _db.Mensajes
            .Where(m => m.BuzonId == buzonId)
            .OrderByDescending(m => m.CreadoEn).ThenByDescending(m => m.Id)
            .Select(ADto)
            .ToListAsync();

        return new BuzonRespuesta
        {
            BuzonId = buzonId,
            Propietario = propietario,
            NoLeidos = mensajes.Count(m => !m.EstaLeido),
            Mensajes = mensajes
        };
    }
}
