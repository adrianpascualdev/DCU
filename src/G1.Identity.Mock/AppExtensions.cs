using G1.Identity.Mock.Middleware;
using G1.Identity.Mock.Persistence;
using G1.Identity.Mock.Settings;
using Microsoft.Extensions.Options;

namespace G1.Identity.Mock.Extensions
{
    /// <summary>
    /// Configuración del pipeline de peticiones del mock de identidad.
    /// </summary>
    public static class AppExtensions
    {
        /// <summary>
        /// Registra el manejador de excepciones, la correlación, la seguridad, CORS,
        /// la protección antiforgery y la limitación de peticiones.
        /// </summary>
        /// <param name="app">Instancia de la aplicación web.</param>
        /// <returns>La aplicación configurada para encadenamiento fluido.</returns>
        /// <exception cref="ArgumentNullException">
        /// Se produce cuando <paramref name="app"/> es nulo.
        /// </exception>
        public static WebApplication UseMiddlewares(this WebApplication app)
        {
            ArgumentNullException.ThrowIfNull(app);

            PersistenceExtensions.InitializePersistence(app.Services);
            app.UseExceptionHandler();
            app.UseMiddleware<CorrelationIdMiddleware>();
            app.UseMiddleware<SecurityHeadersMiddleware>();
            app.UseCors(Constants.Policies.Cors);
            app.UseAntiforgery();
            app.UseRateLimiter();

            if (!app.Services.GetRequiredService<IOptions<CorsOptions>>().Value.AllowedOrigins.Any())
            {
                app.Logger.LogWarning(
                    "No hay orígenes CORS configurados; las peticiones de origen cruzado se rechazarán.");
            }

            return app;
        }

        /// <summary>
        /// Registra los endpoints declarados mediante <see cref="IEndpointDefinition"/>.
        /// </summary>
        /// <param name="app">Instancia de la aplicación web.</param>
        /// <returns>La aplicación configurada para encadenamiento fluido.</returns>
        /// <exception cref="ArgumentNullException">
        /// Se produce cuando <paramref name="app"/> es nulo.
        /// </exception>
        public static WebApplication UseRegisterEndpointDefinitions(this WebApplication app)
        {
            ArgumentNullException.ThrowIfNull(app);

            var definitions = typeof(AppExtensions).Assembly
                .GetTypes()
                .Where(type => type.IsAssignableTo(typeof(IEndpointDefinition))
                    && type is { IsAbstract: false, IsInterface: false })
                .Select(type => Activator.CreateInstance(type, nonPublic: true))
                .OfType<IEndpointDefinition>();

            foreach (var definition in definitions)
            {
                definition.RegisterEndpoints(app);
            }

            return app;
        }
    }

    /// <summary>
    /// Contrato interno para registrar grupos de endpoints Minimal API.
    /// </summary>
    internal interface IEndpointDefinition
    {
        /// <summary>
        /// Registra los endpoints de una característica en la aplicación.
        /// </summary>
        /// <param name="app">Instancia de la aplicación web.</param>
        void RegisterEndpoints(WebApplication app);
    }
}
