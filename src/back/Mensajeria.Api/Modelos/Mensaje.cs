namespace Mensajeria.Api.Modelos;

/// <summary>Mensaje enviado por un usuario (remitente) al buzón de otro.</summary>
public class Mensaje
{
    public int Id { get; set; }

    public int RemitenteId { get; set; }
    public Usuario Remitente { get; set; } = null!;

    /// <summary>Buzón del destinatario.</summary>
    public int BuzonId { get; set; }
    public Buzon Buzon { get; set; } = null!;

    public string Asunto { get; set; } = "";
    public string Contenido { get; set; } = "";

    /// <summary>Indica si el destinatario ya lo marcó como leído.</summary>
    public bool EstaLeido { get; set; }

    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}
