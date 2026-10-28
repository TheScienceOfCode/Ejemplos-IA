using Mensajeria.Api.Dtos;
using Mensajeria.Api.Servicios;
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
        var mensaje = await _mensajes.EnviarAsync(User.ObtenerId(), solicitud);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = mensaje.Id }, mensaje);
    }

    /// <summary>Lista los mensajes que has enviado.</summary>
    [HttpGet("enviados")]
    [ProducesResponseType<List<MensajeRespuesta>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Enviados()
    {
        return Ok(await _mensajes.ObtenerEnviadosAsync(User.ObtenerId()));
    }

    /// <summary>Lee un mensaje. Solo su remitente o su destinatario pueden verlo.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<MensajeRespuesta>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        return Ok(await _mensajes.ObtenerMensajeAsync(id, User.ObtenerId()));
    }

    /// <summary>Marca como leído un mensaje que recibiste. Solo el destinatario puede hacerlo.</summary>
    [HttpPut("{id:int}/leido")]
    [ProducesResponseType<MensajeRespuesta>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarcarComoLeido(int id)
    {
        return Ok(await _mensajes.MarcarComoLeidoAsync(id, User.ObtenerId()));
    }
}
