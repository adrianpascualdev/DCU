using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using G1.Identity.Mock;
using G1.Identity.Mock.Auth;
using G1.Identity.Mock.Extensions;
using G1.Identity.Mock.Fixtures;
using G1.Identity.Mock.Persistence;
using G1.Identity.Mock.Persistence.Entities;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace G1.Identity.ContractTests
{
    /// <summary>
    /// Pruebas de contrato para la autenticación de personas del mock (T016 y T050).
    /// </summary>
    public sealed class MockPersonaAuthenticationTests
    {
        private const string SessionCookieName = @"__Host-g1.session";
        private const string MockPersonaParam = @"mock_persona";

        /// <summary>
        /// Verifica que <c>mock_persona</c> acepta la clave del fixture en Development,
        /// consulta la persistencia y emite el <c>UserId</c> real y la cookie de sesión.
        /// </summary>
        [Fact]
        public async Task MockPersonaParameterAcceptsKeyAndEmitsRealUserIdInDevelopment()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var host = await CreateHostAsync(
                environment: Environments.Development,
                mapEndpoints: MapClaimsInspectionEndpoint,
                cancellationToken: cancellationToken);

            using var anonymousResponse = await host.Client.GetAsync(@"/api/test/claims", cancellationToken);
            Assert.Equal(HttpStatusCode.Unauthorized, anonymousResponse.StatusCode);

            using var response = await host.Client.GetAsync(
                $@"/api/test/claims?{MockPersonaParam}=admin.club",
                cancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var setCookie = Assert.Single(response.Headers.GetValues(@"Set-Cookie"));
            Assert.Contains($@"{SessionCookieName}=", setCookie, StringComparison.Ordinal);
            Assert.Contains(@"httponly", setCookie, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(@"samesite=lax", setCookie, StringComparison.OrdinalIgnoreCase);

            var snapshot = await response.Content.ReadFromJsonAsync<ClaimsSnapshot>(cancellationToken);
            Assert.NotNull(snapshot);
            Assert.Equal(@"usr_admin_club", snapshot.Subject);
            Assert.Equal(@"usr_admin_club", snapshot.NameIdentifier);
            Assert.Equal(@"true", snapshot.EmailVerified);
            Assert.Equal(@"true", snapshot.PhoneVerified);
            Assert.Equal(@"false", snapshot.IsMinor);
            Assert.Contains(@"club_admin", snapshot.Roles);
            Assert.Contains(Constants.Permissions.UserSummaryRead, snapshot.Permissions);
            Assert.Contains(Constants.Permissions.UserLookup, snapshot.Permissions);
            Assert.Contains(Constants.Permissions.IdentityAdmin, snapshot.Permissions);
        }

        /// <summary>
        /// Verifica que <c>mock_persona</c> también acepta el <c>UserId</c> real del fixture
        /// y resuelve exactamente la misma identidad persistida.
        /// </summary>
        [Fact]
        public async Task MockPersonaParameterAcceptsUserIdAndEmitsRealUserId()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var host = await CreateHostAsync(
                environment: Environments.Development,
                mapEndpoints: MapClaimsInspectionEndpoint,
                cancellationToken: cancellationToken);

            using var response = await host.Client.GetAsync(
                $@"/api/test/claims?{MockPersonaParam}=usr_admin_club",
                cancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var snapshot = await response.Content.ReadFromJsonAsync<ClaimsSnapshot>(cancellationToken);
            Assert.NotNull(snapshot);
            Assert.Equal(@"usr_admin_club", snapshot.Subject);
            Assert.Equal(@"usr_admin_club", snapshot.NameIdentifier);
            Assert.Contains(@"club_admin", snapshot.Roles);
        }

        /// <summary>
        /// Verifica que el parámetro <c>mock_persona</c> también se acepta en el entorno Testing.
        /// </summary>
        [Fact]
        public async Task MockPersonaParameterIsAcceptedInTestingEnvironment()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var host = await CreateHostAsync(
                environment: Constants.Environments.Testing,
                mapEndpoints: MapClaimsInspectionEndpoint,
                cancellationToken: cancellationToken);

            using var response = await host.Client.GetAsync(
                $@"/api/test/claims?{MockPersonaParam}=admin.club",
                cancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(response.Headers.Contains(@"Set-Cookie"));
            var snapshot = await response.Content.ReadFromJsonAsync<ClaimsSnapshot>(cancellationToken);
            Assert.NotNull(snapshot);
            Assert.Equal(@"usr_admin_club", snapshot.Subject);
            Assert.Equal(@"usr_admin_club", snapshot.NameIdentifier);
        }

        /// <summary>
        /// Verifica que <c>mock_persona</c> se ignora fuera de Development y Testing y no emite cookie.
        /// </summary>
        /// <param name="environment">Entorno donde no debe aceptarse <c>mock_persona</c>.</param>
        [Theory]
        [InlineData(@"Production")]
        [InlineData(@"Staging")]
        public async Task MockPersonaParameterIsRejectedOutsideDevelopmentAndTesting(string environment)
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var host = await CreateHostAsync(
                environment: environment,
                mapEndpoints: MapClaimsInspectionEndpoint,
                cancellationToken: cancellationToken);

            using var response = await host.Client.GetAsync(
                $@"/api/test/claims?{MockPersonaParam}=admin.club",
                cancellationToken);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.False(response.Headers.Contains(@"Set-Cookie"));
        }

        /// <summary>
        /// Verifica que personas inexistentes, inactivas o bloqueadas se rechazan con un 401 genérico
        /// sin emitir cookie ni revelar el estado de la cuenta.
        /// </summary>
        /// <param name="personaIdentifier">Clave o identificador de la persona rechazada.</param>
        [Theory]
        [InlineData(@"persona.inexistente")]
        [InlineData(@"user.inactive")]
        [InlineData(@"usr_inactive")]
        [InlineData(@"user.locked")]
        [InlineData(@"usr_locked")]
        public async Task NonExistentInactiveOrLockedPersonaIsRejectedWithGenericUnauthorized(string personaIdentifier)
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var host = await CreateHostAsync(
                environment: Environments.Development,
                mapEndpoints: MapClaimsInspectionEndpoint,
                cancellationToken: cancellationToken);

            using var response = await host.Client.GetAsync(
                $@"/api/test/claims?{MockPersonaParam}={Uri.EscapeDataString(personaIdentifier)}",
                cancellationToken);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.False(response.Headers.Contains(@"Set-Cookie"));

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            Assert.DoesNotContain(@"inactive", body, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(@"locked", body, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(@"inactiv", body, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(@"bloquead", body, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(@"not found", body, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(@"inexistente", body, StringComparison.OrdinalIgnoreCase);

            using var document = JsonDocument.Parse(body);
            Assert.Equal(Constants.ErrorCodes.SessionInvalid, document.RootElement.GetProperty(@"code").GetString());
        }

        /// <summary>
        /// Verifica que el cuerpo de error 401 es indistinguible entre una persona inexistente,
        /// una inactiva y una bloqueada.
        /// </summary>
        [Fact]
        public async Task RejectedPersonaErrorResponsesDoNotDistinguishAccountStatus()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var host = await CreateHostAsync(
                environment: Environments.Development,
                mapEndpoints: MapClaimsInspectionEndpoint,
                cancellationToken: cancellationToken);

            using var missingResponse = await host.Client.GetAsync(
                $@"/api/test/claims?{MockPersonaParam}=persona.inexistente",
                cancellationToken);
            using var inactiveResponse = await host.Client.GetAsync(
                $@"/api/test/claims?{MockPersonaParam}=user.inactive",
                cancellationToken);
            using var lockedResponse = await host.Client.GetAsync(
                $@"/api/test/claims?{MockPersonaParam}=user.locked",
                cancellationToken);

            Assert.Equal(HttpStatusCode.Unauthorized, missingResponse.StatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized, inactiveResponse.StatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized, lockedResponse.StatusCode);

            using var missingDoc = JsonDocument.Parse(await missingResponse.Content.ReadAsStringAsync(cancellationToken));
            using var inactiveDoc = JsonDocument.Parse(await inactiveResponse.Content.ReadAsStringAsync(cancellationToken));
            using var lockedDoc = JsonDocument.Parse(await lockedResponse.Content.ReadAsStringAsync(cancellationToken));

            AssertSameGenericProblem(missingDoc.RootElement, inactiveDoc.RootElement);
            AssertSameGenericProblem(missingDoc.RootElement, lockedDoc.RootElement);
        }

        /// <summary>
        /// Verifica que una persona con correo sin verificar autentica con <c>email_verified=false</c>
        /// y permisos limitados sin permisos administrativos ni sensibles de G1.
        /// </summary>
        [Fact]
        public async Task UnverifiedEmailPersonaAuthenticatesWithLimitedPermissions()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var host = await CreateHostAsync(
                environment: Environments.Development,
                mapEndpoints: app =>
                {
                    MapClaimsInspectionEndpoint(app);
                    app.MapGet(@"/api/test/admin", () => TypedResults.Ok(@"admin"))
                        .RequireAuthorization(Constants.Permissions.IdentityAdmin);
                    app.MapGet(@"/api/test/summary", () => TypedResults.Ok(@"summary"))
                        .RequireAuthorization(Constants.Permissions.UserSummaryRead);
                },
                cancellationToken: cancellationToken);

            using var response = await host.Client.GetAsync(
                $@"/api/test/claims?{MockPersonaParam}=user.unverified",
                cancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var snapshot = await response.Content.ReadFromJsonAsync<ClaimsSnapshot>(cancellationToken);
            Assert.NotNull(snapshot);
            Assert.Equal(@"usr_unverified_email", snapshot.Subject);
            Assert.Equal(@"usr_unverified_email", snapshot.NameIdentifier);
            Assert.Equal(@"false", snapshot.EmailVerified);
            Assert.Equal(@"true", snapshot.PhoneVerified);
            Assert.Equal(@"false", snapshot.IsMinor);
            Assert.Empty(snapshot.Permissions);

            using var adminResponse = await host.Client.GetAsync(
                $@"/api/test/admin?{MockPersonaParam}=user.unverified",
                cancellationToken);
            Assert.Equal(HttpStatusCode.Forbidden, adminResponse.StatusCode);

            using var summaryResponse = await host.Client.GetAsync(
                $@"/api/test/summary?{MockPersonaParam}=user.unverified",
                cancellationToken);
            Assert.Equal(HttpStatusCode.Forbidden, summaryResponse.StatusCode);
        }

        /// <summary>
        /// Verifica que incluso si una persona con correo sin verificar tiene asignado un rol privilegiado,
        /// no recibe permisos administrativos ni sensibles de G1 mientras su correo no esté verificado.
        /// </summary>
        [Fact]
        public async Task UnverifiedEmailPersonaWithPrivilegedRoleDoesNotReceiveSensitivePermissions()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var host = await CreateHostAsync(
                environment: Environments.Development,
                mapEndpoints: MapClaimsInspectionEndpoint,
                seedExtraData: async (dbContext, ct) =>
                {
                    dbContext.RoleAssignments.Add(new RoleAssignment
                    {
                        AssignmentId = @"asg_usr_unverified_email_club_admin",
                        UserId = @"usr_unverified_email",
                        RoleCode = Constants.Roles.ClubAdmin,
                        GrantedBy = @"system",
                        GrantedAt = DateTimeOffset.UtcNow,
                        Reason = @"Prueba de permisos limitados sin correo verificado",
                    });
                    await dbContext.SaveChangesAsync(ct);
                },
                cancellationToken: cancellationToken);

            using var response = await host.Client.GetAsync(
                $@"/api/test/claims?{MockPersonaParam}=user.unverified",
                cancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var snapshot = await response.Content.ReadFromJsonAsync<ClaimsSnapshot>(cancellationToken);
            Assert.NotNull(snapshot);
            Assert.Equal(@"false", snapshot.EmailVerified);
            Assert.Empty(snapshot.Permissions);
            Assert.DoesNotContain(Constants.Permissions.IdentityAdmin, snapshot.Permissions);
            Assert.DoesNotContain(Constants.Permissions.UserSummaryRead, snapshot.Permissions);
            Assert.DoesNotContain(Constants.Permissions.UserLookup, snapshot.Permissions);
        }

        /// <summary>
        /// Verifica que una persona menor de edad aplica minimización adicional (D-017)
        /// sin exponer datos de contacto ni del tutor en los claims emitidos.
        /// </summary>
        [Fact]
        public async Task MinorPersonaAppliesAdditionalMinimizationWithoutTutorOrContactData()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var host = await CreateHostAsync(
                environment: Environments.Development,
                mapEndpoints: MapClaimsInspectionEndpoint,
                cancellationToken: cancellationToken);

            using var response = await host.Client.GetAsync(
                $@"/api/test/claims?{MockPersonaParam}=player.minor",
                cancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var snapshot = await response.Content.ReadFromJsonAsync<ClaimsSnapshot>(cancellationToken);
            Assert.NotNull(snapshot);
            Assert.Equal(@"usr_player_minor", snapshot.Subject);
            Assert.Equal(@"usr_player_minor", snapshot.NameIdentifier);
            Assert.Equal(@"true", snapshot.IsMinor);
            Assert.Equal(@"true", snapshot.EmailVerified);
            Assert.Equal(@"true", snapshot.PhoneVerified);
            Assert.Contains(Constants.Roles.Player, snapshot.Roles);
            Assert.Empty(snapshot.Permissions);

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

            foreach (var claim in snapshot.AllClaims)
            {
                Assert.DoesNotContain(claim.Type, forbiddenClaimTypes, StringComparer.OrdinalIgnoreCase);
                Assert.DoesNotContain(@"minor@mock.g1.com", claim.Value, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(@"tutor", claim.Type, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(@"guardian", claim.Type, StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Verifica que las peticiones de escritura autenticadas por cookie exigen token antiforgery.
        /// </summary>
        [Fact]
        public async Task AuthenticatedRequestRequiresAntiforgeryForWrites()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var host = await CreateHostAsync(
                environment: Environments.Development,
                mapEndpoints: app =>
                {
                    app.MapGet(@"/api/test", () => TypedResults.Ok(@"ok"))
                        .RequireAuthorization();
                    app.MapPost(@"/api/test", (HttpContext ctx) =>
                    {
                        var antiforgery = ctx.Features.Get<IAntiforgeryValidationFeature>();
                        return antiforgery is { IsValid: false }
                            ? (IResult)TypedResults.BadRequest()
                            : TypedResults.Ok(@"ok");
                    })
                        .RequireAuthorization()
                        .WithMetadata(new RequireAntiforgeryMetadata());
                },
                cancellationToken: cancellationToken);

            using var loginResponse = await host.Client.GetAsync(
                $@"/api/test?{MockPersonaParam}=admin.club",
                cancellationToken);
            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
            var sessionCookie = loginResponse.Headers.GetValues(@"Set-Cookie").First().Split(';')[0];

            using var request = new HttpRequestMessage(HttpMethod.Post, @"/api/test");
            request.Headers.TryAddWithoutValidation(@"Cookie", sessionCookie);
            using var response = await host.Client.SendAsync(request, cancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        /// <summary>
        /// Verifica que <c>mock_persona</c> no emite tokens JWT por sí mismo.
        /// </summary>
        [Fact]
        public async Task MockPersonaAuthenticationDoesNotEmitJwt()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var host = await CreateHostAsync(
                environment: Environments.Development,
                mapEndpoints: app => app.MapGet(@"/api/test", () => TypedResults.Ok(@"ok")).RequireAuthorization(),
                cancellationToken: cancellationToken);

            using var response = await host.Client.GetAsync(
                $@"/api/test?{MockPersonaParam}=admin.club",
                cancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.False(response.Headers.Contains(@"Authorization"));
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            Assert.DoesNotContain(@"access_token", body, StringComparison.Ordinal);
            Assert.DoesNotContain(@"id_token", body, StringComparison.Ordinal);
            Assert.DoesNotContain(@"refresh_token", body, StringComparison.Ordinal);
            Assert.DoesNotContain(@"eyJ", body, StringComparison.Ordinal);
        }

        private static void MapClaimsInspectionEndpoint(WebApplication app)
        {
            app.MapGet(@"/api/test/claims", (ClaimsPrincipal user) =>
            {
                var snapshot = new ClaimsSnapshot
                {
                    Subject = user.FindFirstValue(Constants.Claims.Subject) ?? String.Empty,
                    NameIdentifier = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? String.Empty,
                    EmailVerified = user.FindFirstValue(@"email_verified") ?? String.Empty,
                    PhoneVerified = user.FindFirstValue(@"phone_verified") ?? String.Empty,
                    IsMinor = user.FindFirstValue(@"is_minor") ?? String.Empty,
                    Roles = user.FindAll(Constants.Claims.Roles).Select(claim => claim.Value).ToArray(),
                    Permissions = user.FindAll(Constants.Claims.Permissions).Select(claim => claim.Value).ToArray(),
                    AllClaims = user.Claims.Select(claim => new ClaimEntry
                    {
                        Type = claim.Type,
                        Value = claim.Value,
                    }).ToArray(),
                };

                return TypedResults.Ok(snapshot);
            }).RequireAuthorization();
        }

        private static void AssertSameGenericProblem(JsonElement expected, JsonElement actual)
        {
            Assert.Equal(expected.GetProperty(@"type").GetString(), actual.GetProperty(@"type").GetString());
            Assert.Equal(expected.GetProperty(@"title").GetString(), actual.GetProperty(@"title").GetString());
            Assert.Equal(expected.GetProperty(@"status").GetInt32(), actual.GetProperty(@"status").GetInt32());
            Assert.Equal(expected.GetProperty(@"detail").GetString(), actual.GetProperty(@"detail").GetString());
            Assert.Equal(expected.GetProperty(@"code").GetString(), actual.GetProperty(@"code").GetString());
        }

        private static async Task<TestHost> CreateHostAsync(
            string environment,
            Action<WebApplication> mapEndpoints,
            CancellationToken cancellationToken,
            Func<G1IdentityDbContext, CancellationToken, Task>? seedExtraData = null)
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                Args = Array.Empty<string>(),
                ContentRootPath = AppContext.BaseDirectory,
                EnvironmentName = environment,
            });

            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                [@"Identity:Issuer"] = @"https://localhost:7180",
                [@"Identity:Lifetimes:AuthorizationCode"] = @"00:05:00",
                [@"Identity:Lifetimes:AccessToken"] = @"00:05:00",
                [@"Identity:Lifetimes:IdentityToken"] = @"00:05:00",
                [@"Identity:Lifetimes:RefreshToken"] = @"08:00:00",
                [@"Identity:Session:IdleTimeout"] = @"00:30:00",
                [@"Identity:Session:AbsoluteTimeout"] = @"12:00:00",
                [@"Identity:Cors:AllowedOrigins:0"] = @"https://localhost:3000",
            });

            builder.Configure();
            builder.Services.AddIdentityPersistence($@"MockPersonaAuthTests_{Guid.NewGuid():N}");

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = @"MockPersona";
                options.DefaultChallengeScheme = @"MockPersona";
            })
            .AddCookie()
            .AddMockPersona();

            builder.WebHost.UseUrls(@"http://127.0.0.1:0");
            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<G1IdentityDbContext>();
                await dbContext.Database.EnsureCreatedAsync(cancellationToken);
                await SeedData.EnsureSeedDataAsync(dbContext, cancellationToken);

                if (seedExtraData is not null)
                {
                    await seedExtraData(dbContext, cancellationToken);
                }
            }

            app.Use(async (context, next) =>
            {
                context.Request.Scheme = @"https";
                await next();
            });
            app.UseMiddlewares();

            mapEndpoints(app);
            await app.StartAsync(cancellationToken);

            var handler = new HttpClientHandler
            {
                UseCookies = false,
            };
            var client = new HttpClient(handler, disposeHandler: true)
            {
                BaseAddress = new Uri(app.Urls.Single()),
            };

            return new TestHost(app, client, cancellationToken);
        }

        private sealed class ClaimsSnapshot
        {
            public string Subject { get; set; } = String.Empty;

            public string NameIdentifier { get; set; } = String.Empty;

            public string EmailVerified { get; set; } = String.Empty;

            public string PhoneVerified { get; set; } = String.Empty;

            public string IsMinor { get; set; } = String.Empty;

            public string[] Roles { get; set; } = Array.Empty<string>();

            public string[] Permissions { get; set; } = Array.Empty<string>();

            public ClaimEntry[] AllClaims { get; set; } = Array.Empty<ClaimEntry>();
        }

        private sealed class ClaimEntry
        {
            public string Type { get; set; } = String.Empty;

            public string Value { get; set; } = String.Empty;
        }

        private sealed class TestHost : IAsyncDisposable
        {
            private readonly WebApplication _app;
            private readonly CancellationToken _cancellationToken;

            internal TestHost(WebApplication app, HttpClient client, CancellationToken cancellationToken)
            {
                _app = app;
                _cancellationToken = cancellationToken;
                Client = client;
            }

            internal HttpClient Client { get; }

            public async ValueTask DisposeAsync()
            {
                Client.Dispose();
                await _app.StopAsync(_cancellationToken);
                await _app.DisposeAsync();
            }
        }

        private sealed class RequireAntiforgeryMetadata : IAntiforgeryMetadata
        {
            public bool RequiresValidation => true;
        }
    }
}
