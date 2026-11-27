using System.Security.Claims;
using System.Collections.Immutable;
using G1.Identity.Mock.Persistence;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace G1.Identity.Mock.Auth
{
    /// <summary>
    /// Construye la fotografía de claims de una persona que OpenIddict firma al emitir tokens.
    /// </summary>
    internal static class ClaimsTransformation
    {
        /// <summary>
        /// Crea una identidad OIDC a partir de la persona persistida y sus asignaciones vigentes.
        /// </summary>
        /// <param name="context">Contexto de persistencia de identidad.</param>
        /// <param name="userId">Identificador opaco de la persona.</param>
        /// <param name="sessionId">Identificador de la sesión creada para la autorización.</param>
        /// <param name="authenticationMethods">Métodos de autenticación de la sesión.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Principal OIDC con los claims contractuales.</returns>
        internal static async Task<ClaimsPrincipal?> CreateAsync(
            G1IdentityDbContext context,
            string userId,
            string sessionId,
            IReadOnlyList<string> authenticationMethods,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentException.ThrowIfNullOrWhiteSpace(userId);
            ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);
            ArgumentNullException.ThrowIfNull(authenticationMethods);

            var persona = await context.MockPersonas
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.UserId == userId, cancellationToken);
            if (persona is null || !persona.CanAuthorize())
            {
                return null;
            }

            var roles = await context.RoleAssignments
                .AsNoTracking()
                .Where(assignment => assignment.UserId == userId && assignment.RevokedAt == null)
                .Select(assignment => assignment.RoleCode)
                .OrderBy(code => code)
                .ToArrayAsync(cancellationToken);

            var permissions = await context.RolePermissions
                .AsNoTracking()
                .Where(permission => permission.Granted && roles.Contains(permission.RoleCode))
                .Select(permission => permission.PermissionCode)
                .Distinct()
                .OrderBy(code => code)
                .ToArrayAsync(cancellationToken);

            var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            identity.SetClaim(OpenIddictConstants.Claims.Subject, persona.UserId);
            identity.SetClaim(OpenIddictConstants.Claims.Name, persona.DisplayName);
            identity.SetClaim(OpenIddictConstants.Claims.Email, persona.Email);
            identity.SetClaim(OpenIddictConstants.Claims.EmailVerified, persona.EmailVerified.ToString().ToLowerInvariant());
            identity.SetClaim(OpenIddictConstants.Claims.PhoneNumberVerified, persona.PhoneVerified.ToString().ToLowerInvariant());
            identity.SetClaim(Constants.Claims.SessionId, sessionId);
            identity.SetClaims(OpenIddictConstants.Claims.Role, roles.ToImmutableArray());
            identity.SetClaims(Constants.Claims.Permissions, permissions.ToImmutableArray());
            identity.SetClaims(OpenIddictConstants.Claims.AuthenticationMethodReference, authenticationMethods.ToImmutableArray());

            var principal = new ClaimsPrincipal(identity);
            principal.SetDestinations(static claim => claim.Type switch
            {
                OpenIddictConstants.Claims.Name or OpenIddictConstants.Claims.Email or OpenIddictConstants.Claims.EmailVerified
                    => [OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken],
                Constants.Claims.SessionId or OpenIddictConstants.Claims.Role or Constants.Claims.Permissions
                    or OpenIddictConstants.Claims.PhoneNumberVerified
                    => [OpenIddictConstants.Destinations.AccessToken],
                OpenIddictConstants.Claims.AuthenticationMethodReference
                    => [OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken],
                _ => [OpenIddictConstants.Destinations.AccessToken],
            });

            return principal;
        }
    }
}
