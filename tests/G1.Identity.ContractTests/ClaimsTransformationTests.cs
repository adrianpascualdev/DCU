using G1.Identity.Mock;
using G1.Identity.Mock.Auth;
using G1.Identity.Mock.Persistence;
using G1.Identity.Mock.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;

namespace G1.Identity.ContractTests
{
    /// <summary>
    /// Verifica que los claims emitibles se calculan desde las entidades persistidas.
    /// </summary>
    public sealed class ClaimsTransformationTests
    {
        /// <summary>
        /// Incluye roles, permisos y verificaciones efectivos en la fotografía de autorización.
        /// </summary>
        [Fact]
        public async Task CreateAsyncBuildsClaimsFromActivePersonaAndEffectiveAssignments()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var context = CreateContext();
            context.MockPersonas.Add(new MockPersona
            {
                Key = "coach.test",
                UserId = "usr_coach_test",
                DisplayName = "Entrenadora de prueba",
                Email = "coach.test@mock.g1.com",
                EmailVerified = true,
                PhoneVerified = false,
                FixtureVersion = "test",
            });
            context.RoleAssignments.AddRange(
                new RoleAssignment
                {
                    AssignmentId = "asg_coach",
                    UserId = "usr_coach_test",
                    RoleCode = Constants.Roles.Coach,
                    GrantedBy = "system",
                    GrantedAt = DateTimeOffset.UtcNow,
                    Reason = "Prueba",
                },
                new RoleAssignment
                {
                    AssignmentId = "asg_revoked",
                    UserId = "usr_coach_test",
                    RoleCode = Constants.Roles.ClubAdmin,
                    GrantedBy = "system",
                    GrantedAt = DateTimeOffset.UtcNow,
                    Reason = "Prueba",
                    RevokedAt = DateTimeOffset.UtcNow,
                });
            context.RolePermissions.AddRange(
                new RolePermission
                {
                    RoleCode = Constants.Roles.Coach,
                    PermissionCode = Constants.Permissions.UserLookup,
                    Granted = true,
                },
                new RolePermission
                {
                    RoleCode = Constants.Roles.ClubAdmin,
                    PermissionCode = Constants.Permissions.IdentityAdmin,
                    Granted = true,
                });
            await context.SaveChangesAsync(cancellationToken);

            var principal = await ClaimsTransformation.CreateAsync(
                context,
                "usr_coach_test",
                "ses_test",
                ["mock_persona"],
                cancellationToken);

            Assert.NotNull(principal);
            Assert.Equal("usr_coach_test", principal.FindFirst(Constants.Claims.Subject)?.Value);
            Assert.Equal("ses_test", principal.FindFirst(Constants.Claims.SessionId)?.Value);
            Assert.Equal("true", principal.FindFirst("email_verified")?.Value);
            Assert.Equal("false", principal.FindFirst("phone_number_verified")?.Value);
            Assert.Equal([Constants.Roles.Coach], principal.FindAll("role").Select(claim => claim.Value));
            Assert.Equal([Constants.Permissions.UserLookup], principal.FindAll(Constants.Claims.Permissions).Select(claim => claim.Value));
        }

        /// <summary>
        /// No permite construir una identidad para una persona inactiva o bloqueada.
        /// </summary>
        [Fact]
        public async Task CreateAsyncRejectsPersonaThatCannotAuthorize()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var context = CreateContext();
            context.MockPersonas.Add(new MockPersona
            {
                Key = "inactive.test",
                UserId = "usr_inactive_test",
                DisplayName = "Persona inactiva",
                Email = "inactive.test@mock.g1.com",
                FixtureVersion = "test",
                Status = PersonaStatus.Inactive,
            });
            await context.SaveChangesAsync(cancellationToken);

            var principal = await ClaimsTransformation.CreateAsync(
                context,
                "usr_inactive_test",
                "ses_test",
                ["mock_persona"],
                cancellationToken);

            Assert.Null(principal);
        }

        /// <summary>
        /// Mantiene los datos de contacto y autorización fuera del ID token cuando el contrato no los permite.
        /// </summary>
        [Fact]
        public async Task CreateAsyncUsesContractualTokenDestinations()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var context = CreateContext();
            context.MockPersonas.Add(new MockPersona
            {
                Key = "member.test",
                UserId = "usr_member_test",
                DisplayName = "Socio de prueba",
                Email = "member.test@mock.g1.com",
                EmailVerified = true,
                PhoneVerified = true,
                FixtureVersion = "test",
            });
            await context.SaveChangesAsync(cancellationToken);

            var principal = await ClaimsTransformation.CreateAsync(
                context,
                "usr_member_test",
                "ses_test",
                ["mock_persona"],
                cancellationToken);

            Assert.NotNull(principal);
            Assert.Equal(
                ["access_token", "id_token"],
                principal.FindFirst("email_verified")!.GetDestinations());
            Assert.Equal(["access_token"], principal.FindFirst(Constants.Claims.SessionId)!.GetDestinations());
            Assert.Equal(["access_token"], principal.FindFirst("phone_number_verified")!.GetDestinations());
        }

        private static G1IdentityDbContext CreateContext()
        {
            return new G1IdentityDbContext(
                new DbContextOptionsBuilder<G1IdentityDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
                    .Options);
        }
    }
}
