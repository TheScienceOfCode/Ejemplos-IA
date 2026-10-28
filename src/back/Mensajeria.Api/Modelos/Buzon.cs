namespace Mensajeria.Api.Modelos;

/// <summary>Buzón de entrada de un usuario. Cada usuario tiene exactamente uno (se crea al registrarse).</summary>
public class Buzon
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    /// <summary>Mensajes que han llegado a este buzón.</summary>
    public List<Mensaje> MensajesRecibidos { get; set; } = new();
}
