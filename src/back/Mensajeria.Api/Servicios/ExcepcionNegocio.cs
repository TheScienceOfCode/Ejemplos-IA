namespace Mensajeria.Api.Servicios;

/// <summary>
/// Error esperado de la lógica de negocio (usuario repetido, mensaje ajeno, etc.).
/// Los servicios la lanzan y <see cref="ManejadorExcepciones"/> la convierte en la respuesta HTTP correcta.
/// </summary>
public class ExcepcionNegocio : Exception
{
    public int CodigoHttp { get; }

    public ExcepcionNegocio(int codigoHttp, string mensaje) : base(mensaje)
    {
        CodigoHttp = codigoHttp;
    }
}
