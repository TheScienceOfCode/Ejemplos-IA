using Mensajeria.Api.Dtos;
using Mensajeria.Api.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mensajeria.Api.Controllers;

/// <summary>Registro de usuarios e inicio de sesión (endpoints públicos).</summary>
[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IServicioAutenticacion _auth;

    public AuthController(IServicioAutenticacion auth)
    {
        _auth = auth;
    }

    /// <summary>Registra un usuario nuevo. Su buzón se crea automáticamente.</summary>
    [HttpPost("registro")]
    [ProducesResponseType<UsuarioRespuesta>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Registro(RegistroSolicitud solicitud)
    {
        var usuario = await _auth.RegistrarAsync(solicitud);
        if (usuario is null) return Conflict("El nombre de usuario o el correo ya están registrados.");

        return StatusCode(StatusCodes.Status201Created, usuario);
    }

    /// <summary>Inicia sesión y devuelve un token JWT para usar en el botón "Authorize" de Swagger.</summary>
    [HttpPost("login")]
    [ProducesResponseType<TokenRespuesta>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginSolicitud solicitud)
    {
        var token = await _auth.LoginAsync(solicitud);
        if (token is null) return Unauthorized("Usuario o contraseña incorrectos.");

        return Ok(token);
    }
}
