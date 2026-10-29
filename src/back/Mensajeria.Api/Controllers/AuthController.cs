using Mensajeria.Api.Dtos;
using Mensajeria.Api.Mapeos;
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
    private readonly IServicioJwt _jwt;

    public AuthController(IServicioAutenticacion auth, IServicioJwt jwt)
    {
        _auth = auth;
        _jwt = jwt;
    }

    /// <summary>Registra un usuario nuevo. Su buzón se crea automáticamente.</summary>
    [HttpPost("registro")]
    [ProducesResponseType<UsuarioRespuesta>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Registro(RegistroSolicitud solicitud)
    {
        var usuario = await _auth.RegistrarAsync(solicitud.NombreUsuario, solicitud.Correo, solicitud.Contrasena);
        if (usuario is null) return Conflict("El nombre de usuario o el correo ya están registrados.");

        var respuesta = MapeoUsuario.Respuesta(usuario);

        return StatusCode(StatusCodes.Status201Created, respuesta);
    }

    /// <summary>Inicia sesión y devuelve un token JWT para usar en el botón "Authorize" de Swagger.</summary>
    [HttpPost("login")]
    [ProducesResponseType<TokenRespuesta>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginSolicitud solicitud)
    {
        var usuario = await _auth.ValidarCredencialesAsync(solicitud.NombreUsuario, solicitud.Contrasena);
        if (usuario is null) return Unauthorized("Usuario o contraseña incorrectos.");

        var (token, expiraEn) = _jwt.CrearToken(usuario);
        var respuesta = MapeoUsuario.RespuestaToken(usuario, token, expiraEn);

        return Ok(respuesta);
    }
}
