using System.Security.Claims;
using System.Text.Json;

using G1.Identity.Mock.Endpoints;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace G1.Identity.Mock.Auth
{
    /// <summary>
    /// Centraliza las políticas de permisos de G1 y aplica denegación por defecto.
    /// </summary>
    internal static class PermissionPolicies
    {
        private static readonly HashSet<string> KnownPermissions = new(StringComparer.Ordinal)
        {
            Constants.Permissions.UserSummaryRead,
            Constants.Permissions.UserLookup,
            Constants.Permissions.IdentityAdmin,
        };

        /// <summary>
        /// Registra una política por cada permiso propiedad de G1.
        /// </summary>
        /// <param name="options">Opciones de autorización del host.</param>
        internal static void Configure(AuthorizationOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            foreach (var permission in KnownPermissions)
            {
                options.AddPolicy(
                    permission,
                    policy => policy
                        .RequireAuthenticatedUser()
                        .RequireAssertion(context => IsGranted(context.User, permission)));
            }
        }

        /// <summary>
        /// Determina si una identidad posee un permiso conocido de G1.
        /// </summary>
        /// <param name="principal">Identidad autenticada que se evalúa.</param>
        /// <param name="requiredPermission">Código de permiso solicitado.</param>
        /// <returns>
        /// <see langword="true"/> solo cuando el permiso es conocido y está concedido explícitamente.
        /// </returns>
        internal static bool IsGranted(ClaimsPrincipal principal, string requiredPermission)
        {
            ArgumentNullException.ThrowIfNull(principal);

            if (!KnownPermissions.Contains(requiredPermission))
            {
                return false;
            }

            var explicitPermissions = principal.FindAll(Constants.Claims.Permissions)
                .SelectMany(GetClaimValues)
                .ToHashSet(StringComparer.Ordinal);
            if (explicitPermissions.Contains(requiredPermission))
            {
                return true;
            }

            var roles = principal.FindAll(Constants.Claims.Roles).SelectMany(GetClaimValues);
            return ResolvePermissions(roles).Contains(requiredPermission, StringComparer.Ordinal);
        }

        /// <summary>
        /// Resuelve la matriz D-002 con denegación por defecto y sin interpretar ámbitos de negocio.
        /// </summary>
        /// <param name="roles">Códigos de rol firmados por G1.</param>
        /// <returns>Permisos efectivos propiedad de G1.</returns>
        internal static IReadOnlySet<string> ResolvePermissions(IEnumerable<string> roles)
        {
            ArgumentNullException.ThrowIfNull(roles);
            var permissions = new HashSet<string>(StringComparer.Ordinal);

            foreach (var role in roles)
            {
                switch (role)
                {
                    case Constants.Roles.ClubAdmin:
                        permissions.Add(Constants.Permissions.UserSummaryRead);
                        permissions.Add(Constants.Permissions.UserLookup);
                        permissions.Add(Constants.Permissions.IdentityAdmin);
                        break;
                    case Constants.Roles.SportsManager:
                        permissions.Add(Constants.Permissions.UserSummaryRead);
                        permissions.Add(Constants.Permissions.UserLookup);
                        break;
                    case Constants.Roles.CommunicationsStaff:
                        permissions.Add(Constants.Permissions.UserLookup);
                        break;
                }
            }

            return permissions;
        }

        /// <summary>
        /// Crea el adaptador que traduce los rechazos de autorización al contrato de errores de G1.
        /// </summary>
        /// <returns>Manejador de resultados de autorización.</returns>
        internal static IAuthorizationMiddlewareResultHandler CreateResultHandler()
        {
            return new ProblemDetailsAuthorizationResultHandler();
        }

        /// <summary>
        /// Registra el traductor de rechazos de autorización en el contenedor.
        /// </summary>
        /// <param name="services">Colección de servicios del host.</param>
        internal static void RegisterResultHandler(IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);
            services.AddSingleton<IAuthorizationMiddlewareResultHandler>(
                _ => CreateResultHandler());
        }

        private sealed class ProblemDetailsAuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
        {
            public Task HandleAsync(
                RequestDelegate next,
                HttpContext context,
                AuthorizationPolicy policy,
                PolicyAuthorizationResult authorizeResult)
            {
                if (authorizeResult.Succeeded)
                {
                    return next(context);
                }

                var authenticationFailure = context.Features
                    .Get<IAuthenticateResultFeature>()?
                    .AuthenticateResult?
                    .Failure;
                var errorCode = authorizeResult.Forbidden
                    ? Constants.ErrorCodes.PermissionDenied
                    : authenticationFailure is null
                        ? Constants.ErrorCodes.SessionRequired
                        : Constants.ErrorCodes.SessionInvalid;

                return ErrorMapping.WriteProblemAsync(context, errorCode, context.RequestAborted);
            }
        }

        private static IEnumerable<string> GetClaimValues(Claim claim)
        {
            if (!claim.Value.StartsWith(@"[", StringComparison.Ordinal))
            {
                return new[] { claim.Value };
            }

            try
            {
                return JsonSerializer.Deserialize<string[]>(claim.Value) ?? Array.Empty<string>();
            }
            catch (JsonException)
            {
                return Array.Empty<string>();
            }
        }
    }
}
