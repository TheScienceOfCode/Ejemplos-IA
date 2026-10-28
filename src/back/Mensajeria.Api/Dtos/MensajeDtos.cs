using System.ComponentModel.DataAnnotations;

namespace Mensajeria.Api.Dtos;

public class EnviarMensajeSolicitud
{
    [Required]
    public string DestinatarioNombreUsuario { get; set; } = "";

    [Required, StringLength(200)]
    public string Asunto { get; set; } = "";

    [Required, StringLength(4000)]
    public string Contenido { get; set; } = "";
}

public class MensajeRespuesta
{
    public int Id { get; set; }
    public string Remitente { get; set; } = "";
    public string Destinatario { get; set; } = "";
    public string Asunto { get; set; } = "";
    public string Contenido { get; set; } = "";
    public bool EstaLeido { get; set; }
    public DateTime CreadoEn { get; set; }
}

public class BuzonRespuesta
{
    public int BuzonId { get; set; }
    public string Propietario { get; set; } = "";
    public int NoLeidos { get; set; }
    public List<MensajeRespuesta> Mensajes { get; set; } = new();
}
