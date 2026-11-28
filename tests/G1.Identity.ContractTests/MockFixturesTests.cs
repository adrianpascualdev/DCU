using System.Net;
using System.Text.Json;
using G1.Identity.Mock;
using G1.Identity.Mock.Endpoints;
using G1.Identity.Mock.Extensions;
using G1.Identity.Mock.Fixtures;
using G1.Identity.Mock.Persistence;
using G1.Identity.Mock.Persistence.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace G1.Identity.ContractTests
{
    /// <summary>
    /// Pruebas de contrato para los endpoints de fixtures y reset del mock de identidad (T047 y T048).
    /// </summary>
    public sealed class MockFixturesTests
    {
        private const string MockPersonasPath = @"/api/v1/identity/mock/personas";
        private const string MockResetPath = @"/api/v1/identity/mock/reset";

        private static readonly string[] ExpectedPersonaKeys =
        {
            @"admin.club",
            @"sports.manager",
            @"coach.ana",
            @"player.minor",
            @"member.active",
            @"member.phone-unverified",
            @"tutor.familiar",
            @"sponsor.acme",
            @"supporter.fan",
            @"kit.staff",
            @"comms.staff",
            @"user.no-role",
            @"user.unverified",
            @"user.inactive",
            @"user.locked",
        };

        private static readonly string[] TokenScenarioKeys =
        {
            @"token.expired",
            @"token.revoked",
            @"token.wrong-audience",
            @"token.invalid-signature",
        };

        /// <summary>
        /// Verifica que <c>GET /api/v1/identity/mock/personas</c> cumple el esquema de <c>openapi.v1.yaml</c>,
        /// publica las 15 personas sembradas con sus roles, permisos efectivos, estados y etiquetas,
        /// mantiene un <c>fixtureVersion</c> estable y no cuenta los 4 escenarios <c>token.*</c> como personas.
        /// </summary>
        /// <param name="environment">Entorno permitido donde se publican los fixtures del mock.</param>
        [Theory]
        [InlineData(@"Development")]
        [InlineData(@"Testing")]
        public async Task GetMockPersonasReturnsContractSchemaAndAll15SeededPersonasWithoutTokenCases(string environment)
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var host = await CreateHostAsync(environment, cancellationToken);

            using var firstResponse = await host.Client.GetAsync(MockPersonasPath, cancellationToken);
            await ContractResponseAssertions.MatchesAsync(
                firstResponse,
                MockPersonasPath,
                @"get",
                HttpStatusCode.OK,
                cancellationToken);

            var firstJson = await firstResponse.Content.ReadAsStringAsync(cancellationToken);
            using var firstDocument = JsonDocument.Parse(firstJson);
            var root = firstDocument.RootElement;

            var fixtureVersion = root.GetProperty(@"fixtureVersion").GetString();
            Assert.False(String.IsNullOrWhiteSpace(fixtureVersion));
            Assert.Equal(1, root.GetProperty(@"page").GetInt32());
            Assert.Equal(20, root.GetProperty(@"pageSize").GetInt32());
            Assert.Equal(15, root.GetProperty(@"total").GetInt32());

            var items = root.GetProperty(@"items").EnumerateArray().ToList();
            Assert.Equal(15, items.Count);

            var itemsByKey = items.ToDictionary(
                item => item.GetProperty(@"key").GetString() ?? String.Empty,
                StringComparer.Ordinal);

            foreach (var expectedKey in ExpectedPersonaKeys)
            {
                Assert.True(itemsByKey.ContainsKey(expectedKey), $@"Falta la persona esperada '{expectedKey}'.");
            }

            foreach (var tokenKey in TokenScenarioKeys)
            {
                Assert.False(itemsByKey.ContainsKey(tokenKey), $@"El escenario '{tokenKey}' no debe listarse como persona.");
            }

            foreach (var item in items)
            {
                Assert.False(item.TryGetProperty(@"phone", out _), @"El listado de personas no debe incluir el campo phone.");
            }

            AssertPersona(
                itemsByKey[@"admin.club"],
                expectedUserId: @"usr_admin_club",
                expectedStatus: @"active",
                expectedEmailVerified: true,
                expectedPhoneVerified: true,
                expectedIsMinor: false,
                expectedRoles: new[] { Constants.Roles.ClubAdmin },
                expectedPermissions: new[]
                {
                    Constants.Permissions.IdentityAdmin,
                    Constants.Permissions.UserLookup,
                    Constants.Permissions.UserSummaryRead,
                },
                expectedLabels: Array.Empty<string>());

            AssertPersona(
                itemsByKey[@"sports.manager"],
                expectedUserId: @"usr_sports_manager",
                expectedStatus: @"active",
                expectedEmailVerified: true,
                expectedPhoneVerified: true,
                expectedIsMinor: false,
                expectedRoles: new[] { Constants.Roles.SportsManager },
                expectedPermissions: new[]
                {
                    Constants.Permissions.UserLookup,
                    Constants.Permissions.UserSummaryRead,
                },
                expectedLabels: Array.Empty<string>());

            AssertPersona(
                itemsByKey[@"comms.staff"],
                expectedUserId: @"usr_comms_staff",
                expectedStatus: @"active",
                expectedEmailVerified: true,
                expectedPhoneVerified: true,
                expectedIsMinor: false,
                expectedRoles: new[] { Constants.Roles.CommunicationsStaff },
                expectedPermissions: new[] { Constants.Permissions.UserLookup },
                expectedLabels: Array.Empty<string>());

            AssertPersona(
                itemsByKey[@"coach.ana"],
                expectedUserId: @"usr_coach_ana",
                expectedStatus: @"active",
                expectedEmailVerified: true,
                expectedPhoneVerified: true,
                expectedIsMinor: false,
                expectedRoles: new[] { Constants.Roles.Coach },
                expectedPermissions: Array.Empty<string>(),
                expectedLabels: Array.Empty<string>());

            AssertPersona(
                itemsByKey[@"player.minor"],
                expectedUserId: @"usr_player_minor",
                expectedStatus: @"active",
                expectedEmailVerified: true,
                expectedPhoneVerified: true,
                expectedIsMinor: true,
                expectedRoles: new[] { Constants.Roles.Player },
                expectedPermissions: Array.Empty<string>(),
                expectedLabels: new[] { @"minor" });

            AssertPersona(
                itemsByKey[@"member.phone-unverified"],
                expectedUserId: @"usr_member_unverified_phone",
                expectedStatus: @"active",
                expectedEmailVerified: true,
                expectedPhoneVerified: false,
                expectedIsMinor: false,
                expectedRoles: new[] { Constants.Roles.Member },
                expectedPermissions: Array.Empty<string>(),
                expectedLabels: new[] { @"unverified" });

            AssertPersona(
                itemsByKey[@"user.no-role"],
                expectedUserId: @"usr_no_role",
                expectedStatus: @"active",
                expectedEmailVerified: true,
                expectedPhoneVerified: true,
                expectedIsMinor: false,
                expectedRoles: Array.Empty<string>(),
                expectedPermissions: Array.Empty<string>(),
                expectedLabels: Array.Empty<string>());

            AssertPersona(
                itemsByKey[@"user.unverified"],
                expectedUserId: @"usr_unverified_email",
                expectedStatus: @"active",
                expectedEmailVerified: false,
                expectedPhoneVerified: true,
                expectedIsMinor: false,
                expectedRoles: new[] { Constants.Roles.Member },
                expectedPermissions: Array.Empty<string>(),
                expectedLabels: new[] { @"unverified" });

            AssertPersona(
                itemsByKey[@"user.inactive"],
                expectedUserId: @"usr_inactive",
                expectedStatus: @"inactive",
                expectedEmailVerified: true,
                expectedPhoneVerified: true,
                expectedIsMinor: false,
                expectedRoles: new[] { Constants.Roles.Member },
                expectedPermissions: Array.Empty<string>(),
                expectedLabels: Array.Empty<string>());

            AssertPersona(
                itemsByKey[@"user.locked"],
                expectedUserId: @"usr_locked",
                expectedStatus: @"locked",
                expectedEmailVerified: true,
                expectedPhoneVerified: true,
                expectedIsMinor: false,
                expectedRoles: new[] { Constants.Roles.Member },
                expectedPermissions: Array.Empty<string>(),
                expectedLabels: Array.Empty<string>());

            using var secondResponse = await host.Client.GetAsync(MockPersonasPath, cancellationToken);
            Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);
            using var secondDocument = JsonDocument.Parse(await secondResponse.Content.ReadAsStringAsync(cancellationToken));
            Assert.Equal(fixtureVersion, secondDocument.RootElement.GetProperty(@"fixtureVersion").GetString());
        }

        /// <summary>
        /// Verifica que <c>GET /api/v1/identity/mock/personas</c> soporta paginación determinista hasta <c>pageSize=100</c>.
        /// </summary>
        [Fact]
        public async Task GetMockPersonasSupportsPaginationUpToPageSize100()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var host = await CreateHostAsync(Constants.Environments.Testing, cancellationToken);

            using var maxPageResponse = await host.Client.GetAsync($@"{MockPersonasPath}?page=1&pageSize=100", cancellationToken);
            await ContractResponseAssertions.MatchesAsync(
                maxPageResponse,
                MockPersonasPath,
                @"get",
                HttpStatusCode.OK,
                cancellationToken);

            using var maxPageDoc = JsonDocument.Parse(await maxPageResponse.Content.ReadAsStringAsync(cancellationToken));
            var expectedFixtureVersion = maxPageDoc.RootElement.GetProperty(@"fixtureVersion").GetString();
            Assert.Equal(1, maxPageDoc.RootElement.GetProperty(@"page").GetInt32());
            Assert.Equal(100, maxPageDoc.RootElement.GetProperty(@"pageSize").GetInt32());
            Assert.Equal(15, maxPageDoc.RootElement.GetProperty(@"total").GetInt32());
            Assert.Equal(15, maxPageDoc.RootElement.GetProperty(@"items").GetArrayLength());

            var collectedKeys = new HashSet<string>(StringComparer.Ordinal);
            for (var page = 1; page <= 3; page++)
            {
                using var pageResponse = await host.Client.GetAsync($@"{MockPersonasPath}?page={page}&pageSize=5", cancellationToken);
                await ContractResponseAssertions.MatchesAsync(
                    pageResponse,
                    MockPersonasPath,
                    @"get",
                    HttpStatusCode.OK,
                    cancellationToken);

                using var pageDoc = JsonDocument.Parse(await pageResponse.Content.ReadAsStringAsync(cancellationToken));
                Assert.Equal(expectedFixtureVersion, pageDoc.RootElement.GetProperty(@"fixtureVersion").GetString());
                Assert.Equal(page, pageDoc.RootElement.GetProperty(@"page").GetInt32());
                Assert.Equal(5, pageDoc.RootElement.GetProperty(@"pageSize").GetInt32());
                Assert.Equal(15, pageDoc.RootElement.GetProperty(@"total").GetInt32());

                var pageItems = pageDoc.RootElement.GetProperty(@"items").EnumerateArray().ToList();
                Assert.Equal(5, pageItems.Count);
                foreach (var item in pageItems)
                {
                    var key = item.GetProperty(@"key").GetString() ?? String.Empty;
                    Assert.True(collectedKeys.Add(key), $@"La clave '{key}' apareció repetida entre páginas.");
                }
            }

            Assert.Equal(15, collectedKeys.Count);

            using var beyondLastPage = await host.Client.GetAsync($@"{MockPersonasPath}?page=4&pageSize=5", cancellationToken);
            await ContractResponseAssertions.MatchesAsync(
                beyondLastPage,
                MockPersonasPath,
                @"get",
                HttpStatusCode.OK,
                cancellationToken);

            using var beyondDoc = JsonDocument.Parse(await beyondLastPage.Content.ReadAsStringAsync(cancellationToken));
            Assert.Equal(expectedFixtureVersion, beyondDoc.RootElement.GetProperty(@"fixtureVersion").GetString());
            Assert.Equal(4, beyondDoc.RootElement.GetProperty(@"page").GetInt32());
            Assert.Equal(5, beyondDoc.RootElement.GetProperty(@"pageSize").GetInt32());
            Assert.Equal(15, beyondDoc.RootElement.GetProperty(@"total").GetInt32());
            Assert.Equal(0, beyondDoc.RootElement.GetProperty(@"items").GetArrayLength());
        }

        /// <summary>
        /// Verifica que los parámetros de paginación inválidos se rechazan con 400 <c>application/problem+json</c>.
        /// </summary>
        /// <param name="queryString">Cadena de consulta con parámetros inválidos.</param>
        [Theory]
        [InlineData(@"?page=0")]
        [InlineData(@"?page=-1")]
        [InlineData(@"?pageSize=0")]
        [InlineData(@"?pageSize=-5")]
        [InlineData(@"?pageSize=101")]
        [InlineData(@"?page=0&pageSize=101")]
        [InlineData(@"?page=invalido")]
        [InlineData(@"?pageSize=invalido")]
        public async Task GetMockPersonasRejectsInvalidPaginationParametersWithProblemJson(string queryString)
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var host = await CreateHostAsync(Constants.Environments.Testing, cancellationToken);

            using var response = await host.Client.GetAsync($@"{MockPersonasPath}{queryString}", cancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(Constants.ContentTypes.ProblemJson, response.Content.Headers.ContentType?.MediaType);

            using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var root = problem.RootElement;
            Assert.Equal(400, root.GetProperty(@"status").GetInt32());
            Assert.Equal(Constants.ErrorCodes.RequestInvalid, root.GetProperty(@"code").GetString());
            Assert.Equal(MockPersonasPath, root.GetProperty(@"instance").GetString());
            Assert.False(String.IsNullOrWhiteSpace(root.GetProperty(@"type").GetString()));
            Assert.False(String.IsNullOrWhiteSpace(root.GetProperty(@"title").GetString()));
            Assert.False(String.IsNullOrWhiteSpace(root.GetProperty(@"detail").GetString()));
            Assert.False(String.IsNullOrWhiteSpace(root.GetProperty(@"traceId").GetString()));
        }

        /// <summary>
        /// Verifica que <c>POST /api/v1/identity/mock/reset</c> devuelve 204, es idempotente,
        /// elimina sesiones, tokens de sesión y eventos de auditoría, y vuelve a sembrar
        /// personas, roles, permisos y asignaciones sin duplicados.
        /// </summary>
        /// <param name="environment">Entorno permitido donde se puede ejecutar el reset.</param>
        [Theory]
        [InlineData(@"Development")]
        [InlineData(@"Testing")]
        public async Task PostMockResetIsIdempotentClearsRuntimeStateAndReseedsWithoutDuplicates(string environment)
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var host = await CreateHostAsync(environment, cancellationToken);

            using (var scope = host.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<G1IdentityDbContext>();
                var now = DateTimeOffset.UtcNow;

                dbContext.Sessions.Add(new Session
                {
                    SessionId = @"ses_test_reset_01",
                    UserId = @"usr_admin_club",
                    ClientId = @"cli_mock_spa",
                    CreatedAt = now,
                    LastSeenAt = now,
                    ExpiresAt = now.AddMinutes(30),
                    AbsoluteExpiresAt = now.AddHours(12),
                    Status = SessionStatus.Active,
                    DeviceLabel = @"Navegador de pruebas",
                });

                dbContext.SessionTokens.Add(new SessionToken
                {
                    TokenId = @"tok_test_reset_01",
                    SessionId = @"ses_test_reset_01",
                    TokenType = SessionTokenType.RefreshToken,
                    CreatedAt = now,
                });

                dbContext.AuditEvents.Add(new AuditEvent
                {
                    EventId = @"evt_test_reset_01",
                    Type = AuditEventType.Logout,
                    ActorId = @"usr_admin_club",
                    TargetId = @"ses_test_reset_01",
                    Result = AuditEventResult.Success,
                    CorrelationId = @"corr-reset-01",
                    Timestamp = now,
                });

                dbContext.MockPersonas.Add(new MockPersona
                {
                    Key = @"extra.persona",
                    UserId = @"usr_extra_persona",
                    DisplayName = @"Persona Temporal",
                    Email = @"extra@mock.g1.com",
                    EmailVerified = true,
                    PhoneVerified = true,
                    FixtureVersion = @"v0.0.0",
                });

                dbContext.RoleAssignments.Add(new RoleAssignment
                {
                    AssignmentId = @"asg_usr_extra_persona_member",
                    UserId = @"usr_extra_persona",
                    RoleCode = Constants.Roles.Member,
                    GrantedBy = @"system",
                    GrantedAt = now,
                    Reason = @"Asignación temporal para prueba de reset",
                });

                var adminPersona = await dbContext.MockPersonas.SingleAsync(
                    persona => persona.Key == @"admin.club",
                    cancellationToken);
                adminPersona.DisplayName = @"Nombre Alterado Temporalmente";

                await dbContext.SaveChangesAsync(cancellationToken);
            }

            using var firstReset = await host.Client.PostAsync(MockResetPath, content: null, cancellationToken);
            await ContractResponseAssertions.MatchesAsync(
                firstReset,
                MockResetPath,
                @"post",
                HttpStatusCode.NoContent,
                cancellationToken);

            var snapshotAfterFirstReset = await ReadPersonasJsonAsync(host.Client, cancellationToken);
            await AssertCleanSeededDatabaseStateAsync(host.Services, cancellationToken);

            using var secondReset = await host.Client.PostAsync(MockResetPath, content: null, cancellationToken);
            await ContractResponseAssertions.MatchesAsync(
                secondReset,
                MockResetPath,
                @"post",
                HttpStatusCode.NoContent,
                cancellationToken);

            var snapshotAfterSecondReset = await ReadPersonasJsonAsync(host.Client, cancellationToken);
            await AssertCleanSeededDatabaseStateAsync(host.Services, cancellationToken);

            Assert.Equal(snapshotAfterFirstReset, snapshotAfterSecondReset);
        }

        /// <summary>
        /// Verifica que tanto <c>GET /api/v1/identity/mock/personas</c> como <c>POST /api/v1/identity/mock/reset</c>
        /// devuelven 404 fuera de los entornos Development y Testing.
        /// </summary>
        /// <param name="environment">Entorno restringido donde los endpoints del mock no están disponibles.</param>
        [Theory]
        [InlineData(@"Production")]
        [InlineData(@"Staging")]
        public async Task MockEndpointsReturnNotFoundOutsideDevelopmentAndTesting(string environment)
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var host = await CreateHostAsync(environment, cancellationToken);

            using var personasResponse = await host.Client.GetAsync(MockPersonasPath, cancellationToken);
            await ContractResponseAssertions.MatchesAsync(
                personasResponse,
                MockPersonasPath,
                @"get",
                HttpStatusCode.NotFound,
                cancellationToken);

            using var resetResponse = await host.Client.PostAsync(MockResetPath, content: null, cancellationToken);
            await ContractResponseAssertions.MatchesAsync(
                resetResponse,
                MockResetPath,
                @"post",
                HttpStatusCode.NotFound,
                cancellationToken);
        }

        private static void AssertPersona(
            JsonElement persona,
            string expectedUserId,
            string expectedStatus,
            bool expectedEmailVerified,
            bool expectedPhoneVerified,
            bool expectedIsMinor,
            string[] expectedRoles,
            string[] expectedPermissions,
            string[] expectedLabels)
        {
            Assert.Equal(expectedUserId, persona.GetProperty(@"userId").GetString());
            Assert.Equal(expectedStatus, persona.GetProperty(@"status").GetString());
            Assert.Equal(expectedEmailVerified, persona.GetProperty(@"emailVerified").GetBoolean());
            Assert.Equal(expectedPhoneVerified, persona.GetProperty(@"phoneVerified").GetBoolean());
            Assert.Equal(expectedIsMinor, persona.GetProperty(@"isMinor").GetBoolean());

            var actualRoles = persona.GetProperty(@"roles")
                .EnumerateArray()
                .Select(item => item.GetString() ?? String.Empty)
                .ToArray();
            Assert.Equal(expectedRoles, actualRoles);

            var actualPermissions = persona.GetProperty(@"permissions")
                .EnumerateArray()
                .Select(item => item.GetString() ?? String.Empty)
                .ToArray();
            Assert.Equal(expectedPermissions, actualPermissions);

            var actualLabels = persona.GetProperty(@"labels")
                .EnumerateArray()
                .Select(item => item.GetString() ?? String.Empty)
                .ToArray();
            Assert.Equal(expectedLabels, actualLabels);

            var authenticationMethods = persona.GetProperty(@"authenticationMethods")
                .EnumerateArray()
                .Select(item => item.GetString() ?? String.Empty)
                .ToArray();
            Assert.NotEmpty(authenticationMethods);
        }

        private static async Task<string> ReadPersonasJsonAsync(HttpClient client, CancellationToken cancellationToken)
        {
            using var response = await client.GetAsync(MockPersonasPath, cancellationToken);
            await ContractResponseAssertions.MatchesAsync(
                response,
                MockPersonasPath,
                @"get",
                HttpStatusCode.OK,
                cancellationToken);

            return await response.Content.ReadAsStringAsync(cancellationToken);
        }

        private static async Task AssertCleanSeededDatabaseStateAsync(
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<G1IdentityDbContext>();

            Assert.Equal(0, await dbContext.Sessions.CountAsync(cancellationToken));
            Assert.Equal(0, await dbContext.SessionTokens.CountAsync(cancellationToken));
            Assert.Equal(0, await dbContext.AuditEvents.CountAsync(cancellationToken));

            Assert.Equal(15, await dbContext.MockPersonas.CountAsync(cancellationToken));
            Assert.Equal(10, await dbContext.Roles.CountAsync(cancellationToken));
            Assert.Equal(3, await dbContext.Permissions.CountAsync(cancellationToken));
            Assert.Equal(6, await dbContext.RolePermissions.CountAsync(cancellationToken));
            Assert.Equal(14, await dbContext.RoleAssignments.CountAsync(cancellationToken));

            var adminPersona = await dbContext.MockPersonas
                .AsNoTracking()
                .SingleAsync(persona => persona.Key == @"admin.club", cancellationToken);
            Assert.Equal(@"Admin G1", adminPersona.DisplayName);
            Assert.Equal(@"usr_admin_club", adminPersona.UserId);
        }

        private static async Task<TestHost> CreateHostAsync(string environment, CancellationToken cancellationToken)
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
            builder.Services.AddIdentityPersistence($@"MockFixturesTests_{Guid.NewGuid():N}");
            builder.WebHost.UseUrls(@"http://127.0.0.1:0");

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<G1IdentityDbContext>();
                await dbContext.Database.EnsureCreatedAsync(cancellationToken);
                await SeedData.EnsureSeedDataAsync(dbContext, cancellationToken);
            }

            app.UseMiddlewares();
            app.MapMockPersonaEndpoints();
            await app.StartAsync(cancellationToken);

            var client = new HttpClient
            {
                BaseAddress = new Uri(app.Urls.Single()),
            };

            return new TestHost(app, client, cancellationToken);
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

            internal IServiceProvider Services => _app.Services;

            public async ValueTask DisposeAsync()
            {
                Client.Dispose();
                await _app.StopAsync(_cancellationToken);
                await _app.DisposeAsync();
            }
        }
    }
}
