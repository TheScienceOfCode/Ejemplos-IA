using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Mensajeria.Api.Servicios;

/// <summary>Convierte las <see cref="ExcepcionNegocio"/> en respuestas HTTP (400, 401, 403, 404, 409...).</summary>
public class ManejadorExcepciones : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext contexto, Exception excepcion, CancellationToken ct)
    {
        if (excepcion is not ExcepcionNegocio error)
            return false; // cualquier otra excepción sigue siendo un 500

        contexto.Response.StatusCode = error.CodigoHttp;
        await contexto.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = error.CodigoHttp,
            Title = error.Message
        }, ct);
        return true;
    }
}
