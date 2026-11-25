using G1.Identity.Mock.Settings;
using OpenIddict.Abstractions;

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

            var lifetimes = options.Lifetimes;

            services.AddOpenIddict()
                .AddServer(server => server
                    .SetIssuer(options.Issuer)
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
                    .RegisterScopes(options.Scopes.ToArray())
                    .SetAuthorizationCodeLifetime(lifetimes.AuthorizationCode)
                    .SetAccessTokenLifetime(lifetimes.AccessToken)
                    .SetIdentityTokenLifetime(lifetimes.IdentityToken)
                    .SetRefreshTokenLifetime(lifetimes.RefreshToken)
                    .AddEphemeralEncryptionKey()
                    .AddEphemeralSigningKey()
                    .DisableAccessTokenEncryption()
                    .UseAspNetCore());

            return services;
        }
    }
}
