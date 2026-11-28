using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using G1.Identity.Mock.Fixtures;
using G1.Identity.Mock.Persistence;
using G1.Identity.Mock.Persistence.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;

namespace G1.Identity.CompatibilityTests
{
    /// <summary>
    /// Pruebas de compatibilidad sobre SQLite en memoria para detectar diferencias de proveedor
    /// respecto a EF Core InMemory y validar la mitigación obligatoria de ADR-0002 (T055).
    /// </summary>
    public sealed class ProviderSmokeTests
    {
        private const string InMemorySqliteConnectionString = @"DataSource=:memory:";
        private const string ExpectedCatalogVersion = @"v1.0.0-provisional";
        private const string ExpectedFixtureVersion = @"v1.0.0";

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
        /// Verifica que <see cref="G1IdentityDbContext"/> crea sin errores de mapeo ni de tipos
        /// las 9 entidades de G1 y las 4 tablas de OpenIddict mediante <c>EnsureCreatedAsync</c> en SQLite.
        /// </summary>
        [Fact]
        public async Task EnsureCreatedCreatesAllG1AndOpenIddictTablesInSqliteAsync()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var harness = await SqliteTestHarness.CreateAsync(cancellationToken);

            var expectedEntityTypes = new[]
            {
                typeof(MockPersona),
                typeof(Role),
                typeof(Permission),
                typeof(RolePermission),
                typeof(RoleAssignment),
                typeof(Session),
                typeof(SessionToken),
                typeof(AuditEvent),
                typeof(CatalogSnapshot),
                typeof(OpenIddictEntityFrameworkCoreApplication),
                typeof(OpenIddictEntityFrameworkCoreAuthorization),
                typeof(OpenIddictEntityFrameworkCoreScope),
                typeof(OpenIddictEntityFrameworkCoreToken),
            };

            var expectedTableNames = new HashSet<string>(StringComparer.Ordinal);
            await using (var scope = harness.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<G1IdentityDbContext>();

                foreach (var entityType in expectedEntityTypes)
                {
                    var mappedEntity = dbContext.Model.FindEntityType(entityType);
                    Assert.NotNull(mappedEntity);

                    var tableName = mappedEntity.GetTableName();
                    Assert.False(String.IsNullOrWhiteSpace(tableName));
                    expectedTableNames.Add(tableName);
                }

                Assert.Equal(13, expectedTableNames.Count);
                Assert.Equal(0, await dbContext.MockPersonas.CountAsync(cancellationToken));
                Assert.Equal(0, await dbContext.Roles.CountAsync(cancellationToken));
                Assert.Equal(0, await dbContext.Permissions.CountAsync(cancellationToken));
                Assert.Equal(0, await dbContext.RolePermissions.CountAsync(cancellationToken));
                Assert.Equal(0, await dbContext.RoleAssignments.CountAsync(cancellationToken));
                Assert.Equal(0, await dbContext.Sessions.CountAsync(cancellationToken));
                Assert.Equal(0, await dbContext.SessionTokens.CountAsync(cancellationToken));
                Assert.Equal(0, await dbContext.AuditEvents.CountAsync(cancellationToken));
                Assert.Equal(0, await dbContext.CatalogSnapshots.CountAsync(cancellationToken));

                var publishedAt = DateTimeOffset.UtcNow;
                dbContext.CatalogSnapshots.Add(new CatalogSnapshot
                {
                    CatalogVersion = ExpectedCatalogVersion,
                    Provisional = true,
                    PublishedAt = publishedAt,
                });

                await dbContext.SaveChangesAsync(cancellationToken);
            }

            var actualTables = await harness.GetCreatedTableNamesAsync(cancellationToken);
            foreach (var expectedTableName in expectedTableNames)
            {
                Assert.Contains(expectedTableName, actualTables);
            }

            await using (var verifyScope = harness.CreateScope())
            {
                var dbContext = verifyScope.ServiceProvider.GetRequiredService<G1IdentityDbContext>();
                var snapshot = await dbContext.CatalogSnapshots
                    .AsNoTracking()
                    .SingleAsync(item => item.CatalogVersion == ExpectedCatalogVersion, cancellationToken);

                Assert.True(snapshot.Provisional);
            }
        }

        /// <summary>
        /// Verifica que <see cref="SeedData.EnsureSeedDataAsync"/> es idempotente en SQLite
        /// al ejecutarse dos veces consecutivas sin duplicados ni violaciones de unicidad.
        /// </summary>
        [Fact]
        public async Task EnsureSeedDataIsIdempotentOnConsecutiveExecutionsInSqliteAsync()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var harness = await SqliteTestHarness.CreateAsync(cancellationToken);

            await using (var firstScope = harness.CreateScope())
            {
                var dbContext = firstScope.ServiceProvider.GetRequiredService<G1IdentityDbContext>();
                await SeedData.EnsureSeedDataAsync(dbContext, cancellationToken);
            }

