using Mensajeria.Api.Dtos;
using Mensajeria.Api.Modelos;

namespace Mensajeria.Api.Mapeos;

public static class MapeoUsuario
{
    /// <summary>Convierte un Usuario en su DTO. El hash de la contraseña no se copia.</summary>
    public static UsuarioRespuesta Respuesta(Usuario usuario)
    {
        if (usuario is null) throw new ArgumentNullException(nameof(usuario));

        return new UsuarioRespuesta
        {
            Id = usuario.Id,
            NombreUsuario = usuario.NombreUsuario,
            Correo = usuario.Correo,
            CreadoEn = usuario.CreadoEn
        };
    }

    /// <summary>Arma la respuesta del login.</summary>
    public static TokenRespuesta RespuestaToken(Usuario usuario, string token, DateTime expiraEn)
    {
        if (usuario is null) throw new ArgumentNullException(nameof(usuario));
        if (string.IsNullOrEmpty(token)) throw new ArgumentNullException(nameof(token));

        return new TokenRespuesta
        {
            Token = token,
            ExpiraEn = expiraEn,
            NombreUsuario = usuario.NombreUsuario
        };
    }
}
