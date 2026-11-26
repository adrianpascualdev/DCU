using G1.Identity.Mock.Fixtures;
using G1.Identity.Mock.Persistence;
using G1.Identity.Mock.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;

namespace G1.Identity.ContractTests
{
    /// <summary>
    /// Comprueba el cliente público de desarrollo requerido por T027.
    /// </summary>
    public sealed class MockClientsTests
    {
        /// <summary>
        /// Registra una única aplicación pública con el flujo y las restricciones aprobadas.
        /// </summary>
        [Fact]
        public async Task InitializeAsyncRegistersTheApprovedPublicClientIdempotently()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            var services = new ServiceCollection();
            services.AddIdentityPersistence("MockClients_" + Guid.NewGuid().ToString("N"));

            using var provider = services.BuildServiceProvider();
            G1.Identity.Mock.Persistence.PersistenceExtensions.InitializePersistence(provider);

            var manager = provider.GetRequiredService<IOpenIddictApplicationManager>();
            await MockClients.InitializeAsync(provider, cancellationToken);
            await MockClients.InitializeAsync(provider, cancellationToken);

            var application = await manager.FindByClientIdAsync(MockClients.ClientId, cancellationToken);

            Assert.NotNull(application);
            Assert.Equal(OpenIddictConstants.ClientTypes.Public, await manager.GetClientTypeAsync(application, cancellationToken));
            Assert.Equal(
                [MockClients.RedirectUri],
                (await manager.GetRedirectUrisAsync(application, cancellationToken)).ToArray());
            Assert.Equal(
                [MockClients.PostLogoutRedirectUri],
                (await manager.GetPostLogoutRedirectUrisAsync(application, cancellationToken)).ToArray());
            Assert.Equal(
                new[]
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
                }.OrderBy(permission => permission),
                (await manager.GetPermissionsAsync(application, cancellationToken)).OrderBy(permission => permission));
            Assert.Equal(
                [OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange],
                (await manager.GetRequirementsAsync(application, cancellationToken)).ToArray());

            var count = 0;
            await foreach (var registeredClient in manager.ListAsync(cancellationToken: cancellationToken))
            {
                count++;
            }

            Assert.Equal(1, count);
        }

        /// <summary>
        /// Registra el cliente al inicializar el host sin invocar manualmente el registrador.
        /// </summary>
        [Fact]
        public async Task HostStartupRegistersThePublicClient()
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
                var manager = app.Services.GetRequiredService<IOpenIddictApplicationManager>();
                var application = await manager.FindByClientIdAsync(MockClients.ClientId, cancellationToken);

                Assert.NotNull(application);
            }
            finally
            {
                await app.StopAsync(cancellationToken);
                await app.DisposeAsync();
            }
        }
    }
}