            await using (var secondScope = harness.CreateScope())
            {
                var dbContext = secondScope.ServiceProvider.GetRequiredService<G1IdentityDbContext>();
                await SeedData.EnsureSeedDataAsync(dbContext, cancellationToken);
            }

            await using (var verifyScope = harness.CreateScope())
            {
                var dbContext = verifyScope.ServiceProvider.GetRequiredService<G1IdentityDbContext>();

                var personas = await dbContext.MockPersonas
                    .AsNoTracking()
                    .OrderBy(persona => persona.Key)
                    .ToListAsync(cancellationToken);

                var roles = await dbContext.Roles
                    .AsNoTracking()
                    .OrderBy(role => role.Code)
                    .ToListAsync(cancellationToken);

                var permissions = await dbContext.Permissions
                    .AsNoTracking()
                    .OrderBy(permission => permission.Code)
                    .ToListAsync(cancellationToken);

                var rolePermissions = await dbContext.RolePermissions
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                var roleAssignments = await dbContext.RoleAssignments
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                Assert.Equal(15, personas.Count);
                Assert.Equal(10, roles.Count);
                Assert.Equal(3, permissions.Count);
                Assert.Equal(6, rolePermissions.Count);
                Assert.Equal(14, roleAssignments.Count);

                var personaKeys = personas.Select(persona => persona.Key).ToHashSet(StringComparer.Ordinal);
                foreach (var expectedKey in ExpectedPersonaKeys)
                {
                    Assert.Contains(expectedKey, personaKeys);
                }

                foreach (var tokenScenarioKey in TokenScenarioKeys)
                {
                    Assert.DoesNotContain(tokenScenarioKey, personaKeys);
                }

                Assert.All(personas, persona =>
                {
                    Assert.Equal(ExpectedFixtureVersion, persona.FixtureVersion);
                    Assert.Contains(@"pwd", persona.AuthenticationMethods);
                    Assert.Contains(@"mock_persona", persona.AuthenticationMethods);
                });

                var minorPersona = personas.Single(persona => persona.Key == @"player.minor");
                Assert.True(minorPersona.IsMinor);
                Assert.Contains(@"minor", minorPersona.Labels);

                Assert.All(rolePermissions, entry => Assert.True(entry.Granted));
                Assert.Equal(3, rolePermissions.Count(entry => entry.RoleCode == @"club_admin"));
                Assert.Equal(2, rolePermissions.Count(entry => entry.RoleCode == @"sports_manager"));
                Assert.Equal(1, rolePermissions.Count(entry => entry.RoleCode == @"communications_staff"));
                Assert.All(roleAssignments, assignment => Assert.True(assignment.IsActive()));
                Assert.DoesNotContain(roleAssignments, assignment => assignment.UserId == @"usr_no_role");
            }
        }

        /// <summary>
        /// Verifica que SQLite aplica las restricciones relacionales de unicidad de <see cref="MockPersona"/>
        /// rechazando valores duplicados en <c>Key</c>, <c>UserId</c> y <c>Email</c>.
        /// </summary>
        [Fact]
        public async Task MockPersonaRejectsDuplicateKeyUserIdAndEmailInSqliteAsync()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var harness = await SqliteTestHarness.CreateAsync(cancellationToken);

            await using (var seedScope = harness.CreateScope())
            {
                var dbContext = seedScope.ServiceProvider.GetRequiredService<G1IdentityDbContext>();
                await SeedData.EnsureSeedDataAsync(dbContext, cancellationToken);
            }

            await AssertDuplicateMockPersonaRejectedAsync(
                harness,
                new MockPersona
                {
                    Key = @"admin.club",
                    UserId = @"usr_duplicate_key_check",
                    DisplayName = @"Persona Clave Duplicada",
                    Email = @"duplicate.key@mock.g1.com",
                    EmailVerified = true,
                    PhoneVerified = true,
                    FixtureVersion = ExpectedFixtureVersion,
                },
                cancellationToken);

            await AssertDuplicateMockPersonaRejectedAsync(
                harness,
                new MockPersona
                {
                    Key = @"duplicate.userid.check",
                    UserId = @"usr_admin_club",
                    DisplayName = @"Persona UserId Duplicado",
                    Email = @"duplicate.userid@mock.g1.com",
                    EmailVerified = true,
                    PhoneVerified = true,
                    FixtureVersion = ExpectedFixtureVersion,
                },
                cancellationToken);

            await AssertDuplicateMockPersonaRejectedAsync(
                harness,
                new MockPersona
                {
                    Key = @"duplicate.email.check",
                    UserId = @"usr_duplicate_email_check",
                    DisplayName = @"Persona Email Duplicado",
                    Email = @"admin@mock.g1.com",
                    EmailVerified = true,
                    PhoneVerified = true,
                    FixtureVersion = ExpectedFixtureVersion,
                },
                cancellationToken);

            await using (var verifyScope = harness.CreateScope())
            {
                var dbContext = verifyScope.ServiceProvider.GetRequiredService<G1IdentityDbContext>();
                Assert.Equal(15, await dbContext.MockPersonas.CountAsync(cancellationToken));
            }
        }

        /// <summary>
        /// Verifica la creación, lectura y actualización de <see cref="Session"/>,
        /// <see cref="SessionToken"/> y <see cref="AuditEvent"/> en SQLite.
        /// </summary>
        [Fact]
        public async Task SessionSessionTokenAndAuditEventSupportCreateAndUpdateInSqliteAsync()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var harness = await SqliteTestHarness.CreateAsync(cancellationToken);

            var nowUtc = DateTimeOffset.UtcNow;
            var sessionId = @"ses_sqlite_smoke_001";
            var tokenId = @"tok_sqlite_refresh_001";
            var authEventId = @"evt_sqlite_auth_001";
            var revokeEventId = @"evt_sqlite_revoke_002";

            await using (var createScope = harness.CreateScope())
            {
                var dbContext = createScope.ServiceProvider.GetRequiredService<G1IdentityDbContext>();

                var session = new Session
                {
                    SessionId = sessionId,
                    UserId = @"usr_admin_club",
                    ClientId = @"cli_mock_spa",
                    CreatedAt = nowUtc,
                    LastSeenAt = nowUtc,
                    ExpiresAt = nowUtc.AddMinutes(30),
                    AbsoluteExpiresAt = nowUtc.AddHours(12),
                    Status = SessionStatus.Active,
                    AuthenticationMethods = new List<string> { @"pwd", @"mock_persona" },
                    DeviceLabel = @"Navegador SQLite de pruebas",
                };

                var sessionToken = new SessionToken
                {
                    TokenId = tokenId,
                    SessionId = sessionId,
                    TokenType = SessionTokenType.RefreshToken,
                    CreatedAt = nowUtc,
                };

                var auditEvent = new AuditEvent
                {
                    EventId = authEventId,
                    Type = AuditEventType.Authentication,
                    ActorId = @"usr_admin_club",
                    TargetId = sessionId,
                    Result = AuditEventResult.Success,
                    CorrelationId = @"corr-sqlite-001",
                    Timestamp = nowUtc,
                };

                auditEvent.AddMetadata(@"provider", @"sqlite");
                Assert.Throws<InvalidOperationException>(() => auditEvent.AddMetadata(@"access_token", @"valor-prohibido"));

                dbContext.Sessions.Add(session);
                dbContext.SessionTokens.Add(sessionToken);
                dbContext.AuditEvents.Add(auditEvent);

                await dbContext.SaveChangesAsync(cancellationToken);
            }

            await using (var updateScope = harness.CreateScope())
            {
                var dbContext = updateScope.ServiceProvider.GetRequiredService<G1IdentityDbContext>();

                var persistedSession = await dbContext.Sessions
                    .SingleAsync(item => item.SessionId == sessionId, cancellationToken);

                Assert.Equal(SessionStatus.Active, persistedSession.Status);
                Assert.True(persistedSession.HasValidExpirations());
                Assert.True(persistedSession.IsActive());
                Assert.Equal(new[] { @"pwd", @"mock_persona" }, persistedSession.AuthenticationMethods);

                var persistedToken = await dbContext.SessionTokens
                    .AsNoTracking()
                    .SingleAsync(item => item.TokenId == tokenId, cancellationToken);

                Assert.Equal(sessionId, persistedToken.SessionId);
                Assert.Equal(SessionTokenType.RefreshToken, persistedToken.TokenType);

                var revokedAtUtc = nowUtc.AddMinutes(2);
                persistedSession.LastSeenAt = revokedAtUtc;
                persistedSession.Status = SessionStatus.Revoked;
                persistedSession.RevokedAt = revokedAtUtc;
                persistedSession.RevocationReason = @"user_revoked";

                dbContext.AuditEvents.Add(new AuditEvent
                {
                    EventId = revokeEventId,
                    Type = AuditEventType.SessionRevoked,
                    ActorId = @"usr_admin_club",
                    TargetId = sessionId,
                    Result = AuditEventResult.Success,
                    CorrelationId = @"corr-sqlite-002",
                    Timestamp = revokedAtUtc,
                });

                await dbContext.SaveChangesAsync(cancellationToken);
            }

            await using (var verifyScope = harness.CreateScope())
            {
                var dbContext = verifyScope.ServiceProvider.GetRequiredService<G1IdentityDbContext>();

                var updatedSession = await dbContext.Sessions
                    .AsNoTracking()
                    .SingleAsync(item => item.SessionId == sessionId, cancellationToken);

                Assert.Equal(SessionStatus.Revoked, updatedSession.Status);
                Assert.False(updatedSession.IsActive());
                Assert.NotNull(updatedSession.RevokedAt);
                Assert.Equal(@"user_revoked", updatedSession.RevocationReason);

                var auditEvents = await dbContext.AuditEvents
                    .AsNoTracking()
                    .OrderBy(item => item.EventId)
                    .ToListAsync(cancellationToken);

                Assert.Equal(2, auditEvents.Count);
                Assert.Equal(authEventId, auditEvents[0].EventId);
                Assert.Equal(AuditEventType.Authentication, auditEvents[0].Type);
                Assert.Equal(revokeEventId, auditEvents[1].EventId);
                Assert.Equal(AuditEventType.SessionRevoked, auditEvents[1].Type);
            }
        }

        /// <summary>
        /// Verifica en SQLite las consultas agnósticas del proveedor equivalentes a
        /// <c>MockPersonaAuthentication</c>, <c>PersonaSelectionValidator</c> y <c>MockPersonaEndpoints</c>,
        /// incluido el borrado completo de estado sin operaciones masivas y posterior resembrado.
        /// </summary>
        [Fact]
        public async Task ProviderAgnosticQueriesAndResetWithoutBulkOperationsSucceedInSqliteAsync()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var harness = await SqliteTestHarness.CreateAsync(cancellationToken);

            await using (var seedScope = harness.CreateScope())
            {
                var dbContext = seedScope.ServiceProvider.GetRequiredService<G1IdentityDbContext>();
                await SeedData.EnsureSeedDataAsync(dbContext, cancellationToken);

                var nowUtc = DateTimeOffset.UtcNow;

                // Añadir una asignación revocada para comprobar que nunca contribuye a roles ni permisos.
                dbContext.RoleAssignments.Add(new RoleAssignment
                {
                    AssignmentId = @"asg_usr_sports_manager_revoked_admin",
                    UserId = @"usr_sports_manager",
                    RoleCode = @"club_admin",
                    GrantedBy = @"system",
                    GrantedAt = nowUtc.AddHours(-2),
                    Reason = @"Asignación revocada de prueba SQLite",
                    RevokedAt = nowUtc.AddHours(-1),
                });

                // Añadir estado transitorio para verificar el borrado sin operaciones masivas.
                dbContext.Sessions.Add(new Session
                {
                    SessionId = @"ses_sqlite_reset_001",
                    UserId = @"usr_admin_club",
                    ClientId = @"cli_mock_spa",
                    CreatedAt = nowUtc,
                    LastSeenAt = nowUtc,
                    ExpiresAt = nowUtc.AddMinutes(30),
                    AbsoluteExpiresAt = nowUtc.AddHours(12),
                    Status = SessionStatus.Active,
                    DeviceLabel = @"Sesión temporal para reset",
                });

                dbContext.SessionTokens.Add(new SessionToken
                {
                    TokenId = @"tok_sqlite_reset_001",
                    SessionId = @"ses_sqlite_reset_001",
                    TokenType = SessionTokenType.RefreshToken,
                    CreatedAt = nowUtc,
                });

                dbContext.AuditEvents.Add(new AuditEvent
                {
                    EventId = @"evt_sqlite_reset_001",
                    Type = AuditEventType.MockReset,
                    ActorId = @"system",
                    TargetId = @"usr_admin_club",
                    Result = AuditEventResult.Success,
                    CorrelationId = @"corr-sqlite-reset",
                    Timestamp = nowUtc,
                });

                dbContext.CatalogSnapshots.Add(new CatalogSnapshot
                {
                    CatalogVersion = ExpectedCatalogVersion,
                    Provisional = true,
                    PublishedAt = nowUtc,
                });

                await dbContext.SaveChangesAsync(cancellationToken);

                // Crear autorización y token de OpenIddict para verificar su borrado en SQLite sin operaciones masivas.
                var applicationManager = seedScope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
                var authorizationManager = seedScope.ServiceProvider.GetRequiredService<IOpenIddictAuthorizationManager>();
                var tokenManager = seedScope.ServiceProvider.GetRequiredService<IOpenIddictTokenManager>();

                var resetApp = await applicationManager.CreateAsync(
                    new OpenIddictApplicationDescriptor
                    {
                        ClientId = @"cli_sqlite_reset_spa",
                        DisplayName = @"Cliente temporal de prueba de reset SQLite",
                        ClientType = OpenIddictConstants.ClientTypes.Public,
                    },
                    cancellationToken);

                var resetAppId = await applicationManager.GetIdAsync(resetApp, cancellationToken);
                Assert.False(String.IsNullOrWhiteSpace(resetAppId));

                var resetAuth = await authorizationManager.CreateAsync(
                    new OpenIddictAuthorizationDescriptor
                    {
                        ApplicationId = resetAppId,
                        Subject = @"usr_admin_club",
                        Status = OpenIddictConstants.Statuses.Valid,
                        Type = OpenIddictConstants.AuthorizationTypes.Permanent,
                        CreationDate = nowUtc,
                    },
                    cancellationToken);

                var resetAuthId = await authorizationManager.GetIdAsync(resetAuth, cancellationToken);
                Assert.False(String.IsNullOrWhiteSpace(resetAuthId));

                await tokenManager.CreateAsync(
                    new OpenIddictTokenDescriptor
                    {
                        ApplicationId = resetAppId,
                        AuthorizationId = resetAuthId,
                        Subject = @"usr_admin_club",
                        Type = OpenIddictConstants.TokenTypeHints.RefreshToken,
                        Status = OpenIddictConstants.Statuses.Valid,
                        ReferenceId = @"ref_sqlite_reset_001",
                        CreationDate = nowUtc,
                        ExpirationDate = nowUtc.AddHours(8),
                    },
                    cancellationToken);
            }

            // Consultas equivalentes a MockPersonaAuthentication y PersonaSelectionValidator.
            await using (var authQueryScope = harness.CreateScope())
            {
                var dbContext = authQueryScope.ServiceProvider.GetRequiredService<G1IdentityDbContext>();

                var byKeyIdentifier = @"admin.club";
                var adminPersona = await dbContext.MockPersonas
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        candidate => candidate.Key == byKeyIdentifier || candidate.UserId == byKeyIdentifier,
                        cancellationToken);

                Assert.NotNull(adminPersona);
                Assert.True(adminPersona.CanAuthorize());

                var byUserIdIdentifier = @"usr_sports_manager";
                var sportsManager = await dbContext.MockPersonas
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        candidate => candidate.Key == byUserIdIdentifier || candidate.UserId == byUserIdIdentifier,
                        cancellationToken);

                Assert.NotNull(sportsManager);
                Assert.True(sportsManager.CanAuthorize());

                var activeRoles = await dbContext.RoleAssignments
                    .AsNoTracking()
                    .Where(assignment => assignment.UserId == sportsManager.UserId && assignment.RevokedAt == null)
                    .Select(assignment => assignment.RoleCode)
                    .Distinct()
                    .OrderBy(roleCode => roleCode)
                    .ToListAsync(cancellationToken);

                Assert.Equal(new[] { @"sports_manager" }, activeRoles);

                var effectivePermissions = await dbContext.RolePermissions
                    .AsNoTracking()
                    .Where(rolePermission => rolePermission.Granted && activeRoles.Contains(rolePermission.RoleCode))
                    .Select(rolePermission => rolePermission.PermissionCode)
                    .Distinct()
                    .OrderBy(permissionCode => permissionCode)
                    .ToListAsync(cancellationToken);

                Assert.Equal(
                    new[] { @"identity.users.lookup", @"identity.users.summary.read" },
                    effectivePermissions);

                var inactiveIdentifier = @"user.inactive";
                var inactivePersona = await dbContext.MockPersonas
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        candidate => candidate.Key == inactiveIdentifier || candidate.UserId == inactiveIdentifier,
                        cancellationToken);

                Assert.NotNull(inactivePersona);
                Assert.False(inactivePersona.CanAuthorize());

                var lockedIdentifier = @"user.locked";
                var lockedPersona = await dbContext.MockPersonas
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        candidate => candidate.Key == lockedIdentifier || candidate.UserId == lockedIdentifier,
                        cancellationToken);

                Assert.NotNull(lockedPersona);
                Assert.False(lockedPersona.CanAuthorize());
            }

            // Consultas paginadas equivalentes a MockPersonaEndpoints (GET /api/v1/identity/mock/personas).
            await using (var listQueryScope = harness.CreateScope())
            {
                var dbContext = listQueryScope.ServiceProvider.GetRequiredService<G1IdentityDbContext>();

                var total = await dbContext.MockPersonas
                    .AsNoTracking()
                    .CountAsync(cancellationToken);

                var fixtureVersion = await dbContext.MockPersonas
                    .AsNoTracking()
                    .OrderBy(persona => persona.Key)
                    .Select(persona => persona.FixtureVersion)
                    .FirstOrDefaultAsync(cancellationToken);

                var secondPagePersonas = await dbContext.MockPersonas
                    .AsNoTracking()
                    .OrderBy(persona => persona.Key)
                    .Skip(5)
                    .Take(5)
                    .ToListAsync(cancellationToken);

                var pageUserIds = secondPagePersonas
                    .Select(persona => persona.UserId)
                    .ToList();

                var pageActiveAssignments = await dbContext.RoleAssignments
                    .AsNoTracking()
                    .Where(assignment => pageUserIds.Contains(assignment.UserId) && assignment.RevokedAt == null)
                    .ToListAsync(cancellationToken);

                var pageRoleCodes = pageActiveAssignments
                    .Select(assignment => assignment.RoleCode)
                    .Distinct(StringComparer.Ordinal)
                    .ToList();

                var pageGrantedPermissions = await dbContext.RolePermissions
                    .AsNoTracking()
                    .Where(rolePermission => rolePermission.Granted && pageRoleCodes.Contains(rolePermission.RoleCode))
                    .ToListAsync(cancellationToken);

                Assert.Equal(15, total);
                Assert.Equal(ExpectedFixtureVersion, fixtureVersion);
                Assert.Equal(5, secondPagePersonas.Count);
                Assert.NotEmpty(pageActiveAssignments);
                Assert.NotNull(pageGrantedPermissions);
            }

            // Borrado completo de estado sin operaciones masivas equivalente a POST /api/v1/identity/mock/reset.
            await using (var resetScope = harness.CreateScope())
            {
                var dbContext = resetScope.ServiceProvider.GetRequiredService<G1IdentityDbContext>();

                var sessionTokens = await dbContext.SessionTokens.ToListAsync(cancellationToken);
                var sessions = await dbContext.Sessions.ToListAsync(cancellationToken);
                var auditEvents = await dbContext.AuditEvents.ToListAsync(cancellationToken);
                var openIddictTokens = await dbContext.Set<OpenIddictEntityFrameworkCoreToken>().ToListAsync(cancellationToken);
                var openIddictAuthorizations = await dbContext.Set<OpenIddictEntityFrameworkCoreAuthorization>().ToListAsync(cancellationToken);
                var roleAssignments = await dbContext.RoleAssignments.ToListAsync(cancellationToken);
                var rolePermissions = await dbContext.RolePermissions.ToListAsync(cancellationToken);
                var personas = await dbContext.MockPersonas.ToListAsync(cancellationToken);
                var roles = await dbContext.Roles.ToListAsync(cancellationToken);
                var permissions = await dbContext.Permissions.ToListAsync(cancellationToken);
                var catalogSnapshots = await dbContext.CatalogSnapshots.ToListAsync(cancellationToken);

                dbContext.SessionTokens.RemoveRange(sessionTokens);
                dbContext.Sessions.RemoveRange(sessions);
                dbContext.AuditEvents.RemoveRange(auditEvents);
                dbContext.Set<OpenIddictEntityFrameworkCoreToken>().RemoveRange(openIddictTokens);
                dbContext.Set<OpenIddictEntityFrameworkCoreAuthorization>().RemoveRange(openIddictAuthorizations);
                dbContext.RoleAssignments.RemoveRange(roleAssignments);
                dbContext.RolePermissions.RemoveRange(rolePermissions);
                dbContext.MockPersonas.RemoveRange(personas);
                dbContext.Roles.RemoveRange(roles);
                dbContext.Permissions.RemoveRange(permissions);
                dbContext.CatalogSnapshots.RemoveRange(catalogSnapshots);

                await dbContext.SaveChangesAsync(cancellationToken);
                dbContext.ChangeTracker.Clear();

                await SeedData.EnsureSeedDataAsync(dbContext, cancellationToken);
            }

            await using (var verifyScope = harness.CreateScope())
            {
                var dbContext = verifyScope.ServiceProvider.GetRequiredService<G1IdentityDbContext>();

                Assert.Equal(0, await dbContext.Sessions.CountAsync(cancellationToken));
                Assert.Equal(0, await dbContext.SessionTokens.CountAsync(cancellationToken));
                Assert.Equal(0, await dbContext.AuditEvents.CountAsync(cancellationToken));
                Assert.Equal(0, await dbContext.CatalogSnapshots.CountAsync(cancellationToken));
                Assert.Equal(0, await dbContext.Set<OpenIddictEntityFrameworkCoreToken>().CountAsync(cancellationToken));
                Assert.Equal(0, await dbContext.Set<OpenIddictEntityFrameworkCoreAuthorization>().CountAsync(cancellationToken));
                Assert.Equal(15, await dbContext.MockPersonas.CountAsync(cancellationToken));
                Assert.Equal(10, await dbContext.Roles.CountAsync(cancellationToken));
                Assert.Equal(3, await dbContext.Permissions.CountAsync(cancellationToken));
                Assert.Equal(6, await dbContext.RolePermissions.CountAsync(cancellationToken));
                Assert.Equal(14, await dbContext.RoleAssignments.CountAsync(cancellationToken));
            }
        }

        /// <summary>
        /// Verifica que los stores de OpenIddict EF Core con <c>DisableBulkOperations()</c>
        /// funcionan sobre SQLite al crear aplicación, autorización y token, revocar por sujeto
        /// (dejando el token en estado <c>revoked</c>) y eliminar la aplicación.
        /// </summary>
        [Fact]
        public async Task OpenIddictEntityFrameworkCoreStoresWorkWithDisableBulkOperationsInSqliteAsync()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var harness = await SqliteTestHarness.CreateAsync(cancellationToken);

            const string clientId = @"cli_sqlite_smoke_spa";
            const string subject = @"usr_admin_club";
            string applicationId;
            string authorizationId;
            string tokenId;

            await using (var createScope = harness.CreateScope())
            {
                var applicationManager = createScope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
                var authorizationManager = createScope.ServiceProvider.GetRequiredService<IOpenIddictAuthorizationManager>();
                var tokenManager = createScope.ServiceProvider.GetRequiredService<IOpenIddictTokenManager>();

                var applicationDescriptor = new OpenIddictApplicationDescriptor
                {
                    ClientId = clientId,
                    DisplayName = @"Cliente SPA de compatibilidad SQLite",
                    ClientType = OpenIddictConstants.ClientTypes.Public,
                    ConsentType = OpenIddictConstants.ConsentTypes.Implicit,
                    RedirectUris =
                    {
                        new Uri(@"https://localhost:5173/auth/callback"),
                    },
                    Permissions =
                    {
                        OpenIddictConstants.Permissions.Endpoints.Authorization,
                        OpenIddictConstants.Permissions.Endpoints.Token,
                        OpenIddictConstants.Permissions.Endpoints.Revocation,
                        OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                        OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                        OpenIddictConstants.Permissions.ResponseTypes.Code,
                        OpenIddictConstants.Permissions.Scopes.Profile,
                        OpenIddictConstants.Permissions.Scopes.Email,
                        OpenIddictConstants.Permissions.Prefixes.Scope + @"offline_access",
                        OpenIddictConstants.Permissions.Prefixes.Scope + @"identity.api",
                    },
                    Requirements =
                    {
                        OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange,
                    },
                };

                var application = await applicationManager.CreateAsync(applicationDescriptor, cancellationToken);
                applicationId = await applicationManager.GetIdAsync(application, cancellationToken) ?? String.Empty;
                Assert.False(String.IsNullOrWhiteSpace(applicationId));

                var authorizationDescriptor = new OpenIddictAuthorizationDescriptor
                {
                    ApplicationId = applicationId,
                    Subject = subject,
                    Status = OpenIddictConstants.Statuses.Valid,
                    Type = OpenIddictConstants.AuthorizationTypes.Permanent,
                    CreationDate = DateTimeOffset.UtcNow,
                    Scopes =
                    {
                        OpenIddictConstants.Scopes.OpenId,
                        OpenIddictConstants.Scopes.Profile,
                        OpenIddictConstants.Scopes.Email,
                        OpenIddictConstants.Scopes.OfflineAccess,
                        @"identity.api",
                    },
                };

                var authorization = await authorizationManager.CreateAsync(authorizationDescriptor, cancellationToken);
                authorizationId = await authorizationManager.GetIdAsync(authorization, cancellationToken) ?? String.Empty;
                Assert.False(String.IsNullOrWhiteSpace(authorizationId));

                var nowUtc = DateTimeOffset.UtcNow;
                var tokenDescriptor = new OpenIddictTokenDescriptor
                {
                    ApplicationId = applicationId,
                    AuthorizationId = authorizationId,
                    Subject = subject,
                    Type = OpenIddictConstants.TokenTypeHints.RefreshToken,
                    Status = OpenIddictConstants.Statuses.Valid,
                    ReferenceId = @"ref_sqlite_smoke_001",
                    CreationDate = nowUtc,
                    ExpirationDate = nowUtc.AddHours(8),
                };

                var token = await tokenManager.CreateAsync(tokenDescriptor, cancellationToken);
                tokenId = await tokenManager.GetIdAsync(token, cancellationToken) ?? String.Empty;
                Assert.False(String.IsNullOrWhiteSpace(tokenId));
            }

            // Revocar por sujeto con DisableBulkOperations() y verificar estado revoked en SQLite.
            await using (var revokeScope = harness.CreateScope())
            {
                var tokenManager = revokeScope.ServiceProvider.GetRequiredService<IOpenIddictTokenManager>();

                var revokedCount = await tokenManager.RevokeBySubjectAsync(subject, cancellationToken);
                Assert.Equal(1L, revokedCount);
            }

            await using (var verifyRevocationScope = harness.CreateScope())
            {
                var tokenManager = verifyRevocationScope.ServiceProvider.GetRequiredService<IOpenIddictTokenManager>();

                var persistedToken = await tokenManager.FindByIdAsync(tokenId, cancellationToken);
                Assert.NotNull(persistedToken);

                var status = await tokenManager.GetStatusAsync(persistedToken, cancellationToken);
                Assert.Equal(OpenIddictConstants.Statuses.Revoked, status);
            }

            // Borrar la aplicación con DisableBulkOperations() y confirmar eliminación limpia.
            await using (var deleteScope = harness.CreateScope())
            {
                var applicationManager = deleteScope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

                var persistedApplication = await applicationManager.FindByClientIdAsync(clientId, cancellationToken);
                Assert.NotNull(persistedApplication);

                await applicationManager.DeleteAsync(persistedApplication, cancellationToken);
            }

            await using (var verifyDeletionScope = harness.CreateScope())
            {
                var applicationManager = verifyDeletionScope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
                var authorizationManager = verifyDeletionScope.ServiceProvider.GetRequiredService<IOpenIddictAuthorizationManager>();
                var tokenManager = verifyDeletionScope.ServiceProvider.GetRequiredService<IOpenIddictTokenManager>();

                Assert.Null(await applicationManager.FindByClientIdAsync(clientId, cancellationToken));
                Assert.Null(await authorizationManager.FindByIdAsync(authorizationId, cancellationToken));
                Assert.Null(await tokenManager.FindByIdAsync(tokenId, cancellationToken));
            }
        }

        private static async Task AssertDuplicateMockPersonaRejectedAsync(
            SqliteTestHarness harness,
            MockPersona duplicateCandidate,
            CancellationToken cancellationToken)
        {
            await using var scope = harness.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<G1IdentityDbContext>();

            dbContext.MockPersonas.Add(duplicateCandidate);

            var exception = await Assert.ThrowsAsync<DbUpdateException>(
                () => dbContext.SaveChangesAsync(cancellationToken));

            Assert.IsType<SqliteException>(exception.InnerException);
        }

        /// <summary>
        /// Entorno aislado con una conexión SQLite en memoria abierta durante la vida de cada prueba
        /// y cerrada de forma determinista mediante <see cref="DisposeAsync"/>.
        /// </summary>
        private sealed class SqliteTestHarness : IAsyncDisposable
        {
            private readonly SqliteConnection _connection;
            private readonly ServiceProvider _serviceProvider;

            private SqliteTestHarness(SqliteConnection connection, ServiceProvider serviceProvider)
            {
                _connection = connection;
                _serviceProvider = serviceProvider;
            }

            /// <summary>
            /// Abre una conexión SQLite en memoria, registra <see cref="G1IdentityDbContext"/> con OpenIddict
            /// (<c>DisableBulkOperations()</c>) y crea el esquema completo mediante <c>EnsureCreatedAsync</c>.
            /// </summary>
            /// <param name="cancellationToken">Token de cancelación.</param>
            /// <returns>Una instancia inicializada de <see cref="SqliteTestHarness"/>.</returns>
            internal static async Task<SqliteTestHarness> CreateAsync(CancellationToken cancellationToken)
            {
                var connection = new SqliteConnection(InMemorySqliteConnectionString);
                await connection.OpenAsync(cancellationToken);

                var services = new ServiceCollection();
                services.AddLogging();
                services.AddDbContext<G1IdentityDbContext>(options =>
                {
                    options.UseSqlite(connection);
                    options.UseOpenIddict();
                });

                services.AddOpenIddict()
                    .AddCore(options =>
                    {
                        options.UseEntityFrameworkCore()
                            .UseDbContext<G1IdentityDbContext>()
                            .DisableBulkOperations();
                    });

                var serviceProvider = services.BuildServiceProvider();

                try
                {
                    await using var initScope = serviceProvider.CreateAsyncScope();
                    var dbContext = initScope.ServiceProvider.GetRequiredService<G1IdentityDbContext>();
                    await dbContext.Database.EnsureCreatedAsync(cancellationToken);
                }
                catch
                {
                    await serviceProvider.DisposeAsync();
                    await connection.DisposeAsync();
                    throw;
                }

                return new SqliteTestHarness(connection, serviceProvider);
            }

            /// <summary>
            /// Crea un nuevo ámbito de servicios asíncrono sobre la misma conexión SQLite abierta.
            /// </summary>
            /// <returns>El ámbito de servicios creado.</returns>
            internal AsyncServiceScope CreateScope()
            {
                return _serviceProvider.CreateAsyncScope();
            }

            /// <summary>
            /// Consulta los nombres de las tablas creadas en el catálogo <c>sqlite_master</c>.
            /// </summary>
            /// <param name="cancellationToken">Token de cancelación.</param>
            /// <returns>Conjunto con los nombres de las tablas de usuario existentes en SQLite.</returns>
            internal async Task<HashSet<string>> GetCreatedTableNamesAsync(CancellationToken cancellationToken)
            {
                var tables = new HashSet<string>(StringComparer.Ordinal);

                await using var command = _connection.CreateCommand();
                command.CommandText = @"SELECT name FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%';";

                if (_connection.State != ConnectionState.Open)
                {
                    await _connection.OpenAsync(cancellationToken);
                }

                await using var reader = await command.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    tables.Add(reader.GetString(0));
                }

                return tables;
            }

            /// <inheritdoc />
            public async ValueTask DisposeAsync()
            {
                await _serviceProvider.DisposeAsync();
                await _connection.CloseAsync();
                await _connection.DisposeAsync();
            }
        }
    }
}
