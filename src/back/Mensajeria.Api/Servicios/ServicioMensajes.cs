using System.Linq.Expressions;
using Mensajeria.Api.Datos;
using Mensajeria.Api.Dtos;
using Mensajeria.Api.Modelos;
using Microsoft.EntityFrameworkCore;

namespace Mensajeria.Api.Servicios;

public interface IServicioMensajes
{
    Task<MensajeRespuesta> EnviarAsync(int remitenteId, EnviarMensajeSolicitud solicitud);
    Task<BuzonRespuesta> ObtenerMiBuzonAsync(int usuarioId);
    Task<BuzonRespuesta> ObtenerBuzonPorIdAsync(int buzonId, int usuarioId);
    Task<List<MensajeRespuesta>> ObtenerEnviadosAsync(int usuarioId);
    Task<MensajeRespuesta> ObtenerMensajeAsync(int mensajeId, int usuarioId);
    Task<MensajeRespuesta> MarcarComoLeidoAsync(int mensajeId, int usuarioId);
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

    public async Task<MensajeRespuesta> EnviarAsync(int remitenteId, EnviarMensajeSolicitud solicitud)
    {
        var nombreDestino = solicitud.DestinatarioNombreUsuario.Trim();

        var buzonDestino = await _db.Buzones.FirstOrDefaultAsync(b => b.Usuario.NombreUsuario == nombreDestino);
        if (buzonDestino is null)
            throw new ExcepcionNegocio(StatusCodes.Status404NotFound, $"No existe el usuario '{nombreDestino}'.");

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

    public async Task<BuzonRespuesta> ObtenerBuzonPorIdAsync(int buzonId, int usuarioId)
    {
        var buzon = await _db.Buzones.FirstOrDefaultAsync(b => b.Id == buzonId);
        if (buzon is null)
            throw new ExcepcionNegocio(StatusCodes.Status404NotFound, "El buzón no existe.");

        // Comparamos con el usuario del TOKEN: nadie puede abrir el buzón de otra persona
        if (buzon.UsuarioId != usuarioId)
            throw new ExcepcionNegocio(StatusCodes.Status403Forbidden, "Este buzón pertenece a otro usuario.");

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

    public async Task<MensajeRespuesta> ObtenerMensajeAsync(int mensajeId, int usuarioId)
    {
        var mensaje = await _db.Mensajes
            .Include(m => m.Buzon)
            .FirstOrDefaultAsync(m => m.Id == mensajeId);
        if (mensaje is null)
            throw new ExcepcionNegocio(StatusCodes.Status404NotFound, "El mensaje no existe.");

        // Solo pueden verlo quien lo envió y quien lo recibió
        var esRemitente = mensaje.RemitenteId == usuarioId;
        var esDestinatario = mensaje.Buzon.UsuarioId == usuarioId;
        if (!esRemitente && !esDestinatario)
            throw new ExcepcionNegocio(StatusCodes.Status403Forbidden, "No tienes acceso a este mensaje.");

        return await _db.Mensajes.Where(m => m.Id == mensajeId).Select(ADto).SingleAsync();
    }

    public async Task<MensajeRespuesta> MarcarComoLeidoAsync(int mensajeId, int usuarioId)
    {
        var mensaje = await _db.Mensajes
            .Include(m => m.Buzon)
            .FirstOrDefaultAsync(m => m.Id == mensajeId);
        if (mensaje is null)
            throw new ExcepcionNegocio(StatusCodes.Status404NotFound, "El mensaje no existe.");

        // Solo el DESTINATARIO puede marcarlo como leído (ni siquiera el remitente)
        if (mensaje.Buzon.UsuarioId != usuarioId)
            throw new ExcepcionNegocio(StatusCodes.Status403Forbidden, "Solo el destinatario puede marcar el mensaje como leído.");

        if (!mensaje.EstaLeido)
        {
            mensaje.EstaLeido = true;
            await _db.SaveChangesAsync();
        }

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
