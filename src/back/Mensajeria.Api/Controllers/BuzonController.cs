using Mensajeria.Api.Dtos;
using Mensajeria.Api.Mapeos;
using Mensajeria.Api.Servicios;
using Mensajeria.Api.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mensajeria.Api.Controllers;

/// <summary>Buzón de entrada (mensajes recibidos).</summary>
[ApiController]
[Route("api/buzon")]
[Authorize]
public class BuzonController : ControllerBase
{
    private readonly IServicioMensajes _mensajes;

    public BuzonController(IServicioMensajes mensajes)
    {
        _mensajes = mensajes;
    }

    /// <summary>Muestra el buzón del usuario autenticado, con sus mensajes recibidos.</summary>
    [HttpGet]
    [ProducesResponseType<BuzonRespuesta>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MiBuzon()
    {
        var usuarioId = User.ObtenerId();
        if (usuarioId is null) return Unauthorized();

        var buzon = await _mensajes.ObtenerBuzonPorUsuarioIdAsync(usuarioId.Value);
        var respuesta = MapeoBuzon.MapearA(buzon);
        if (respuesta is null) return Problem("No se pudo construir la respuesta.");

        return Ok(respuesta);
    }
}
