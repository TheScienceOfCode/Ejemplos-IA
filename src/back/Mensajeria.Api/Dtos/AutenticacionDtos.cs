using System.ComponentModel.DataAnnotations;

namespace Mensajeria.Api.Dtos;

// Los DTOs son los "contratos" de la API: lo que entra y lo que sale.
// Así nunca exponemos directamente las entidades de la base de datos (¡ni el hash de la contraseña!).

public class RegistroSolicitud
{
    [Required, StringLength(50, MinimumLength = 3)]
    public string NombreUsuario { get; set; } = "";

    [Required, EmailAddress, StringLength(200)]
    public string Correo { get; set; } = "";

    [Required, StringLength(100, MinimumLength = 6)]
    public string Contrasena { get; set; } = "";
}

public class LoginSolicitud
{
    [Required]
    public string NombreUsuario { get; set; } = "";

    [Required]
    public string Contrasena { get; set; } = "";
}

public class UsuarioRespuesta
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = "";
    public string Correo { get; set; } = "";
    public DateTime CreadoEn { get; set; }
}

public class TokenRespuesta
{
    public string Token { get; set; } = "";
    public DateTime ExpiraEn { get; set; }
    public string NombreUsuario { get; set; } = "";
}
