using Mensajeria.Api.Dtos;
using Mensajeria.Api.Mapeos;
using Mensajeria.Api.Servicios;
using Mensajeria.Api.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mensajeria.Api.Controllers;

/// <summary>Enviar, consultar y marcar como leídos los mensajes.</summary>
[ApiController]
[Route("api/mensajes")]
[Authorize]
public class MensajesController : ControllerBase
{
    private readonly IServicioMensajes _mensajes;

    public MensajesController(IServicioMensajes mensajes)
    {
        _mensajes = mensajes;
    }

    /// <summary>Envía un mensaje a otro usuario registrado (el remitente eres tú, según tu token).</summary>
    [HttpPost]
    [ProducesResponseType<MensajeRespuesta>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Enviar(EnviarMensajeSolicitud solicitud)
    {
        var usuarioId = User.ObtenerId();
        if (usuarioId is null) return Unauthorized();

        var mensaje = await _mensajes.EnviarAsync(usuarioId.Value, solicitud.DestinatarioNombreUsuario, solicitud.Asunto, solicitud.Contenido);
        if (mensaje is null) return NotFound($"No existe el usuario '{solicitud.DestinatarioNombreUsuario}'.");

        return CreatedAtAction(nameof(ObtenerPorId), new { id = mensaje.Id }, MapeoMensaje.MapearA(mensaje));
    }

    /// <summary>Lista los mensajes que has enviado.</summary>
    [HttpGet("enviados")]
    [ProducesResponseType<List<MensajeRespuesta>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Enviados()
    {
        var usuarioId = User.ObtenerId();
        if (usuarioId is null) return Unauthorized();

        var mensajes = await _mensajes.ObtenerEnviadosAsync(usuarioId.Value);
        return Ok(MapeoMensaje.MapearA(mensajes));
    }

    /// <summary>Lee un mensaje. Solo su remitente o su destinatario pueden verlo (para los demás no existe).</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<MensajeRespuesta>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var usuarioId = User.ObtenerId();
        if (usuarioId is null) return Unauthorized();

        var mensaje = await _mensajes.ObtenerMensajeAsync(id, usuarioId.Value);
        if (mensaje is null) return NotFound("El mensaje no existe o no tienes acceso.");

        return Ok(MapeoMensaje.MapearA(mensaje));
    }

    /// <summary>Marca como leído un mensaje que recibiste. Solo el destinatario puede hacerlo.</summary>
    [HttpPut("{id:int}/leido")]
    [ProducesResponseType<MensajeRespuesta>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarcarComoLeido(int id)
    {
        var usuarioId = User.ObtenerId();
        if (usuarioId is null) return Unauthorized();

        var mensaje = await _mensajes.MarcarComoLeidoAsync(id, usuarioId.Value);
        if (mensaje is null) return NotFound("El mensaje no existe o no eres su destinatario.");

        return Ok(MapeoMensaje.MapearA(mensaje));
    }
}
