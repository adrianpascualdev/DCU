using System.Security.Claims;
using G1.Identity.Mock.Abstractions;
using G1.Identity.Mock.Auth;
using G1.Identity.Mock.Extensions;
using G1.Identity.Mock.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.HttpResults;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace G1.Identity.Mock.Endpoints
{
    /// <summary>
    /// Endpoints OIDC delegados a OpenIddict para el mock de identidad.
    /// </summary>
    internal sealed class OidcEndpoints : IEndpointDefinition
    {
        /// <inheritdoc />
        public void RegisterEndpoints(WebApplication app)
        {
            ArgumentNullException.ThrowIfNull(app);

            app.MapMethods("/connect/authorize", [HttpMethods.Get, HttpMethods.Post], AuthorizeAsync)
                .RequireRateLimiting(Constants.Policies.Authorization)
                .WithTags("OIDC");
            app.MapPost("/connect/token", TokenAsync)
                .RequireRateLimiting(Constants.Policies.Token)
                .WithTags("OIDC");
            app.MapMethods("/connect/userinfo", [HttpMethods.Get, HttpMethods.Post], (Delegate)UserInfoAsync)
                .WithTags("OIDC");
            app.MapPost("/connect/revoke", RevokeAsync)
                .WithTags("OIDC");
        }

        private static async Task<IResult> AuthorizeAsync(
            HttpContext context,
            G1IdentityDbContext database,
            ISessionService sessions,
            CancellationToken cancellationToken)
        {
            var authentication = await context.AuthenticateAsync("MockPersona");
            if (!authentication.Succeeded)
            {
                return TypedResults.Challenge(new AuthenticationProperties { RedirectUri = context.Request.PathBase + context.Request.Path + context.Request.QueryString }, ["MockPersona"]);
            }

            var userId = authentication.Principal?.FindFirst(OpenIddictConstants.Claims.Subject)?.Value
                ?? authentication.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return ForbidOpenIddict();
            }

            var sessionId = $"ses_{Guid.NewGuid():N}";
            IReadOnlyList<string> authenticationMethods = ["mock_persona"];
            var principal = await ClaimsTransformation.CreateAsync(
                database,
                userId,
                sessionId,
                authenticationMethods,
                cancellationToken);
            if (principal is null)
            {
                return ForbidOpenIddict();
            }

            sessions.Create(
                sessionId,
                userId,
                context.Request.Query[OpenIddictConstants.Parameters.ClientId].ToString(),
                authenticationMethods,
                "Navegador");
            principal.SetScopes(context.Request.Query[OpenIddictConstants.Parameters.Scope].ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
            principal.SetResources("identity-api");

            return TypedResults.SignIn(principal, authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        private static async Task<IResult> TokenAsync(
            HttpContext context,
            ISessionService sessions,
            CancellationToken cancellationToken)
        {
            var authentication = await context.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            if (!authentication.Succeeded || authentication.Principal is null)
            {
                return ForbidOpenIddict();
            }

            var sessionId = authentication.Principal.FindFirst(Constants.Claims.SessionId)?.Value;
            if (string.IsNullOrWhiteSpace(sessionId) || !sessions.Touch(sessionId))
            {
                return ForbidOpenIddict();
            }

            return TypedResults.SignIn(
                authentication.Principal,
                authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        private static async Task<IResult> UserInfoAsync(HttpContext context)
        {
            var authentication = await context.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            if (!authentication.Succeeded || authentication.Principal is null)
            {
                return ForbidOpenIddict();
            }

            var principal = authentication.Principal;
            var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            CopyClaim(principal, identity, OpenIddictConstants.Claims.Subject);
            CopyClaim(principal, identity, OpenIddictConstants.Claims.Name);
            CopyClaim(principal, identity, OpenIddictConstants.Claims.Email);
            CopyClaim(principal, identity, OpenIddictConstants.Claims.EmailVerified);

            return TypedResults.SignIn(
                new ClaimsPrincipal(identity),
                authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        private static IResult RevokeAsync()
        {
            return TypedResults.Forbid(authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
        }

        private static IResult ForbidOpenIddict()
        {
            return TypedResults.Forbid(authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
        }

        private static void CopyClaim(ClaimsPrincipal source, ClaimsIdentity destination, string type)
        {
            var claim = source.FindFirst(type);
            if (claim is not null)
            {
                destination.AddClaim(claim);
            }
        }
    }
}
