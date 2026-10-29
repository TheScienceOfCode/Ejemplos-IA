using Mensajeria.Api.Modelos;

namespace Mensajeria.Api.Servicios;

public interface IServicioAutenticacion
{
    /// <summary>Crea el usuario y su buzón. Devuelve null si el nombre o el correo ya existen.</summary>
    Task<Usuario?> RegistrarAsync(string nombreUsuario, string correo, string contrasena);

    /// <summary>Devuelve el usuario si las credenciales son correctas, o null si no lo son.</summary>
    Task<Usuario?> ValidarCredencialesAsync(string nombreUsuario, string contrasena);
}
