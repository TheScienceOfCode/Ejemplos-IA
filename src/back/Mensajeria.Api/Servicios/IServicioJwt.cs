using Mensajeria.Api.Modelos;

namespace Mensajeria.Api.Servicios;

public interface IServicioJwt
{
    (string Token, DateTime ExpiraEn) CrearToken(Usuario usuario);
}
