using System.Security.Claims;
using System.Text.Encodings.Web;
using G1.Identity.Mock.Endpoints;
using G1.Identity.Mock.Persistence;
using G1.Identity.Mock.Persistence.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace G1.Identity.Mock.Auth
{
    /// <summary>
    /// Opciones para la autenticación de personas del mock.
    /// </summary>
    public sealed class MockPersonaAuthenticationOptions : AuthenticationSchemeOptions
    {
        /// <summary>
        /// Nombre del parámetro de consulta para seleccionar la persona del mock en desarrollo o pruebas.
        /// </summary>
        public string ParameterName { get; set; } = @"mock_persona";

        /// <summary>
        /// Nombre de la cookie de sesión emitida tras autenticar a la persona.
        /// </summary>
        public string CookieName { get; set; } = Constants.Antiforgery.SessionCookieName;
    }

    /// <summary>
    /// Extensiones para registrar la autenticación de personas del mock.
    /// </summary>
    public static class MockPersonaAuthenticationExtensions
    {
        /// <summary>
        /// Nombre del esquema de autenticación utilizado para las personas del mock.
        /// </summary>
        internal const string SchemeName = @"MockPersona";

        /// <summary>
        /// Añade la autenticación de personas del mock al pipeline de autenticación.
        /// </summary>
        /// <param name="builder">Constructor de autenticación.</param>
        /// <returns>El constructor de autenticación configurado.</returns>
        /// <exception cref="ArgumentNullException">
        /// Se produce cuando <paramref name="builder"/> es nulo.
        /// </exception>
        public static AuthenticationBuilder AddMockPersona(this AuthenticationBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Services.AddOptions<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme)
                .Configure(options =>
                {
                    options.Cookie.Name = Constants.Antiforgery.SessionCookieName;
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.ExpireTimeSpan = TimeSpan.FromHours(12);
                    options.SlidingExpiration = true;
                });

            builder.Services.AddScoped<IPersonaSelectionValidator, PersonaSelectionValidator>();

            return builder.AddScheme<MockPersonaAuthenticationOptions, MockPersonaAuthenticationHandler>(
                SchemeName,
                _ => { });
        }
    }

    /// <summary>
    /// Manejador de autenticación que resuelve personas del mock en persistencia
    /// y aplica validaciones de estado, permisos limitados y minimización de menores.
    /// </summary>
    internal sealed class MockPersonaAuthenticationHandler : AuthenticationHandler<MockPersonaAuthenticationOptions>
    {
        private const string NameClaimType = @"name";
        private const string EmailClaimType = @"email";
        private const string EmailVerifiedClaimType = @"email_verified";
        private const string PhoneVerifiedClaimType = @"phone_verified";
        private const string IsMinorClaimType = @"is_minor";
        private const string MockClaimType = @"mock";
        private const string TrueClaimValue = @"true";
        private const string FalseClaimValue = @"false";
        private const string GenericAuthenticationFailureMessage = @"No se pudo autenticar la persona solicitada.";

        private readonly IWebHostEnvironment _environment;
        private readonly G1IdentityDbContext _dbContext;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="MockPersonaAuthenticationHandler"/>.
        /// </summary>
        /// <param name="options">Opciones del esquema de autenticación.</param>
        /// <param name="logger">Factoría de registros.</param>
        /// <param name="encoder">Codificador de URL.</param>
        /// <param name="environment">Entorno de ejecución del host.</param>
        /// <param name="dbContext">Contexto de persistencia del mock de identidad.</param>
        /// <exception cref="ArgumentNullException">
        /// Se produce cuando <paramref name="environment"/> o <paramref name="dbContext"/> es nulo.
        /// </exception>
        public MockPersonaAuthenticationHandler(
            IOptionsMonitor<MockPersonaAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IWebHostEnvironment environment,
            G1IdentityDbContext dbContext)
            : base(options, logger, encoder)
        {
            ArgumentNullException.ThrowIfNull(environment);
            ArgumentNullException.ThrowIfNull(dbContext);

            _environment = environment;
            _dbContext = dbContext;
        }

        /// <inheritdoc />
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var cookieResult = await Context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (cookieResult.Succeeded)
            {
                return cookieResult;
            }

            if (!_environment.IsDevelopment() && !_environment.IsEnvironment(Constants.Environments.Testing))
            {
                return AuthenticateResult.NoResult();
            }

            if (!Context.Request.Query.TryGetValue(Options.ParameterName, out var personaValues))
            {
                return AuthenticateResult.NoResult();
            }

            var requestedIdentifier = personaValues.ToString().Trim();
            if (String.IsNullOrWhiteSpace(requestedIdentifier))
            {
                return AuthenticateResult.NoResult();
            }

            var cancellationToken = Context.RequestAborted;
            var principal = await ResolvePrincipalAsync(
                _dbContext,
                requestedIdentifier,
                Scheme.Name,
                cancellationToken);

            if (principal is null)
            {
                Logger.LogWarning("Autenticación del mock rechazada con error genérico para el parámetro recibido.");
                var failureResult = AuthenticateResult.Fail(GenericAuthenticationFailureMessage);
                Context.Features.Set<IAuthenticateResultFeature>(new MockAuthenticateResultFeature(failureResult));
                return failureResult;
            }

            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            await Context.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                });

            return AuthenticateResult.Success(ticket);
        }

        /// <summary>
        /// Resuelve una persona activa en persistencia por su clave o <c>UserId</c>
        /// y construye el principal autenticado con sus roles, permisos efectivos y minimización.
        /// </summary>
        /// <param name="dbContext">Contexto de persistencia del mock de identidad.</param>
        /// <param name="personaIdentifier">Clave del fixture o <c>UserId</c> de la persona.</param>
        /// <param name="authenticationType">Tipo de autenticación asignado a la identidad.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>
        /// El principal autenticado si la persona existe y está activa; en caso contrario, <see langword="null"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Se produce cuando <paramref name="dbContext"/> es nulo.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Se produce cuando <paramref name="authenticationType"/> es nulo o está en blanco.
        /// </exception>
        internal static async Task<ClaimsPrincipal?> ResolvePrincipalAsync(
            G1IdentityDbContext dbContext,
            string personaIdentifier,
            string authenticationType,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dbContext);
            ArgumentException.ThrowIfNullOrWhiteSpace(authenticationType);

            if (String.IsNullOrWhiteSpace(personaIdentifier))
            {
                return null;
            }

            var normalizedIdentifier = personaIdentifier.Trim();
            var persona = await dbContext.MockPersonas
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    candidate => candidate.Key == normalizedIdentifier || candidate.UserId == normalizedIdentifier,
                    cancellationToken);

            if (persona is null || !persona.CanAuthorize())
            {
                return null;
            }

            var activeRoles = await GetActiveRolesAsync(dbContext, persona.UserId, cancellationToken);
            var effectivePermissions = await GetEffectivePermissionsAsync(dbContext, persona, activeRoles, cancellationToken);
            var claims = BuildClaims(persona, activeRoles, effectivePermissions);

            var identity = new ClaimsIdentity(claims, authenticationType);
            return new ClaimsPrincipal(identity);
        }

        private static async Task<List<string>> GetActiveRolesAsync(
            G1IdentityDbContext dbContext,
            string userId,
            CancellationToken cancellationToken)
        {
            return await dbContext.RoleAssignments
                .AsNoTracking()
                .Where(assignment => assignment.UserId == userId && assignment.RevokedAt == null)
                .Select(assignment => assignment.RoleCode)
                .Distinct()
                .OrderBy(roleCode => roleCode)
                .ToListAsync(cancellationToken);
        }

        private static async Task<List<string>> GetEffectivePermissionsAsync(
            G1IdentityDbContext dbContext,
            MockPersona persona,
            IReadOnlyCollection<string> activeRoles,
            CancellationToken cancellationToken)
        {
            if (!persona.EmailVerified || activeRoles.Count == 0)
            {
                return new List<string>();
            }

            return await dbContext.RolePermissions
                .AsNoTracking()
                .Where(rolePermission => rolePermission.Granted && activeRoles.Contains(rolePermission.RoleCode))
                .Select(rolePermission => rolePermission.PermissionCode)
                .Distinct()
                .OrderBy(permissionCode => permissionCode)
                .ToListAsync(cancellationToken);
        }

        private static List<Claim> BuildClaims(
            MockPersona persona,
            IReadOnlyCollection<string> roles,
            IReadOnlyCollection<string> permissions)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, persona.UserId),
                new Claim(Constants.Claims.Subject, persona.UserId),
                new Claim(NameClaimType, persona.DisplayName),
                new Claim(EmailVerifiedClaimType, FormatBooleanClaim(persona.EmailVerified)),
                new Claim(PhoneVerifiedClaimType, FormatBooleanClaim(persona.PhoneVerified)),
                new Claim(IsMinorClaimType, FormatBooleanClaim(persona.IsMinor)),
                new Claim(MockClaimType, TrueClaimValue),
            };

            if (!persona.IsMinor && !String.IsNullOrWhiteSpace(persona.Email))
            {
                claims.Add(new Claim(EmailClaimType, persona.Email));
            }

            foreach (var role in roles)
            {
                claims.Add(new Claim(Constants.Claims.Roles, role));
            }

            foreach (var permission in permissions)
            {
                claims.Add(new Claim(Constants.Claims.Permissions, permission));
            }

            return claims;
        }

        private static string FormatBooleanClaim(bool value)
        {
            return value ? TrueClaimValue : FalseClaimValue;
        }

        private sealed class MockAuthenticateResultFeature : IAuthenticateResultFeature
        {
            public MockAuthenticateResultFeature(AuthenticateResult authenticateResult)
            {
                AuthenticateResult = authenticateResult;
            }

            public AuthenticateResult? AuthenticateResult { get; set; }
        }
    }
}
