using System.Diagnostics;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;

namespace G1.Identity.Mock.Exceptions
{
    /// <summary>
    /// Escribe respuestas de error RFC 9457 sin exponer detalles internos.
    /// </summary>
    internal static class ProblemResponseWriter
    {
        /// <summary>
        /// Escribe un problema con el código estable y la traza de la petición.
        /// </summary>
        /// <param name="context">Contexto HTTP de la petición.</param>
        /// <param name="statusCode">Código de estado HTTP.</param>
        /// <param name="errorCode">Código estable del contrato.</param>
        /// <param name="cancellationToken">Token de cancelación de la petición.</param>
        /// <returns>Una tarea que finaliza cuando se ha escrito la respuesta.</returns>
        internal static Task WriteAsync(
            HttpContext context,
            int statusCode,
            string errorCode,
            CancellationToken cancellationToken = default)
        {
            var problem = new ProblemDetails
            {
                Type = ErrorCatalog.GetType(errorCode),
                Title = ErrorCatalog.GetTitle(errorCode),
                Status = statusCode,
                Detail = ErrorCatalog.GetDetail(errorCode),
                Instance = context.Request.Path.Value,
            };

            problem.Extensions[@"code"] = errorCode;
            problem.Extensions[@"traceId"] = GetTraceId(context);

            context.Response.StatusCode = statusCode;
            context.Response.Headers[Constants.Headers.CorrelationId] = GetTraceId(context);

            return context.Response.WriteAsJsonAsync(
                problem,
                options: null,
                contentType: Constants.ContentTypes.ProblemJson,
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Añade el intervalo de reintento calculado por el limitador de peticiones.
        /// </summary>
        /// <param name="response">Respuesta HTTP.</param>
        /// <param name="retryAfter">Intervalo hasta que haya licencias disponibles.</param>
        internal static void SetRetryAfter(HttpResponse response, TimeSpan retryAfter)
        {
            var seconds = (long)Math.Ceiling(retryAfter.TotalSeconds);
            response.Headers[Constants.Headers.RetryAfter] = seconds.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Obtiene el identificador de correlación o, en su defecto, el de la traza distribuida.
        /// </summary>
        /// <param name="context">Contexto HTTP de la petición.</param>
        /// <returns>Identificador que se devuelve al cliente.</returns>
        internal static string GetTraceId(HttpContext context)
        {
            if (context.Items.TryGetValue(Constants.HttpItems.CorrelationId, out var correlationId)
                && correlationId is string value
                && !string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            return Activity.Current?.TraceId.ToString() ?? string.Empty;
        }
    }
}
