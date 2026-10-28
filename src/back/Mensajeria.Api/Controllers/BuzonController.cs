using Mensajeria.Api.Dtos;
using Mensajeria.Api.Servicios;
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
        return Ok(await _mensajes.ObtenerMiBuzonAsync(User.ObtenerId()));
    }

    /// <summary>Abre un buzón por su Id. Solo funciona si el buzón es tuyo; si es de otra persona responde 403.</summary>
    [HttpGet("{buzonId:int}")]
    [ProducesResponseType<BuzonRespuesta>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BuzonPorId(int buzonId)
    {
        return Ok(await _mensajes.ObtenerBuzonPorIdAsync(buzonId, User.ObtenerId()));
    }
}
