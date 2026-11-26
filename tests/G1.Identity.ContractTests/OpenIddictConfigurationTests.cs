using G1.Identity.Mock.Settings;
using G1.Identity.Mock.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using System.Text.Json;

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

        /// <summary>
        /// Publica el descubrimiento y las claves públicas de firma sin requerir una persona autenticada.
        /// </summary>
        [Fact]
        public async Task DiscoveryAndJwksAreAvailableWithoutPersonaAuthentication()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                Args = [],
                ContentRootPath = AppContext.BaseDirectory,
                EnvironmentName = "Testing",
            });
            builder.Configuration.AddJsonFile(
                Path.Combine(AppContext.BaseDirectory, "appsettings.mock.json"),
                optional: false);
            builder.WebHost.UseUrls("https://127.0.0.1:0");
            builder.Configure();

            var app = builder.Build();
            await app.UseMiddlewaresAsync(cancellationToken);
            await app.StartAsync(cancellationToken);

            try
            {
                using var client = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };
                using var discovery = await client.GetAsync("/.well-known/openid-configuration", cancellationToken);

                Assert.True(discovery.IsSuccessStatusCode);

                var discoveryJson = await discovery.Content.ReadAsStringAsync(cancellationToken);
                using var discoveryDocument = JsonDocument.Parse(discoveryJson);
                var jwksUri = discoveryDocument.RootElement
                    .GetProperty("jwks_uri")
                    .GetString();
                Assert.False(string.IsNullOrWhiteSpace(jwksUri));

                using var jwks = await client.GetAsync(jwksUri, cancellationToken);
                Assert.True(jwks.IsSuccessStatusCode);

                var jwksJson = await jwks.Content.ReadAsStringAsync(cancellationToken);
                Assert.Contains("\"authorization_code\"", discoveryJson, StringComparison.Ordinal);
                Assert.Contains("\"refresh_token\"", discoveryJson, StringComparison.Ordinal);
                Assert.Contains("\"S256\"", discoveryJson, StringComparison.Ordinal);
                Assert.DoesNotContain("\"implicit\"", discoveryJson, StringComparison.Ordinal);
                Assert.DoesNotContain("\"password\"", discoveryJson, StringComparison.Ordinal);
                Assert.DoesNotContain("\"client_credentials\"", discoveryJson, StringComparison.Ordinal);

                using var jwksDocument = JsonDocument.Parse(jwksJson);
                Assert.Contains(
                    jwksDocument.RootElement.GetProperty("keys").EnumerateArray(),
                    key => string.Equals(key.GetProperty("kty").GetString(), "RSA", StringComparison.Ordinal));
            }
            finally
            {
                await app.StopAsync(cancellationToken);
                await app.DisposeAsync();
            }
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
