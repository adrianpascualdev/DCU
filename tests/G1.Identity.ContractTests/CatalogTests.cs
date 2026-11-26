using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.RateLimiting;

using G1.Identity.Mock;
using G1.Identity.Mock.Auth;
using G1.Identity.Mock.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;

namespace G1.Identity.ContractTests
{
    /// <summary>
    /// Verifica los catálogos versionados de roles y permisos de G1.
    /// </summary>
    public sealed class CatalogTests
    {
        [Fact]
        public async Task RoleCatalogPublishesTheTenSharedCodesAndSupportsEtag()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var host = await CreateHostAsync(cancellationToken);

            using var response = await host.Client.GetAsync(Constants.Paths.RoleCatalog, cancellationToken);

            await ContractResponseAssertions.MatchesAsync(response, Constants.Paths.RoleCatalog, @"get", HttpStatusCode.OK, cancellationToken);
            var etag = Assert.Single(response.Headers.GetValues(Constants.Headers.ETag));
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            Assert.Equal(Constants.Catalog.Version, document.RootElement.GetProperty(@"catalogVersion").GetString());
            Assert.True(document.RootElement.GetProperty(@"provisional").GetBoolean());

            var codes = document.RootElement.GetProperty(@"items")
                .EnumerateArray()
                .Select(item => item.GetProperty(@"code").GetString())
                .ToArray();
            Assert.Equal(10, codes.Length);
            Assert.Contains(@"sports_manager", codes);
            Assert.Contains(@"communications_staff", codes);

            using var conditional = new HttpRequestMessage(HttpMethod.Get, Constants.Paths.RoleCatalog);
            conditional.Headers.TryAddWithoutValidation(Constants.Headers.IfNoneMatch, etag);
            using var notModified = await host.Client.SendAsync(conditional, cancellationToken);
            Assert.Equal(HttpStatusCode.NotModified, notModified.StatusCode);
            Assert.Equal(etag, Assert.Single(notModified.Headers.GetValues(Constants.Headers.ETag)));
        }

        [Fact]
        public async Task PermissionCatalogPublishesOnlyTheThreePermissionsOwnedByG1()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var host = await CreateHostAsync(cancellationToken);

            using var response = await host.Client.GetAsync(Constants.Paths.PermissionCatalog, cancellationToken);

            await ContractResponseAssertions.MatchesAsync(response, Constants.Paths.PermissionCatalog, @"get", HttpStatusCode.OK, cancellationToken);
            Assert.Equal(Constants.Catalog.PermissionsEntityTag, Assert.Single(response.Headers.GetValues(Constants.Headers.ETag)));
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var permissions = document.RootElement.GetProperty(@"items").EnumerateArray().ToArray();
            Assert.Equal(3, permissions.Length);
            Assert.All(
                permissions,
                permission => Assert.Equal(Constants.Catalog.OwnerModule, permission.GetProperty(@"ownerModule").GetString()));
            Assert.Contains(
                permissions,
                permission => permission.GetProperty(@"code").GetString() == Constants.Permissions.IdentityAdmin);
        }

        [Theory]
        [InlineData(Constants.Paths.RoleCatalog)]
        [InlineData(Constants.Paths.PermissionCatalog)]
        public async Task CatalogRequiresAuthenticationWithDocumentedProblem(string path)
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var host = await CreateHostAsync(cancellationToken, authenticated: false);

            using var response = await host.Client.GetAsync(path, cancellationToken);

            await ContractResponseAssertions.MatchesAsync(response, path, @"get", HttpStatusCode.Unauthorized, cancellationToken);
            Assert.StartsWith(Constants.ContentTypes.ProblemJson, response.Content.Headers.ContentType?.MediaType);
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            Assert.Equal(Constants.ErrorCodes.SessionRequired, document.RootElement.GetProperty(@"code").GetString());
            Assert.Equal(401, document.RootElement.GetProperty(@"status").GetInt32());
            Assert.Equal(path, document.RootElement.GetProperty(@"instance").GetString());
            Assert.False(string.IsNullOrWhiteSpace(document.RootElement.GetProperty(@"traceId").GetString()));
        }

        private static async Task<TestHost> CreateHostAsync(CancellationToken cancellationToken, bool authenticated = true)
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                Args = Array.Empty<string>(),
                ContentRootPath = AppContext.BaseDirectory,
                EnvironmentName = @"Production",
            });
            builder.WebHost.UseUrls(@"http://127.0.0.1:0");
            builder.Services.AddAuthentication();
            builder.Services.AddAuthorization(PermissionPolicies.Configure);
            PermissionPolicies.RegisterResultHandler(builder.Services);
            builder.Services.AddRateLimiter(
                options => options.AddFixedWindowLimiter(
                    Constants.Policies.Summary,
                    limiter =>
                    {
                        limiter.PermitLimit = Constants.RateLimits.SummaryPermitLimit;
                        limiter.Window = TimeSpan.FromSeconds(Constants.RateLimits.WindowSeconds);
                        limiter.QueueLimit = 0;
                        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    }));

            var app = builder.Build();
            app.Use(
                async (context, next) =>
                {
                    if (authenticated)
                    {
                        context.User = new ClaimsPrincipal(new ClaimsIdentity(authenticationType: @"test"));
                    }
                    await next(context);
                });
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseRateLimiter();
            new CatalogEndpointDefinition().RegisterEndpoints(app);
            await app.StartAsync(cancellationToken);

            return new TestHost(
                app,
                new HttpClient { BaseAddress = new Uri(app.Urls.Single()) },
                cancellationToken);
        }

        private sealed class TestHost : IAsyncDisposable
        {
            private readonly WebApplication _app;
            private readonly CancellationToken _cancellationToken;

            internal TestHost(WebApplication app, HttpClient client, CancellationToken cancellationToken)
            {
                _app = app;
                _cancellationToken = cancellationToken;
                Client = client;
            }

            internal HttpClient Client { get; }

            public async ValueTask DisposeAsync()
            {
                Client.Dispose();
                await _app.StopAsync(_cancellationToken);
                await _app.DisposeAsync();
            }
        }
    }
}
