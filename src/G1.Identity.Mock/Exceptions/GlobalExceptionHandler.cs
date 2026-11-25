using Microsoft.AspNetCore.Diagnostics;

namespace G1.Identity.Mock.Exceptions
{
    /// <summary>
    /// Manejador global que convierte excepciones no controladas en problemas RFC 9457.
    /// </summary>
    internal sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        /// <summary>
        /// Inicializa el manejador con el registrador del tipo concreto.
        /// </summary>
        /// <param name="logger">Registrador de diagnóstico.</param>
        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(
                exception,
                "Error inesperado al procesar la petición con correlación {CorrelationId}",
                ProblemResponseWriter.GetTraceId(httpContext));

            await ProblemResponseWriter.WriteAsync(
                httpContext,
                StatusCodes.Status500InternalServerError,
                Constants.ErrorCodes.InternalError,
                cancellationToken);

            return true;
        }
    }
}
