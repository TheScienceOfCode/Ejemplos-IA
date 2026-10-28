namespace Mensajeria.Api.Modelos;

/// <summary>Persona registrada en el sistema.</summary>
public class Usuario
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = "";
    public string Correo { get; set; } = "";

    /// <summary>Hash generado con PasswordHasher. Nunca se guarda ni se devuelve la contraseña en texto plano.</summary>
    public string HashContrasena { get; set; } = "";

    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}
