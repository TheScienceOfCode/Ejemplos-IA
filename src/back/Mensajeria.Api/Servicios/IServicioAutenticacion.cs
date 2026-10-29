using Mensajeria.Api.Dtos;

namespace Mensajeria.Api.Servicios;

public interface IServicioAutenticacion
{
    /// <summary>Devuelve null si el nombre o el correo ya existen.</summary>
    Task<UsuarioRespuesta?> RegistrarAsync(RegistroSolicitud solicitud);

    /// <summary>Devuelve null si el usuario o la contraseña son incorrectos.</summary>
    Task<TokenRespuesta?> LoginAsync(LoginSolicitud solicitud);
}
