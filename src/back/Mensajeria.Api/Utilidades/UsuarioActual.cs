using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Mensajeria.Api.Utilidades;

public static class UsuarioActual
{
    /// <summary>Id del usuario autenticado, leído del token JWT (claim "sub").</summary>
    public static int ObtenerId(this ClaimsPrincipal usuario)
    {
        var valor = usuario.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return int.Parse(valor!);
    }
}
