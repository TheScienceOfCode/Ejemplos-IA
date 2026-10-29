using Mensajeria.Api.Dtos;
using Mensajeria.Api.Modelos;

namespace Mensajeria.Api.Mapeos;

public static class MapeoUsuario
{
    /// <summary>Convierte un Usuario en su DTO. Nota: el hash de la contraseña no se copia.</summary>
    public static UsuarioRespuesta MapearA(Usuario usuario)
    {
        return new UsuarioRespuesta
        {
            Id = usuario.Id,
            NombreUsuario = usuario.NombreUsuario,
            Correo = usuario.Correo,
            CreadoEn = usuario.CreadoEn
        };
    }

    public static TokenRespuesta MapearA(Usuario usuario, string token, DateTime expiraEn)
    {
        return new TokenRespuesta
        {
            Token = token,
            ExpiraEn = expiraEn,
            NombreUsuario = usuario.NombreUsuario
        };
    }
}
