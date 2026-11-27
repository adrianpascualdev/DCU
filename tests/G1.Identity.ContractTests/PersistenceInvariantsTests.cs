using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using G1.Identity.Mock.Persistence;
using G1.Identity.Mock.Persistence.Entities;
using G1.Identity.Mock.Fixtures;

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

        /// <summary>
        /// Verifica que una sesión Revoked o Expired no permite operaciones.
        /// </summary>
        /// <param name="status">El estado de la sesión.</param>
        [Theory]
        [InlineData(SessionStatus.Revoked)]
        [InlineData(SessionStatus.Expired)]
        public void Session_RevokedOrExpired_ShouldNotBeActive(SessionStatus status)
        {
            // Arrange
            var session = new Session { Status = status };

            // Act
            var isActive = session.IsActive();

            // Assert
            Assert.False(isActive, $"Una sesión en estado {status} no debería estar activa.");
        }

        /// <summary>
        /// Verifica que ExpiresAt nunca supera AbsoluteExpiresAt.
        /// </summary>
        [Fact]
        public void Session_ExpiresAt_ShouldNotExceedAbsoluteExpiresAt()
        {
            // Arrange
            var now = DateTimeOffset.UtcNow;
            var session = new Session
            {
                AbsoluteExpiresAt = now.AddHours(1),
                ExpiresAt = now.AddHours(2)
            };

            // Act
            var isValid = session.HasValidExpirations();

            // Assert
            Assert.False(isValid, "ExpiresAt no debería superar AbsoluteExpiresAt.");
        }

        /// <summary>
        /// Verifica que AuditEvent no expone secretos ni tokens en los metadatos.
        /// </summary>
        [Fact]
        public void AuditEvent_ShouldNotExposeSecrets()
        {
            // Arrange
            var audit = new AuditEvent();
            var metadata = new Dictionary<string, string>
            {
                { "token", "secret_value" },
                { "password", "password123" },
                { "health_data", "sensitive" }
            };

            // Act & Assert
            foreach (var key in metadata.Keys)
            {
                Assert.Throws<InvalidOperationException>(() => audit.AddMetadata(key, metadata[key]));
            }
        }

        /// <summary>
        /// Verifica que Session y SessionToken tienen configuradas las restricciones de claves y prefijos.
        /// </summary>
        [Fact]
        public async Task Persistence_SessionAndToken_ShouldHaveRequiredMeta()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddIdentityPersistence("TestDb_Session_Meta_" + Guid.NewGuid().ToString("N"));
            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<G1IdentityDbContext>();

            // Act & Assert
            var sessionEntity = context.Model.FindEntityType(typeof(Session));
            Assert.NotNull(sessionEntity);
            Assert.Equal(nameof(Session.SessionId), sessionEntity.FindPrimaryKey()?.Properties[0].Name);

            var sessionTokenEntity = context.Model.FindEntityType(typeof(SessionToken));
            Assert.NotNull(sessionTokenEntity);
            
            var auditEntity = context.Model.FindEntityType(typeof(AuditEvent));
            Assert.NotNull(auditEntity);

            var catalogEntity = context.Model.FindEntityType(typeof(CatalogSnapshot));
            Assert.NotNull(catalogEntity);
        }

        /// <summary>
        /// Verifica que el seeding es idempotente y no duplica registros al ejecutarse dos veces.
        /// </summary>
        [Fact]
        public async Task SeedData_ShouldBeIdempotent()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddIdentityPersistence("TestDb_Seed_Idempotency_" + Guid.NewGuid().ToString("N"));
            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<G1IdentityDbContext>();
            
            // Act
            await SeedData.EnsureSeedDataAsync(context, TestContext.Current.CancellationToken);
            var rolesCountFirst = await context.Roles.CountAsync(TestContext.Current.CancellationToken);
            
            await SeedData.EnsureSeedDataAsync(context, TestContext.Current.CancellationToken);
            var rolesCountSecond = await context.Roles.CountAsync(TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(rolesCountFirst, rolesCountSecond);
            Assert.NotEqual(0, rolesCountFirst);
        }

        /// <summary>
        /// Verifica que el catálogo v1 contiene exactamente los 10 roles definidos.
        /// </summary>
        [Fact]
        public async Task SeedData_ShouldHaveExactly10Roles()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddIdentityPersistence("TestDb_Seed_Roles_" + Guid.NewGuid().ToString("N"));
            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<G1IdentityDbContext>();
            
            // Act
            await SeedData.EnsureSeedDataAsync(context, TestContext.Current.CancellationToken);
            var roles = await context.Roles.ToListAsync(TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(10, roles.Count);
            var expectedRoles = new[] 
            { 
                "club_admin", "sports_manager", "coach", "player", "tutor", 
                "member", "sponsor", "supporter", "kit_staff", "communications_staff" 
            };
            foreach (var code in expectedRoles)
            {
                Assert.Contains(roles, r => r.Code == code);
            }
        }

        /// <summary>
        /// Verifica que el catálogo contiene exactamente los 3 permisos de G1.
        /// </summary>
        [Fact]
        public async Task SeedData_ShouldHaveExactly3G1Permissions()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddIdentityPersistence("TestDb_Seed_Permissions_" + Guid.NewGuid().ToString("N"));
            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<G1IdentityDbContext>();
            
            // Act
            await SeedData.EnsureSeedDataAsync(context, TestContext.Current.CancellationToken);
            var permissions = await context.Permissions.ToListAsync(TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(3, permissions.Count);
            var expectedPermissions = new[] 
            { 
                "identity.users.summary.read", "identity.users.lookup", "identity.admin" 
            };
            foreach (var code in expectedPermissions)
            {
                Assert.Contains(permissions, p => p.Code == code);
            }
        }

        /// <summary>
        /// Verifica la matriz de permisos exacta para los roles clave.
        /// </summary>
        [Fact]
        public async Task SeedData_ShouldHaveCorrectRolePermissionMatrix()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddIdentityPersistence("TestDb_Seed_Matrix_" + Guid.NewGuid().ToString("N"));
            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<G1IdentityDbContext>();
            
            // Act
            await SeedData.EnsureSeedDataAsync(context, TestContext.Current.CancellationToken);
            
            // Helper local para verificar
            async Task AssertPermission(string role, string permission, bool granted)
            {
                var hasPermission = await context.RolePermissions.AnyAsync(
                    rp => rp.RoleCode == role && rp.PermissionCode == permission && rp.Granted == granted,
                    TestContext.Current.CancellationToken);
                Assert.True(hasPermission, $"El rol {role} debería tener el permiso {permission} como {granted}.");
            }

            // Assert
            // club_admin: todos
            await AssertPermission("club_admin", "identity.users.summary.read", true);
            await AssertPermission("club_admin", "identity.users.lookup", true);
            await AssertPermission("club_admin", "identity.admin", true);

            // sports_manager: read, lookup
            await AssertPermission("sports_manager", "identity.users.summary.read", true);
            await AssertPermission("sports_manager", "identity.users.lookup", true);
            
            // communications_staff: lookup
            await AssertPermission("communications_staff", "identity.users.lookup", true);

            // Otros: deny-by-default (no deberían tener registros Granted=true si no están en la lista)
            var otherGranted = await context.RolePermissions.AnyAsync(
                rp => rp.Granted && 
                      rp.RoleCode != "club_admin" && 
                      rp.RoleCode != "sports_manager" && 
                      rp.RoleCode != "communications_staff",
                TestContext.Current.CancellationToken);
            Assert.False(otherGranted, "No debería haber permisos concedidos para otros roles.");
        }

        /// <summary>
        /// Verifica que se siembran exactamente las 15 personas ficticias.
        /// </summary>
        [Fact]
        public async Task SeedData_ShouldHaveExactly15Personas()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddIdentityPersistence("TestDb_Seed_Personas_" + Guid.NewGuid().ToString("N"));
            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<G1IdentityDbContext>();
            
            // Act
            await SeedData.EnsureSeedDataAsync(context, TestContext.Current.CancellationToken);
            var personas = await context.MockPersonas.ToListAsync(TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(15, personas.Count);
            var expectedKeys = new[] 
            { 
                "admin.club", "sports.manager", "coach.ana", "player.minor", "member.active",
                "member.phone-unverified", "tutor.familiar", "sponsor.acme", "supporter.fan",
                "kit.staff", "comms.staff", "user.no-role", "user.unverified", "user.inactive", "user.locked"
            };
            foreach (var key in expectedKeys)
            {
                Assert.Contains(personas, p => p.Key == key);
            }
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
