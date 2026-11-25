using Microsoft.AspNetCore.Http;

namespace G1.Identity.Mock.Middleware
{
    /// <summary>
    /// Añade las cabeceras de seguridad exigidas por el contrato.
    /// </summary>
    internal sealed class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Inicializa el middleware con el siguiente delegado del pipeline.
        /// </summary>
        /// <param name="next">Siguiente delegado del pipeline.</param>
        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Añade cabeceras de seguridad a todas las respuestas y una CSP específica en Swagger UI.
        /// </summary>
        /// <param name="context">Contexto HTTP de la petición.</param>
        /// <returns>Una tarea que finaliza cuando la petición ha sido procesada.</returns>
        public Task InvokeAsync(HttpContext context)
        {
            var headers = context.Response.Headers;

            headers[Constants.Headers.ContentTypeOptions] = Constants.Headers.NoSniff;
            headers[Constants.Headers.FrameOptions] = Constants.Headers.Deny;
            headers[Constants.Headers.ReferrerPolicy] = Constants.Headers.NoReferrer;

            if (context.Request.Path.StartsWithSegments(
                    new PathString(Constants.Paths.SwaggerPrefix),
                    StringComparison.OrdinalIgnoreCase))
            {
                headers[Constants.Headers.ContentSecurityPolicy] =
                    Constants.SecurityHeaders.SwaggerContentSecurityPolicy;
            }

            return _next(context);
        }
    }
}
