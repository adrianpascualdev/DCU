using System.Security.Claims;
using G1.Identity.Mock;
using G1.Identity.Mock.Auth;
using G1.Identity.Mock.Endpoints;
using G1.Identity.Mock.Fixtures;
using G1.Identity.Mock.Persistence;
using G1.Identity.Mock.Persistence.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace G1.Identity.ContractTests
{
    /// <summary>
    /// Pruebas unitarias y de contrato para <see cref="IPersonaSelectionValidator"/>.
    /// </summary>
    public sealed class PersonaSelectionValidatorTests
    {
        /// <summary>
        /// Proporciona identificadores de persona que deben rechazarse devolviendo <see langword="null"/>.
        /// </summary>
        public static TheoryData<string> InvalidOrBlockedPersonaIdentifiers => new TheoryData<string>
        {
            String.Empty,
            @"   ",
            @"persona.inexistente",
            @"user.inactive",
            @"usr_inactive",
            @"user.locked",
            @"usr_locked",
        };

        /// <summary>
        /// Verifica que una persona activa resuelta por clave o por <c>UserId</c> devuelve
        /// un principal autenticado con su <c>UserId</c> real, roles, permisos efectivos y claims de estado.
        /// </summary>
        /// <param name="personaIdentifier">Clave de fixture o <c>UserId</c> de la persona activa.</param>
        [Theory]
        [InlineData(@"admin.club")]
        [InlineData(@"usr_admin_club")]
        public async Task ValidateAsyncWithActivePersonaKeyOrUserIdReturnsAuthenticatedPrincipalWithExpectedClaims(
            string personaIdentifier)
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var dbContext = await CreateSeededDbContextAsync(cancellationToken);
            IPersonaSelectionValidator validator = new PersonaSelectionValidator(dbContext);

            var principal = await validator.ValidateAsync(personaIdentifier, cancellationToken);

            Assert.NotNull(principal);
            Assert.NotNull(principal.Identity);
            Assert.True(principal.Identity.IsAuthenticated);
            Assert.Equal(@"usr_admin_club", principal.FindFirstValue(Constants.Claims.Subject));
            Assert.Equal(@"usr_admin_club", principal.FindFirstValue(ClaimTypes.NameIdentifier));
            Assert.Equal(@"Admin G1", principal.FindFirstValue(@"name"));
            Assert.Equal(@"admin@mock.g1.com", principal.FindFirstValue(@"email"));
            Assert.Equal(@"true", principal.FindFirstValue(@"email_verified"));
            Assert.Equal(@"true", principal.FindFirstValue(@"phone_verified"));
            Assert.Equal(@"false", principal.FindFirstValue(@"is_minor"));

            var roles = principal.FindAll(Constants.Claims.Roles)
                .Select(claim => claim.Value)
                .ToArray();
            Assert.Equal(new[] { Constants.Roles.ClubAdmin }, roles);

            var permissions = principal.FindAll(Constants.Claims.Permissions)
                .Select(claim => claim.Value)
                .ToArray();
            Assert.Equal(
                new[]
                {
                    Constants.Permissions.IdentityAdmin,
                    Constants.Permissions.UserLookup,
                    Constants.Permissions.UserSummaryRead,
                },
                permissions);
        }

        /// <summary>
        /// Verifica que una persona inexistente, vacía/blanca, inactiva o bloqueada devuelve <see langword="null"/>.
        /// </summary>
        /// <param name="personaIdentifier">Clave o identificador rechazado.</param>
        [Theory]
        [MemberData(nameof(InvalidOrBlockedPersonaIdentifiers))]
        public async Task ValidateAsyncWithInvalidInactiveOrLockedPersonaReturnsNull(string personaIdentifier)
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var dbContext = await CreateSeededDbContextAsync(cancellationToken);
            IPersonaSelectionValidator validator = new PersonaSelectionValidator(dbContext);

            var principal = await validator.ValidateAsync(personaIdentifier, cancellationToken);

            Assert.Null(principal);
        }

        /// <summary>
        /// Verifica que una persona con correo sin verificar devuelve un principal autenticado con
        /// <c>email_verified=false</c> y sin permisos restringidos de G1.
        /// </summary>
        [Fact]
        public async Task ValidateAsyncWithUnverifiedEmailPersonaReturnsUnverifiedClaimAndRestrictedG1Permissions()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var dbContext = await CreateSeededDbContextAsync(cancellationToken);
            IPersonaSelectionValidator validator = new PersonaSelectionValidator(dbContext);

            var principal = await validator.ValidateAsync(@"user.unverified", cancellationToken);

            Assert.NotNull(principal);
            Assert.NotNull(principal.Identity);
            Assert.True(principal.Identity.IsAuthenticated);
            Assert.Equal(@"usr_unverified_email", principal.FindFirstValue(Constants.Claims.Subject));
            Assert.Equal(@"usr_unverified_email", principal.FindFirstValue(ClaimTypes.NameIdentifier));
            Assert.Equal(@"false", principal.FindFirstValue(@"email_verified"));
            Assert.Equal(@"true", principal.FindFirstValue(@"phone_verified"));
            Assert.Equal(@"false", principal.FindFirstValue(@"is_minor"));

            var permissions = principal.FindAll(Constants.Claims.Permissions)
                .Select(claim => claim.Value)
                .ToArray();
            Assert.Empty(permissions);
            Assert.DoesNotContain(Constants.Permissions.IdentityAdmin, permissions);
            Assert.DoesNotContain(Constants.Permissions.UserSummaryRead, permissions);
            Assert.DoesNotContain(Constants.Permissions.UserLookup, permissions);
        }

        /// <summary>
        /// Verifica que incluso si una persona con correo sin verificar tiene un rol privilegiado,
        /// no recibe permisos sensibles ni administrativos de G1.
        /// </summary>
        [Fact]
        public async Task ValidateAsyncWithUnverifiedEmailPersonaHavingPrivilegedRoleRestrictsG1Permissions()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var dbContext = await CreateSeededDbContextAsync(cancellationToken);

            dbContext.RoleAssignments.Add(new RoleAssignment
            {
                AssignmentId = @"asg_usr_unverified_email_club_admin",
                UserId = @"usr_unverified_email",
                RoleCode = Constants.Roles.ClubAdmin,
                GrantedBy = @"system",
                GrantedAt = DateTimeOffset.UtcNow,
                Reason = @"Prueba de restricción de permisos sin correo verificado",
            });
            await dbContext.SaveChangesAsync(cancellationToken);

            IPersonaSelectionValidator validator = new PersonaSelectionValidator(dbContext);

            var principal = await validator.ValidateAsync(@"user.unverified", cancellationToken);

            Assert.NotNull(principal);
            Assert.Equal(@"false", principal.FindFirstValue(@"email_verified"));

            var permissions = principal.FindAll(Constants.Claims.Permissions)
                .Select(claim => claim.Value)
                .ToArray();
            Assert.Empty(permissions);
            Assert.DoesNotContain(Constants.Permissions.IdentityAdmin, permissions);
            Assert.DoesNotContain(Constants.Permissions.UserSummaryRead, permissions);
            Assert.DoesNotContain(Constants.Permissions.UserLookup, permissions);
        }

        /// <summary>
        /// Verifica que una persona menor de edad devuelve <c>is_minor=true</c> y aplica minimización D-017
        /// sin exponer correo, teléfono ni datos de tutor en los claims emitidos.
        /// </summary>
        [Fact]
        public async Task ValidateAsyncWithMinorPersonaAppliesD017MinimizationWithoutContactOrTutorClaims()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var dbContext = await CreateSeededDbContextAsync(cancellationToken);
            IPersonaSelectionValidator validator = new PersonaSelectionValidator(dbContext);

            var principal = await validator.ValidateAsync(@"player.minor", cancellationToken);

            Assert.NotNull(principal);
            Assert.NotNull(principal.Identity);
            Assert.True(principal.Identity.IsAuthenticated);
            Assert.Equal(@"usr_player_minor", principal.FindFirstValue(Constants.Claims.Subject));
            Assert.Equal(@"usr_player_minor", principal.FindFirstValue(ClaimTypes.NameIdentifier));
            Assert.Equal(@"true", principal.FindFirstValue(@"is_minor"));
            Assert.Equal(@"true", principal.FindFirstValue(@"email_verified"));
            Assert.Equal(@"true", principal.FindFirstValue(@"phone_verified"));

            var roles = principal.FindAll(Constants.Claims.Roles)
                .Select(claim => claim.Value)
                .ToArray();
            Assert.Equal(new[] { Constants.Roles.Player }, roles);
            Assert.Empty(principal.FindAll(Constants.Claims.Permissions));

            var forbiddenClaimTypes = new[]
            {
                @"email",
                ClaimTypes.Email,
                @"phone",
                @"phone_number",
                ClaimTypes.MobilePhone,
                ClaimTypes.HomePhone,
                ClaimTypes.OtherPhone,
                @"tutor",
                @"tutor_id",
                @"guardian",
                @"guardian_id",
            };

            foreach (var claim in principal.Claims)
            {
                Assert.DoesNotContain(claim.Type, forbiddenClaimTypes, StringComparer.OrdinalIgnoreCase);
                Assert.DoesNotContain(@"minor@mock.g1.com", claim.Value, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(@"tutor", claim.Type, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(@"guardian", claim.Type, StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Verifica que <see cref="MockPersonaAuthenticationExtensions.AddMockPersona"/> registra
        /// <see cref="IPersonaSelectionValidator"/> como Scoped sin duplicar el esquema de cookies.
        /// </summary>
        [Fact]
        public async Task AddMockPersonaRegistersPersonaSelectionValidatorAsScopedWithoutDuplicatingCookiesScheme()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddIdentityPersistence($@"PersonaSelectionValidatorDiTests_{Guid.NewGuid():N}");
            services.AddAuthentication()
                .AddCookie()
                .AddMockPersona();

            var descriptor = Assert.Single(
                services,
                service => service.ServiceType == typeof(IPersonaSelectionValidator));
            Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
            Assert.Equal(typeof(PersonaSelectionValidator), descriptor.ImplementationType);

            await using var provider = services.BuildServiceProvider();
            var schemeProvider = provider.GetRequiredService<IAuthenticationSchemeProvider>();
            var cookieScheme = await schemeProvider.GetSchemeAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            Assert.NotNull(cookieScheme);

            using var firstScope = provider.CreateScope();
            var dbContext = firstScope.ServiceProvider.GetRequiredService<G1IdentityDbContext>();
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
            await SeedData.EnsureSeedDataAsync(dbContext, cancellationToken);

            var firstInstance = firstScope.ServiceProvider.GetRequiredService<IPersonaSelectionValidator>();
            var sameScopeInstance = firstScope.ServiceProvider.GetRequiredService<IPersonaSelectionValidator>();
            Assert.Same(firstInstance, sameScopeInstance);

            using var secondScope = provider.CreateScope();
            var secondScopeInstance = secondScope.ServiceProvider.GetRequiredService<IPersonaSelectionValidator>();
            Assert.NotSame(firstInstance, secondScopeInstance);
        }

        private static async Task<G1IdentityDbContext> CreateSeededDbContextAsync(CancellationToken cancellationToken)
        {
            var options = new DbContextOptionsBuilder<G1IdentityDbContext>()
                .UseInMemoryDatabase($@"PersonaSelectionValidatorTests_{Guid.NewGuid():N}")
                .Options;

            var dbContext = new G1IdentityDbContext(options);
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
            await SeedData.EnsureSeedDataAsync(dbContext, cancellationToken);
            return dbContext;
        }
    }
}
