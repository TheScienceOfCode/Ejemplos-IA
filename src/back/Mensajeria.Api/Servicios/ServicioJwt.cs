using System.Text;
using Mensajeria.Api.Modelos;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Mensajeria.Api.Servicios;

/// <summary>Crea los tokens JWT con la clave configurada en "Jwt:Clave".</summary>
public class ServicioJwt : IServicioJwt
{
    private readonly IConfiguration _config;

    public ServicioJwt(IConfiguration config)
    {
        _config = config;
    }

    public (string Token, DateTime ExpiraEn) CrearToken(Usuario usuario)
    {
        var minutos = _config.GetValue("Jwt:MinutosExpiracion", 60);
        var expira = DateTime.UtcNow.AddMinutes(minutos);
        var clave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Clave"]!));

        var descriptor = new SecurityTokenDescriptor
        {
            // El Id del usuario viaja DENTRO del token (claim "sub"). Los controladores lo leen de aquí,
            // nunca de lo que envíe el cliente.
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = usuario.Id.ToString(),
                [JwtRegisteredClaimNames.UniqueName] = usuario.NombreUsuario
            },
            Issuer = _config["Jwt:Emisor"],
            Audience = _config["Jwt:Audiencia"],
            Expires = expira,
            SigningCredentials = new SigningCredentials(clave, SecurityAlgorithms.HmacSha256)
        };

        return (new JsonWebTokenHandler().CreateToken(descriptor), expira);
    }
}
