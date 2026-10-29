using Mensajeria.Api.Dtos;
using Mensajeria.Api.Modelos;

namespace Mensajeria.Api.Mapeos;

public static class MapeoUsuario
{
    /// <summary>Convierte un Usuario en su DTO (el hash de la contraseña no se copia). Devuelve null si el usuario es null.</summary>
    public static UsuarioRespuesta? MapearA(Usuario? usuario)
    {
        if (usuario is null) return null;

        return new UsuarioRespuesta
        {
            Id = usuario.Id,
            NombreUsuario = usuario.NombreUsuario,
            Correo = usuario.Correo,
            CreadoEn = usuario.CreadoEn
        };
    }

    /// <summary>Arma la respuesta del login. Devuelve null si falta el usuario o el token.</summary>
    public static TokenRespuesta? MapearA(Usuario? usuario, string? token, DateTime expiraEn)
    {
        if (usuario is null || string.IsNullOrEmpty(token)) return null;

        return new TokenRespuesta
        {
            Token = token,
            ExpiraEn = expiraEn,
            NombreUsuario = usuario.NombreUsuario
        };
    }
}
