using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Mensajeria.Api.Utilidades;

public static class UsuarioActual
{
    /// <summary>Id del usuario autenticado, leído del token JWT (claim "sub"). Devuelve null si no es válido.</summary>
    public static int? ObtenerId(this ClaimsPrincipal usuario)
    {
        var valor = usuario.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return int.TryParse(valor, out var id) ? id : null;
    }
}
