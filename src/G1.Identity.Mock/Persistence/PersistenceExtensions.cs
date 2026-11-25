using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace G1.Identity.Mock.Persistence
{
    /// <summary>
    /// Extensiones y utilidades para la persistencia del mock de identidad.
    /// </summary>
    public static class PersistenceExtensions
    {
        /// <summary>
        /// Registra el contexto de base de datos y la configuración de OpenIddict relacionada con la persistencia.
        /// </summary>
        /// <param name="services">La colección de servicios.</param>
        /// <param name="databaseName">Nombre de la base de datos InMemory para aislamiento.</param>
        /// <returns>La colección de servicios configurada.</returns>
        public static IServiceCollection AddIdentityPersistence(this IServiceCollection services, string databaseName = "G1IdentityMock")
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddDbContext<G1IdentityDbContext>(options =>
            {
                options.UseInMemoryDatabase(databaseName);
                options.UseOpenIddict();
            });

            // Configuración de OpenIddict Core para usar este DbContext y deshabilitar operaciones masivas
            // DisableBulkOperations() es obligatorio porque InMemory no soporta ExecuteUpdate/ExecuteDelete (ADR-0002).
            services.AddOpenIddict()
                .AddCore(options =>
                {
                    options.UseEntityFrameworkCore()
                           .UseDbContext<G1IdentityDbContext>()
                           .DisableBulkOperations();
                });

            return services;
        }

        /// <summary>
        /// Inicializa el esquema de la base de datos en memoria y registra advertencias de volatilidad.
        /// </summary>
        /// <param name="serviceProvider">El proveedor de servicios de la aplicación.</param>
        /// <exception cref="ArgumentNullException">Se produce cuando <paramref name="serviceProvider"/> es nulo.</exception>
        public static void InitializePersistence(IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);

            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<G1IdentityDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<G1IdentityDbContext>>();

            // Asegurar la creación del esquema InMemory (no soporta migraciones)
            context.Database.EnsureCreated();

            logger.LogWarning("PERSISTENCIA VOLÁTIL ACTIVADA: Se está utilizando EF Core InMemory. Los datos no se persistirán y se perderán al reiniciar la aplicación (ADR-0002).");
        }
    }
}
