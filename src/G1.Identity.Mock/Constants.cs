namespace G1.Identity.Mock
{
    /// <summary>
    /// Constantes internas del mock de identidad.
    /// </summary>
    internal static class Constants
    {
        /// <summary>
        /// Claves de configuración enlazadas por el patrón Options.
        /// </summary>
        internal static class Configuration
        {
            internal const string Identity = @"Identity";
            internal const string Lifetimes = @"Identity:Lifetimes";
            internal const string Session = @"Identity:Session";
            internal const string Cors = @"Identity:Cors";
        }

        /// <summary>
        /// Códigos estables definidos en el contrato para errores de la API.
        /// </summary>
        internal static class ErrorCodes
        {
            internal const string SessionRequired = @"identity.session.required";
            internal const string SessionInvalid = @"identity.session.invalid";
            internal const string PermissionDenied = @"identity.permission.denied";
            internal const string UserNotFound = @"identity.user.not_found";
            internal const string RequestInvalid = @"identity.request.invalid";
            internal const string RateLimited = @"identity.rate_limited";
            internal const string InternalError = @"identity.internal_error";
        }

        /// <summary>
        /// Identificadores RFC 9457 de los errores de la API.
        /// </summary>
        internal static class ErrorTypes
        {
            internal const string SessionRequired = @"https://identity.example.test/errors/session-required";
            internal const string SessionInvalid = @"https://identity.example.test/errors/session-invalid";
            internal const string PermissionDenied = @"https://identity.example.test/errors/permission-denied";
            internal const string UserNotFound = @"https://identity.example.test/errors/user-not-found";
            internal const string RequestInvalid = @"https://identity.example.test/errors/request-invalid";
            internal const string RateLimited = @"https://identity.example.test/errors/rate-limited";
            internal const string InternalError = @"https://identity.example.test/errors/internal";
        }

        /// <summary>
        /// Cabeceras HTTP utilizadas por el pipeline.
        /// </summary>
        internal static class Headers
        {
            internal const string CorrelationId = @"X-Correlation-ID";
            internal const string ContentTypeOptions = @"X-Content-Type-Options";
            internal const string ContentSecurityPolicy = @"Content-Security-Policy";
            internal const string FrameOptions = @"X-Frame-Options";
            internal const string ReferrerPolicy = @"Referrer-Policy";
            internal const string RetryAfter = @"Retry-After";
            internal const string AntiforgeryToken = @"X-XSRF-TOKEN";
            internal const string NoSniff = @"nosniff";
            internal const string Deny = @"DENY";
            internal const string NoReferrer = @"no-referrer";
        }

        /// <summary>
        /// Claves del contexto HTTP compartidas entre middlewares.
        /// </summary>
        internal static class HttpItems
        {
            internal const string CorrelationId = @"G1.CorrelationId";
        }

        /// <summary>
        /// Nombres de las políticas CORS y de limitación de peticiones.
        /// </summary>
        internal static class Policies
        {
            internal const string Cors = @"g1-identity-cors";
            internal const string Authorization = @"g1-authorization";
            internal const string Token = @"g1-token";
            internal const string MockLogin = @"g1-mock-login";
            internal const string Registration = @"g1-registration";
            internal const string Summary = @"g1-summary";
        }

        /// <summary>
        /// Límites por ventana de un minuto para el alcance académico del mock.
        /// </summary>
        internal static class RateLimits
        {
            internal const int WindowSeconds = 60;
            internal const int AuthorizationPermitLimit = 5;
            internal const int TokenPermitLimit = 10;
            internal const int MockLoginPermitLimit = 5;
            internal const int RegistrationPermitLimit = 5;
            internal const int SummaryPermitLimit = 10;
        }

        /// <summary>
        /// Valores de las cabeceras de seguridad.
        /// </summary>
        internal static class SecurityHeaders
        {
            internal const string SwaggerContentSecurityPolicy =
                @"default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; object-src 'none'; base-uri 'self'; frame-ancestors 'none'";
        }

        /// <summary>
        /// Tipos de contenido relevantes para el contrato.
        /// </summary>
        internal static class ContentTypes
        {
            internal const string ProblemJson = @"application/problem+json";
        }

        /// <summary>
        /// Rutas que requieren cabeceras específicas.
        /// </summary>
        internal static class Paths
        {
            internal const string SwaggerPrefix = @"/swagger";
        }

        /// <summary>
        /// Cookie de protección frente CSRF.
        /// </summary>
        internal static class Antiforgery
        {
            internal const string CookieName = @"__Host-g1.antiforgery";
        }
    }
}
