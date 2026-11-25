using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using G1.Identity.Mock.Persistence;

namespace G1.Identity.ContractTests
{
    /// <summary>
    /// Pruebas de invariantes para la persistencia del mock de identidad.
    /// </summary>
    public sealed class PersistenceInvariantsTests
    {
        /// <summary>
        /// Verifica que el DbContext se inicializa correctamente con InMemory y el modelo de OpenIddict.
        /// </summary>
        [Fact]
        public async Task DbContext_ShouldInitializeInMemoryAndOpenIddict()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddIdentityPersistence("TestDb_" + Guid.NewGuid().ToString("N"));

            using var provider = services.BuildServiceProvider();
            
            // Act
            PersistenceExtensions.InitializePersistence(provider);
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<G1IdentityDbContext>();
            var canConnect = await context.Database.CanConnectAsync(TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(context);
            Assert.True(canConnect, "Debería ser posible conectar a la base de datos InMemory.");
            
            // Verificar que el modelo incluye las entidades de OpenIddict
            var entityTypes = context.Model.GetEntityTypes().Select(t => t.Name).ToList();
            Assert.Contains(entityTypes, name => name.Contains("OpenIddictEntityFrameworkCoreApplication"));
            Assert.Contains(entityTypes, name => name.Contains("OpenIddictEntityFrameworkCoreToken"));
        }

        /// <summary>
        /// Verifica que InitializePersistence registra la advertencia de volatilidad en los logs.
        /// </summary>
        [Fact]
        public void InitializePersistence_ShouldLogVolatilityWarning()
        {
            // Arrange
            var logEntries = new List<(LogLevel Level, string Message)>();
            var services = new ServiceCollection();
            services.AddIdentityPersistence("TestDb_Log_" + Guid.NewGuid().ToString("N"));
            services.AddSingleton<ILogger<G1IdentityDbContext>>(new TestLogger<G1IdentityDbContext>(logEntries));

            using var provider = services.BuildServiceProvider();

            // Act
            PersistenceExtensions.InitializePersistence(provider);

            // Assert
            Assert.Contains(logEntries, log => 
                log.Level == LogLevel.Warning && 
                log.Message.Contains("PERSISTENCIA VOLÁTIL ACTIVADA") &&
                log.Message.Contains("ADR-0002"));
        }

        private sealed class TestLogger<T> : ILogger<T>
        {
            private readonly List<(LogLevel, string)> _logEntries;

            public TestLogger(List<(LogLevel, string)> logEntries) => _logEntries = logEntries;

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                _logEntries.Add((logLevel, formatter(state, exception)));
            }
        }
    }
}
