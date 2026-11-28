using System.Security.Claims;
using System.Text.Json;

using G1.Identity.Mock;
using G1.Identity.Mock.Auth;
using G1.Identity.Mock.Filters;
using G1.Identity.Mock.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;

namespace G1.Identity.ContractTests
{
    /// <summary>
    /// Verifica la denegación por defecto y los errores estables de autorización.
    /// </summary>
    public sealed class AuthorizationTests
    {
        [Theory]
        [InlineData(Constants.Permissions.UserSummaryRead)]
        [InlineData(Constants.Permissions.UserLookup)]
        [InlineData(Constants.Permissions.IdentityAdmin)]
        public void ExplicitKnownPermissionIsGranted(string permission)
        {
            var principal = CreatePrincipal(permission);

            Assert.True(PermissionPolicies.IsGranted(principal, permission));
        }

        [Theory]
        [InlineData(Constants.Permissions.UserSummaryRead)]
        [InlineData("g2.teams.manage")]
        [InlineData("")]
        public void MissingOrUnknownPermissionIsDenied(string permission)
        {
            var principal = CreatePrincipal(Constants.Permissions.UserLookup);

            Assert.False(PermissionPolicies.IsGranted(principal, permission));
        }

        [Fact]
        public void RoleMatrixGrantsOnlyThePermissionsDeclaredByD002()
        {
            Assert.Equal(
                3,
                PermissionPolicies.ResolvePermissions(new[] { Constants.Roles.ClubAdmin }).Count);
            Assert.Equal(
                new[] { Constants.Permissions.UserLookup, Constants.Permissions.UserSummaryRead },
                PermissionPolicies.ResolvePermissions(new[] { Constants.Roles.SportsManager }).Order());
            Assert.Equal(
                new[] { Constants.Permissions.UserLookup },
                PermissionPolicies.ResolvePermissions(new[] { Constants.Roles.CommunicationsStaff }));
            Assert.Empty(PermissionPolicies.ResolvePermissions(new[] { Constants.Roles.Coach, @"unknown_role" }));
        }

        [Fact]
        public void JsonArrayClaimsAreSupportedWithoutGrantingUnknownCodes()
        {
            var identity = new ClaimsIdentity(
                new[]
                {
                    new Claim(
                        Constants.Claims.Permissions,
                        "[\"identity.users.lookup\",\"unknown.permission\"]"),
                },
                authenticationType: @"test");
            var principal = new ClaimsPrincipal(identity);

            Assert.True(PermissionPolicies.IsGranted(principal, Constants.Permissions.UserLookup));
            Assert.False(PermissionPolicies.IsGranted(principal, @"unknown.permission"));
        }

        [Fact]
        public async Task MissingSessionProducesStableUnauthorizedProblem()
        {
            var problem = await ExecuteAuthorizationFailureAsync(PolicyAuthorizationResult.Challenge());

            Assert.Equal(StatusCodes.Status401Unauthorized, problem.StatusCode);
            Assert.Equal(Constants.ErrorCodes.SessionRequired, problem.Code);
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public async Task RejectedBearerOrSessionCookieProducesSessionInvalid(bool bearer, bool cookie)
        {
            var problem = await ExecuteAuthorizationFailureAsync(
                PolicyAuthorizationResult.Challenge(), bearer, cookie);

            Assert.Equal(StatusCodes.Status401Unauthorized, problem.StatusCode);
            Assert.Equal(Constants.ErrorCodes.SessionInvalid, problem.Code);
        }

        [Fact]
        public async Task MissingPermissionProducesStableForbiddenProblem()
        {
            var problem = await ExecuteAuthorizationFailureAsync(PolicyAuthorizationResult.Forbid());

            Assert.Equal(StatusCodes.Status403Forbidden, problem.StatusCode);
            Assert.Equal(Constants.ErrorCodes.PermissionDenied, problem.Code);
        }

        [Fact]
        public async Task ForbiddenRequestIsAuditedWithoutRequestParametersOrCredentials()
        {
            var audit = new AuditService(TimeProvider.System);
            var principal = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(Constants.Claims.Subject, @"usr_1")], @"test"));

            var problem = await ExecuteAuthorizationFailureAsync(
                PolicyAuthorizationResult.Forbid(), auditService: audit, user: principal);

            Assert.Equal(Constants.ErrorCodes.PermissionDenied, problem.Code);
            var entry = Assert.Single(audit.List());
            Assert.Equal(@"access_denied", entry.EventType);
            Assert.Equal(@"usr_1", entry.ActorUserId);
            Assert.Equal(@"authorization", entry.TargetId);
            Assert.Equal(@"denied", entry.Result);
            Assert.Equal(@"correlacion-autorizacion", entry.CorrelationId);
            Assert.NotEqual(default, entry.OccurredAt);
        }

        [Fact]
        public void BearerHeaderCannotBypassAntiforgeryWhenSessionCookieIsPresent()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Authorization = @"Bearer token-falso";
            context.Request.Headers.Cookie = $@"{Constants.Antiforgery.SessionCookieName}=sesion";

            Assert.True(CookieAntiforgeryFilter.RequiresValidation(context.Request));
        }

        [Fact]
        public void BearerWithoutSessionCookieDoesNotRequireAntiforgery()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Authorization = @"Bearer token";

            Assert.False(CookieAntiforgeryFilter.RequiresValidation(context.Request));
        }

        private static ClaimsPrincipal CreatePrincipal(string permission)
        {
            var identity = new ClaimsIdentity(
                new[] { new Claim(Constants.Claims.Permissions, permission) },
                authenticationType: @"test");
            return new ClaimsPrincipal(identity);
        }

        private static async Task<(int StatusCode, string Code)> ExecuteAuthorizationFailureAsync(
            PolicyAuthorizationResult result, bool bearer = false, bool cookie = false,
            AuditService? auditService = null, ClaimsPrincipal? user = null)
        {
            var context = new DefaultHttpContext();
            context.Request.Path = @"/api/v1/identity/catalog/roles";
            if (user is not null)
            {
                context.User = user;
            }
            if (bearer)
            {
                context.Request.Headers.Authorization = @"Bearer token-invalido";
            }
            if (cookie)
            {
                context.Request.Headers.Cookie = $@"{Constants.Antiforgery.SessionCookieName}=sesion-invalida";
            }
            context.Response.Body = new MemoryStream();
            context.Items[Constants.HttpItems.CorrelationId] = @"correlacion-autorizacion";

            var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
            await PermissionPolicies.CreateResultHandler(auditService).HandleAsync(
                _ => Task.CompletedTask,
                context,
                policy,
                result);

            context.Response.Body.Position = 0;
            using var document = await JsonDocument.ParseAsync(
                context.Response.Body,
                cancellationToken: TestContext.Current.CancellationToken);
            var code = document.RootElement.GetProperty(@"code").GetString()
                ?? throw new InvalidOperationException(@"La respuesta no contiene un código de error.");

            return (context.Response.StatusCode, code);
        }
    }
}
