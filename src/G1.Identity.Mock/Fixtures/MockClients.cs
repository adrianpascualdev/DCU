using OpenIddict.Abstractions;

namespace G1.Identity.Mock.Fixtures
{
    /// <summary>
    /// Registra los clientes OAuth de desarrollo del mock.
    /// </summary>
    public static class MockClients
    {
        /// <summary>
        /// Identificador del cliente público de desarrollo.
        /// </summary>
        public const string ClientId = "cli_mock_spa";

        /// <summary>
        /// URI de retorno exacta del cliente público de desarrollo.
        /// </summary>
        public const string RedirectUri = "https://localhost:3000/callback";

        /// <summary>
        /// URI de retorno posterior al cierre de sesión del cliente público de desarrollo.
        /// </summary>
        public const string PostLogoutRedirectUri = "https://localhost:3000/";

        /// <summary>
        /// Registra el cliente público de desarrollo si todavía no existe.
        /// </summary>
        /// <param name="serviceProvider">Proveedor de servicios de la aplicación.</param>
        /// <param name="cancellationToken">Token de cancelación de la operación.</param>
        /// <returns>Una tarea que representa la inicialización.</returns>
        /// <exception cref="ArgumentNullException">
        /// Se produce cuando <paramref name="serviceProvider"/> es nulo.
        /// </exception>
        public static async Task InitializeAsync(
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);

            using var scope = serviceProvider.CreateScope();
            var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
            var application = await manager.FindByClientIdAsync(ClientId, cancellationToken);

            if (application is not null)
            {
                return;
            }

            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = ClientId,
                ClientType = OpenIddictConstants.ClientTypes.Public,
                ConsentType = OpenIddictConstants.ConsentTypes.Explicit,
                DisplayName = "Cliente SPA del mock de identidad",
                RedirectUris = { new Uri(RedirectUri, UriKind.Absolute) },
                PostLogoutRedirectUris = { new Uri(PostLogoutRedirectUri, UriKind.Absolute) },
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Authorization,
                    OpenIddictConstants.Permissions.Endpoints.EndSession,
                    OpenIddictConstants.Permissions.Endpoints.Revocation,
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    OpenIddictConstants.Permissions.ResponseTypes.Code,
                    OpenIddictConstants.Permissions.Scopes.Email,
                    OpenIddictConstants.Permissions.Scopes.Profile,
                    OpenIddictConstants.Permissions.Prefixes.Scope + "identity.api",
                },
                Requirements =
                {
                    OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange,
                },
            }, cancellationToken);
        }
    }
}
