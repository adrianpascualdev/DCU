using G1.Identity.Mock.Settings;
using OpenIddict.Abstractions;
using OpenIddict.Server;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Configura los componentes independientes de OpenIddict para el mock de identidad.
    /// </summary>
    public static class OpenIddictConfiguration
    {
        /// <summary>
        /// Registra el servidor OpenIddict con el flujo y los valores temporales aprobados para la fase A.
        /// </summary>
        /// <param name="services">Colección de servicios de la aplicación.</param>
        /// <param name="options">Opciones de identidad previamente validadas.</param>
        /// <returns>La colección de servicios configurada.</returns>
        /// <exception cref="ArgumentNullException">
        /// Se produce cuando <paramref name="services"/> u <paramref name="options"/> son nulos.
        /// </exception>
        public static IServiceCollection ConfigureOpenIddict(
            this IServiceCollection services,
            MockOptions options)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(options);

            services.AddOpenIddict()
                .AddServer(server => server
                    .SetAuthorizationEndpointUris("/connect/authorize")
                    .SetTokenEndpointUris("/connect/token")
                    .SetUserInfoEndpointUris("/connect/userinfo")
                    .SetRevocationEndpointUris("/connect/revoke")
                    .SetEndSessionEndpointUris("/connect/logout")
                    .AllowAuthorizationCodeFlow()
                    .AllowRefreshTokenFlow()
                    .RequireProofKeyForCodeExchange()
                    .Configure(options =>
                        options.CodeChallengeMethods.Clear())
                    .Configure(options =>
                        options.CodeChallengeMethods.Add(OpenIddictConstants.CodeChallengeMethods.Sha256))
                    .AddEphemeralEncryptionKey()
                    .AddEphemeralSigningKey()
                    .DisableAccessTokenEncryption()
                    .UseAspNetCore());

            services.PostConfigure<OpenIddictServerOptions>(server =>
            {
                server.Issuer = options.Issuer;
                server.Scopes.UnionWith(options.Scopes);
                server.AuthorizationCodeLifetime = options.Lifetimes.AuthorizationCode;
                server.AccessTokenLifetime = options.Lifetimes.AccessToken;
                server.IdentityTokenLifetime = options.Lifetimes.IdentityToken;
                server.RefreshTokenLifetime = options.Lifetimes.RefreshToken;
            });

            return services;
        }
    }
}
