using System.Net;
using System.Text.Json.Nodes;
using G1.Identity.Mock;
using G1.Identity.Mock.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace G1.Identity.ContractTests
{
    /// <summary>
    /// Comprueba Swagger UI y el documento publicado con un host Kestrel en proceso.
    /// </summary>
    public sealed class SwaggerAvailabilityTests
    {
        [Theory]
        [InlineData(@"Development")]
        [InlineData(@"Testing")]
        public async Task DocumentationIsAvailableInDevelopmentAndTesting(string environment)
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var host = await CreateHostAsync(environment, cancellationToken);

            using var root = await host.Client.GetAsync(@"/swagger", cancellationToken);
            Assert.Equal(HttpStatusCode.OK, root.StatusCode);
            Assert.EndsWith(
                @"/swagger/index.html",
                root.RequestMessage?.RequestUri?.AbsolutePath ?? string.Empty,
                StringComparison.OrdinalIgnoreCase);

            var html = await root.Content.ReadAsStringAsync(cancellationToken);
            Assert.Contains(@"swagger-ui", html, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(@"swagger-ui-bundle.js", html, StringComparison.Ordinal);
            Assert.Contains(@"index.js", html, StringComparison.Ordinal);

            using var configuration = await host.Client.GetAsync(@"/swagger/index.js", cancellationToken);
            Assert.Equal(HttpStatusCode.OK, configuration.StatusCode);
            Assert.Contains(
                @"/openapi/v1.json",
                await configuration.Content.ReadAsStringAsync(cancellationToken),
                StringComparison.Ordinal);

            var policy = Assert.Single(root.Headers.GetValues(Constants.Headers.ContentSecurityPolicy));
            Assert.Contains(@"script-src 'self' 'unsafe-inline'", policy, StringComparison.Ordinal);
            Assert.Contains(@"style-src 'self' 'unsafe-inline'", policy, StringComparison.Ordinal);
            Assert.Contains(@"frame-ancestors 'none'", policy, StringComparison.Ordinal);
        }

        [Fact]
        public async Task SwaggerUiAssetsAreServedFromTheConfiguredRoute()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var host = await CreateHostAsync(@"Testing", cancellationToken);

            var assets = new[]
            {
                @"/swagger/swagger-ui.css",
                @"/swagger/swagger-ui-bundle.js",
                @"/swagger/swagger-ui-standalone-preset.js",
                @"/swagger/index.js",
                @"/swagger/index.css",
            };

            foreach (var asset in assets)
            {
                using var response = await host.Client.GetAsync(asset, cancellationToken);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.NotEmpty(await response.Content.ReadAsStringAsync(cancellationToken));
            }
        }

        [Fact]
        public async Task PublishedDocumentMatchesTheContractMetadataAndSupportsEtag()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var host = await CreateHostAsync(@"Testing", cancellationToken);

            using var response = await host.Client.GetAsync(@"/openapi/v1.json", cancellationToken);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(@"application/json", response.Content.Headers.ContentType?.MediaType);
            Assert.Equal(
                Constants.OpenApi.CacheControl,
                Assert.Single(response.Headers.GetValues(Constants.Headers.CacheControl)));

            var etag = Assert.Single(response.Headers.GetValues(Constants.Headers.ETag));
            Assert.StartsWith(@"""", etag, StringComparison.Ordinal);
            Assert.EndsWith(@"""", etag, StringComparison.Ordinal);

            var document = JsonNode.Parse(await response.Content.ReadAsStringAsync(cancellationToken))?.AsObject()
                ?? throw new InvalidOperationException(@"El documento publicado no es un objeto JSON.");

            Assert.Equal(@"3.1.0", document[@"openapi"]?.GetValue<string>());
            Assert.Equal(@"1.0.0", document[@"info"]?[@"version"]?.GetValue<string>());
            Assert.NotNull(document[@"info"]?[@"x-reviewed-at"]);
            Assert.NotNull(document[@"info"]?[@"x-persistence-exception"]);
            Assert.Equal(5, document[@"tags"]?.AsArray().Count);
            Assert.Equal(18, document[@"paths"]?.AsObject().Count);
            Assert.Equal(23, document[@"components"]?[@"schemas"]?.AsObject().Count);

            using var conditional = new HttpRequestMessage(HttpMethod.Get, @"/openapi/v1.json");
            conditional.Headers.TryAddWithoutValidation(Constants.Headers.IfNoneMatch, etag);
            using var notModified = await host.Client.SendAsync(conditional, cancellationToken);

            Assert.Equal(HttpStatusCode.NotModified, notModified.StatusCode);
            Assert.Equal(etag, Assert.Single(notModified.Headers.GetValues(Constants.Headers.ETag)));
        }

        [Fact]
        public async Task DocumentationIsNotPublishedInOtherEnvironments()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var host = await CreateHostAsync(@"Production", cancellationToken);

            foreach (var path in new[] { @"/swagger", @"/swagger/index.html", @"/openapi/v1.json" })
            {
                using var response = await host.Client.GetAsync(path, cancellationToken);
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
        }

        private static async Task<TestHost> CreateHostAsync(string environment, CancellationToken cancellationToken)
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                Args = [],
                ContentRootPath = AppContext.BaseDirectory,
                EnvironmentName = environment,
            });

            builder.Configuration.AddJsonFile(
                Path.Combine(AppContext.BaseDirectory, @"appsettings.mock.json"),
                optional: false);
            builder.WebHost.UseUrls(@"http://127.0.0.1:0");
            builder.Configure();

            var app = builder.Build();
            await app.UseMiddlewaresAsync(cancellationToken);
            app.UseRegisterEndpointDefinitions();
            await app.StartAsync(cancellationToken);

            return new TestHost(app, new HttpClient { BaseAddress = new Uri(app.Urls.Single()) }, cancellationToken);
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
