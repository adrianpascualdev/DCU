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
            internal const string CacheControl = @"Cache-Control";
            internal const string ETag = @"ETag";
            internal const string IfNoneMatch = @"If-None-Match";
            internal const string NoSniff = @"nosniff";
            internal const string Deny = @"DENY";
            internal const string NoReferrer = @"no-referrer";
            internal const string NoStore = @"no-store";
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
        /// Permisos atómicos cuya evaluación pertenece a G1.
        /// </summary>
        internal static class Permissions
        {
            internal const string UserSummaryRead = @"identity.users.summary.read";
            internal const string UserLookup = @"identity.users.lookup";
            internal const string IdentityAdmin = @"identity.admin";
        }

        /// <summary>
        /// Tipos de claim publicados por el proveedor de identidad.
        /// </summary>
        internal static class Claims
        {
            internal const string Permissions = @"permissions";
            internal const string Roles = @"roles";
            internal const string Subject = @"sub";
            internal const string SessionId = @"sid";
        }

        /// <summary>
        /// Códigos estables del catálogo común de roles.
        /// </summary>
        internal static class Roles
        {
            internal const string ClubAdmin = @"club_admin";
            internal const string SportsManager = @"sports_manager";
            internal const string Coach = @"coach";
            internal const string Player = @"player";
            internal const string Tutor = @"tutor";
            internal const string Member = @"member";
            internal const string Sponsor = @"sponsor";
            internal const string Supporter = @"supporter";
            internal const string KitStaff = @"kit_staff";
            internal const string CommunicationsStaff = @"communications_staff";
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
                @"default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self' data:; object-src 'none'; base-uri 'self'; frame-ancestors 'none'";

            internal const string SelectorContentSecurityPolicy =
                @"default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self' data:; font-src 'self' data:; connect-src 'self'; object-src 'none'; base-uri 'none'; form-action 'self'; frame-ancestors 'none'";
        }

        /// <summary>
        /// Tipos de contenido relevantes para el contrato.
        /// </summary>
        internal static class ContentTypes
        {
            internal const string ProblemJson = @"application/problem+json";
            internal const string Json = @"application/json";
        }

        /// <summary>
        /// Valores que identifican el documento OpenAPI publicado.
        /// </summary>
        internal static class OpenApi
        {
            internal const string DocumentName = @"v1";
            internal const string SpecificationField = @"openapi";
            internal const string SpecificationVersion = @"3.1.0";
            internal const string ApiVersion = @"1.0.0";
            internal const string CacheControl = @"no-cache";
            internal const string SwaggerRoutePrefix = @"swagger";
            internal const string SwaggerTitle = @"G1 Identity API 1.0.0";
        }

        /// <summary>
        /// Entornos donde se publica la documentación interactiva.
        /// </summary>
        internal static class Environments
        {
            internal const string Testing = @"Testing";
        }

        /// <summary>
        /// Rutas que requieren cabeceras específicas.
        /// </summary>
        internal static class Paths
        {
            internal const string SwaggerPrefix = @"/swagger";
            internal const string OpenApiDocument = @"/openapi/v1.json";
            internal const string RoleCatalog = @"/api/v1/identity/catalog/roles";
            internal const string PermissionCatalog = @"/api/v1/identity/catalog/permissions";
            internal const string Sessions = @"/api/v1/identity/sessions";
            internal const string SessionById = @"/api/v1/identity/sessions/{sessionId}";
            internal const string RevokeAllSessions = @"/api/v1/identity/sessions/revoke-all";
            internal const string Logout = @"/api/v1/identity/logout";
            internal const string Authorize = @"/connect/authorize";
            internal const string Selector = @"/identity/select-persona";
            internal const string SelectorAssets = @"/identity-ui";
        }

        /// <summary>
        /// Valores internos del selector alojado por el host (fuera de la API versionada).
        /// </summary>
        internal static class Selector
        {
            internal const string RequestUriPrefix = @"urn:ietf:params:oauth:request_uri:";
            internal const string RequestUriAttribute = @"data-request-uri";
            internal const string ClientIdAttribute = @"data-client-id";
            internal const string AntiforgeryAttribute = @"data-antiforgery";
            internal const string FlowCookieName = @"__Host-g1.flow";
            internal const int MaximumIdentifierLength = 256;

            /// <summary>
            /// Compone el nombre de la cookie de vinculación de un flujo concreto.
            /// </summary>
            /// <param name="identifier">Identificador opaco de la solicitud.</param>
            /// <returns>Un nombre compatible con el prefijo <c>__Host-</c>.</returns>
            internal static string BuildFlowCookieName(string identifier) =>
                string.Concat(FlowCookieName, @".", identifier);
            internal static readonly TimeSpan BindingLifetime = TimeSpan.FromMinutes(5);
            internal static readonly TimeSpan TokenLifetime = TimeSpan.FromMinutes(5);
        }

        /// <summary>
        /// Valores estables del ciclo de vida de las sesiones.
        /// </summary>
        internal static class Sessions
        {
            internal const string LogoutReason = @"logout";
            internal const string UserRevokedReason = @"user_revoked";
            internal const int DefaultPage = 1;
            internal const int DefaultPageSize = 20;
            internal const int MaximumPageSize = 100;
        }

        /// <summary>
        /// Nombres estables de eventos de auditoría.
        /// </summary>
        internal static class AuditEvents
        {
            internal const string Logout = @"logout";
            internal const string SessionRevoked = @"session_revoked";
            internal const string TokenRevoked = @"token_revoked";
        }

        /// <summary>
        /// Valores estables de los catálogos públicos de identidad.
        /// </summary>
        internal static class Catalog
        {
            internal const string Version = @"1.0.0-provisional";
            internal const string OwnerModule = @"G1";
            internal const string RolesEntityTag = @"""roles-1.0.0-provisional""";
            internal const string PermissionsEntityTag = @"""permissions-1.0.0-provisional""";
        }

        /// <summary>
        /// Cookie de protección frente CSRF.
        /// </summary>
        internal static class Antiforgery
        {
            internal const string CookieName = @"__Host-g1.antiforgery";
            internal const string SessionCookieName = @"__Host-g1.session";
        }
    }
}
