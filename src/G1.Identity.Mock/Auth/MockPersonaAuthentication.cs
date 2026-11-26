using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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
        /// Nombre del parámetro de consulta para forzar la identidad.
        /// </summary>
        public string ParameterName { get; set; } = @"mock_persona";

        /// <summary>
        /// Nombre de la cookie de sesión.
        /// </summary>
        public string CookieName { get; set; } = @"__Host-g1.session";
    }

    /// <summary>
    /// Extensiones para registrar la autenticación de personas del mock.
    /// </summary>
    public static class MockPersonaAuthenticationExtensions
    {
        /// <summary>
        /// Añade la autenticación de personas del mock al pipeline.
        /// </summary>
        /// <param name="builder">Constructor de autenticación.</param>
        /// <returns>El constructor configurado.</returns>
        /// <exception cref="ArgumentNullException">
        /// Se produce cuando <paramref name="builder"/> es nulo.
        /// </exception>
        public static AuthenticationBuilder AddMockPersona(this AuthenticationBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Services.AddOptions<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme)
                .Configure(options =>
                {
                    options.Cookie.Name = @"__Host-g1.session";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.ExpireTimeSpan = TimeSpan.FromHours(12);
                    options.SlidingExpiration = true;
                });

            return builder.AddScheme<MockPersonaAuthenticationOptions, MockPersonaAuthenticationHandler>(
                @"MockPersona",
                _ => { });
        }
    }

    /// <summary>
    /// Manejador de autenticación que permite simular un usuario mediante un parámetro de consulta.
    /// </summary>
    internal sealed class MockPersonaAuthenticationHandler : AuthenticationHandler<MockPersonaAuthenticationOptions>
    {
        private readonly IWebHostEnvironment _environment;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="MockPersonaAuthenticationHandler"/>.
        /// </summary>
        /// <param name="options">Opciones del esquema.</param>
        /// <param name="logger">Factoría de logs.</param>
        /// <param name="encoder">Codificador de URL.</param>
        /// <param name="environment">Entorno de ejecución.</param>
        public MockPersonaAuthenticationHandler(
            IOptionsMonitor<MockPersonaAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IWebHostEnvironment environment)
            : base(options, logger, encoder)
        {
            _environment = environment;
        }

        /// <inheritdoc />
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // 1. Intentar autenticar por cookie primero (esquema por defecto)
            var cookieResult = await Context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (cookieResult.Succeeded)
            {
                return cookieResult;
            }

            // 2. Si no hay cookie, buscar el parámetro mock_persona si no estamos en producción
            if (!_environment.IsDevelopment() && !_environment.IsEnvironment(@"Testing"))
            {
                return AuthenticateResult.NoResult();
            }

            if (!Context.Request.Query.TryGetValue(Options.ParameterName, out var personaId) || string.IsNullOrWhiteSpace(personaId))
            {
                return AuthenticateResult.NoResult();
            }

            // 3. Crear identidad simulada
            // Nota: En T031 se añadirán más claims. Por ahora lo mínimo para identificar al usuario.
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, personaId.ToString()),
                new(@"sub", personaId.ToString()),
                new(@"mock", @"true")
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            // 4. Emitir cookie de sesión automáticamente
            await Context.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = true });

            return AuthenticateResult.Success(ticket);
        }
    }
}
