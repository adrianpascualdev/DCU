using G1.Identity.Mock.Exceptions;

namespace G1.Identity.Mock.Endpoints
{
    /// <summary>
    /// Centraliza la traducción de errores de la API de negocio a respuestas RFC 9457.
    /// Los errores OAuth se mantienen fuera de este mapeo y conservan su formato estándar.
    /// </summary>
    internal static class ErrorMapping
    {
        /// <summary>
        /// Escribe una respuesta <c>application/problem+json</c> con el estado asociado al código estable.
        /// </summary>
        /// <param name="context">Contexto HTTP de la petición.</param>
        /// <param name="errorCode">Código estable de error de la API.</param>
        /// <param name="cancellationToken">Token de cancelación de la petición.</param>
        /// <returns>Una tarea que finaliza cuando se ha escrito la respuesta.</returns>
        internal static Task WriteProblemAsync(
            HttpContext context,
            string errorCode,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context);

            var normalizedCode = NormalizeErrorCode(errorCode);

            return ProblemResponseWriter.WriteAsync(
                context,
                GetStatusCode(normalizedCode),
                normalizedCode,
                cancellationToken);
        }

        /// <summary>
        /// Obtiene el estado HTTP documentado para un código de error de la API.
        /// </summary>
        /// <param name="errorCode">Código estable de error.</param>
        /// <returns>Estado HTTP documentado, o 500 si el código no pertenece al catálogo de la API.</returns>
        internal static int GetStatusCode(string errorCode) => errorCode switch
        {
            Constants.ErrorCodes.SessionRequired => StatusCodes.Status401Unauthorized,
            Constants.ErrorCodes.SessionInvalid => StatusCodes.Status401Unauthorized,
            Constants.ErrorCodes.PermissionDenied => StatusCodes.Status403Forbidden,
            Constants.ErrorCodes.UserNotFound => StatusCodes.Status404NotFound,
            Constants.ErrorCodes.RequestInvalid => StatusCodes.Status400BadRequest,
            Constants.ErrorCodes.RateLimited => StatusCodes.Status429TooManyRequests,
            Constants.ErrorCodes.InternalError => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status500InternalServerError,
        };

        /// <summary>
        /// Evita exponer códigos desconocidos, internos o pertenecientes al protocolo OAuth.
        /// </summary>
        /// <param name="errorCode">Código recibido por la capa de endpoints.</param>
        /// <returns>El código si pertenece al catálogo de la API; en otro caso, el error interno estable.</returns>
        internal static string NormalizeErrorCode(string errorCode) => errorCode switch
        {
            Constants.ErrorCodes.SessionRequired => Constants.ErrorCodes.SessionRequired,
            Constants.ErrorCodes.SessionInvalid => Constants.ErrorCodes.SessionInvalid,
            Constants.ErrorCodes.PermissionDenied => Constants.ErrorCodes.PermissionDenied,
            Constants.ErrorCodes.UserNotFound => Constants.ErrorCodes.UserNotFound,
            Constants.ErrorCodes.RequestInvalid => Constants.ErrorCodes.RequestInvalid,
            Constants.ErrorCodes.RateLimited => Constants.ErrorCodes.RateLimited,
            Constants.ErrorCodes.InternalError => Constants.ErrorCodes.InternalError,
            _ => Constants.ErrorCodes.InternalError,
        };
    }
}
