using G1.Identity.Mock.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Server;

namespace G1.Identity.ContractTests
{
    /// <summary>
    /// Comprueba la configuración independiente de OpenIddict para la tarea T015.
    /// </summary>
    public sealed class OpenIddictConfigurationTests
    {
        /// <summary>
        /// Registra el servidor sin requerir todavía los stores EF ni la autenticación de persona.
        /// </summary>
        [Fact]
        public void ConfigureOpenIddictUsesOnlyApprovedProtocolOptions()
        {
            var services = new ServiceCollection();
            var options = CreateOptions();

            var configured = services.ConfigureOpenIddict(options);

            Assert.Same(services, configured);
            using var provider = services.BuildServiceProvider();
            var server = provider.GetRequiredService<IOptions<OpenIddictServerOptions>>().Value;

            Assert.Equal(options.Issuer, server.Issuer);
            Assert.Contains(new Uri("/connect/authorize", UriKind.Relative), server.AuthorizationEndpointUris);
            Assert.Contains(new Uri("/connect/token", UriKind.Relative), server.TokenEndpointUris);
            Assert.Contains(new Uri("/connect/userinfo", UriKind.Relative), server.UserInfoEndpointUris);
            Assert.Contains(new Uri("/connect/revoke", UriKind.Relative), server.RevocationEndpointUris);
            Assert.Contains(new Uri("/connect/logout", UriKind.Relative), server.EndSessionEndpointUris);
            Assert.Equal(
                [OpenIddictConstants.GrantTypes.AuthorizationCode, OpenIddictConstants.GrantTypes.RefreshToken],
                server.GrantTypes.OrderBy(grantType => grantType));
            Assert.True(server.RequireProofKeyForCodeExchange);
            Assert.Equal([OpenIddictConstants.CodeChallengeMethods.Sha256], server.CodeChallengeMethods);
            Assert.Equal(options.Scopes.OrderBy(scope => scope), server.Scopes.OrderBy(scope => scope));
            Assert.Equal(options.Lifetimes.AuthorizationCode, server.AuthorizationCodeLifetime);
            Assert.Equal(options.Lifetimes.AccessToken, server.AccessTokenLifetime);
            Assert.Equal(options.Lifetimes.IdentityToken, server.IdentityTokenLifetime);
            Assert.Equal(options.Lifetimes.RefreshToken, server.RefreshTokenLifetime);
            Assert.True(server.DisableAccessTokenEncryption);
            Assert.NotEmpty(server.SigningCredentials);
            Assert.NotEmpty(server.EncryptionCredentials);
        }

        /// <summary>
        /// Rechaza opciones nulas para evitar una configuración incompleta del servidor.
        /// </summary>
        [Fact]
        public void ConfigureOpenIddictRejectsNullOptions()
        {
            var services = new ServiceCollection();

            Assert.Throws<ArgumentNullException>(() => services.ConfigureOpenIddict(null!));
        }

        private static MockOptions CreateOptions()
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                Args = [],
                ContentRootPath = AppContext.BaseDirectory,
                EnvironmentName = "Production",
            });
            builder.Configuration.AddJsonFile(
                Path.Combine(AppContext.BaseDirectory, "appsettings.mock.json"),
                optional: false);
            builder.Configure();

            using var provider = builder.Services.BuildServiceProvider();
            return provider.GetRequiredService<IOptions<MockOptions>>().Value;
        }
    }
}
