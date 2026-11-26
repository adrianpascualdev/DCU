using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using G1.Identity.Mock.Persistence;
using G1.Identity.Mock.Persistence.Entities;

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

        /// <summary>
        /// Verifica que MockPersona tiene configuradas las restricciones de unicidad necesarias.
        /// </summary>
        [Fact]
        public void MockPersona_ShouldHaveUniqueConstraints()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddIdentityPersistence("TestDb_MockPersona_Meta_" + Guid.NewGuid().ToString("N"));
            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<G1IdentityDbContext>();

            // Act
            var entity = context.Model.FindEntityType(typeof(MockPersona));

            // Assert
            Assert.NotNull(entity);
            
            var keyIndex = entity.GetIndexes().FirstOrDefault(i => i.Properties.Any(p => p.Name == "Key"));
            Assert.True(keyIndex?.IsUnique, "Key debe ser único en MockPersona.");

            var userIdIndex = entity.GetIndexes().FirstOrDefault(i => i.Properties.Any(p => p.Name == "UserId"));
            Assert.True(userIdIndex?.IsUnique, "UserId debe ser único en MockPersona.");
            
            var emailIndex = entity.GetIndexes().FirstOrDefault(i => i.Properties.Any(p => p.Name == "Email"));
            Assert.True(emailIndex?.IsUnique, "Email debe ser único en MockPersona.");
        }

        /// <summary>
        /// Verifica que el UserId de MockPersona es inmutable (solo lectura o init).
        /// </summary>
        [Fact]
        public void MockPersona_UserId_ShouldBeImmutable()
        {
            // Arrange & Act
            var property = typeof(MockPersona).GetProperty(nameof(MockPersona.UserId));

            // Assert
            Assert.NotNull(property);
            var isInitOnly = property.SetMethod?.ReturnParameter.GetRequiredCustomModifiers().Any(a => a.Name == "IsExternalInit") ?? false;
            var isPrivate = property.SetMethod?.IsPrivate ?? false;
            var hasNoSet = property.SetMethod == null;

            Assert.True(hasNoSet || isPrivate || isInitOnly, "UserId debería ser inmutable (sin setter, privado o init).");
        }

        /// <summary>
        /// Verifica que una persona Inactive o Locked no puede completar la autorización.
        /// </summary>
        /// <param name="status">El estado de la persona.</param>
        [Theory]
        [InlineData(PersonaStatus.Inactive)]
        [InlineData(PersonaStatus.Locked)]
        public void MockPersona_InactiveOrLocked_CannotAuthorize(PersonaStatus status)
        {
            // Arrange
            var persona = new MockPersona { Status = status };

            // Act
            var canAuthorize = persona.CanAuthorize();

            // Assert
            Assert.False(canAuthorize, $"Una persona en estado {status} no debería poder autorizar.");
        }

        /// <summary>
        /// Verifica que Email y Phone no se exponen en el resumen de terceros.
        /// </summary>
        [Fact]
        public void MockPersona_Summary_ShouldExcludeEmailAndPhone()
        {
            // Arrange
            var persona = new MockPersona 
            { 
                DisplayName = "Test User",
                Email = "secret@example.com", 
                Phone = "123456789" 
            };

            // Act
            var summary = persona.ToThirdPartySummary();

            // Assert
            Assert.DoesNotContain("secret@example.com", summary);
            Assert.DoesNotContain("123456789", summary);
            Assert.Contains("Test User", summary);
        }

        /// <summary>
        /// Verifica que una RoleAssignment con RevokedAt no contribuye a roles ni permisos.
        /// </summary>
        [Fact]
        public void RoleAssignment_RevokedAt_ShouldNotBeActive()
        {
            // Arrange
            var assignment = new RoleAssignment { RevokedAt = DateTimeOffset.UtcNow };

            // Act
            var isActive = assignment.IsActive();

            // Assert
            Assert.False(isActive, "Una asignación con RevokedAt no debería estar activa.");
        }

        /// <summary>
        /// Verifica que ScopeRef se guarda y recupera sin validación de negocio (opaco).
        /// </summary>
        [Fact]
        public void RoleAssignment_ScopeRef_ShouldBeStoredOpaquely()
        {
            // Arrange
            var scopeRef = "any:opaque:business:value:123";
            var assignment = new RoleAssignment { ScopeRef = scopeRef };

            // Act & Assert
            Assert.Equal(scopeRef, assignment.ScopeRef);
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
