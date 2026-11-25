using G1.Identity.Mock.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SessionSettings = G1.Identity.Mock.Settings.SessionOptions;

namespace G1.Identity.ContractTests
{
    /// <summary>
    /// Comprueba el enlazado y la validación fail-fast de las opciones de D-004.
    /// </summary>
    public sealed class OptionsValidationTests
    {
        [Fact]
        public void DefaultConfigurationBindsD004LifetimesAndSession()
        {
            using var provider = CreateProvider();

            var mock = provider.GetRequiredService<IOptions<MockOptions>>().Value;
            var lifetimes = provider.GetRequiredService<IOptions<OpenIdConnectOptions>>().Value;
            var session = provider.GetRequiredService<IOptions<SessionSettings>>().Value;

            Assert.Equal(new Uri("https://localhost:7180"), mock.Issuer);
            Assert.Equal("identity-api", mock.Audience);
            Assert.Equal("cli_mock_spa", mock.ClientId);
            Assert.Equal(5, mock.Scopes.Count);
            Assert.Equal(TimeSpan.FromMinutes(5), lifetimes.AuthorizationCode);
            Assert.Equal(TimeSpan.FromMinutes(5), lifetimes.AccessToken);
            Assert.Equal(TimeSpan.FromMinutes(5), lifetimes.IdentityToken);
            Assert.Equal(TimeSpan.FromHours(8), lifetimes.RefreshToken);
            Assert.Equal(TimeSpan.FromMinutes(2), lifetimes.ClockSkew);
            Assert.Equal(TimeSpan.FromMinutes(30), session.IdleTimeout);
            Assert.Equal(TimeSpan.FromHours(12), session.AbsoluteTimeout);
        }

        [Fact]
        public void ClockSkewAboveTwoMinutesFailsValidation()
        {
            using var provider = CreateProvider(new Dictionary<string, string?>
            {
                [@"Identity:Lifetimes:ClockSkew"] = @"00:03:00",
            });

            var options = provider.GetRequiredService<IOptions<OpenIdConnectOptions>>();
            Assert.Throws<OptionsValidationException>(() => _ = options.Value);
        }

        [Fact]
        public void IdleTimeoutNotShorterThanAbsoluteFailsValidation()
        {
            using var provider = CreateProvider(new Dictionary<string, string?>
            {
                [@"Identity:Session:IdleTimeout"] = @"13:00:00",
            });

            var options = provider.GetRequiredService<IOptions<SessionSettings>>();
            Assert.Throws<OptionsValidationException>(() => _ = options.Value);
        }

        [Fact]
        public void WildcardCorsOriginFailsValidation()
        {
            using var provider = CreateProvider(new Dictionary<string, string?>
            {
                [@"Identity:Cors:AllowedOrigins:0"] = @"*",
            });

            var options = provider.GetRequiredService<IOptions<CorsOptions>>();
            Assert.Throws<OptionsValidationException>(() => _ = options.Value);
        }

        [Fact]
        public void NonHttpsIssuerFailsValidation()
        {
            using var provider = CreateProvider(new Dictionary<string, string?>
            {
                [@"Identity:Issuer"] = @"http://localhost:5180",
            });

            var options = provider.GetRequiredService<IOptions<MockOptions>>();
            Assert.Throws<OptionsValidationException>(() => _ = options.Value);
        }

        [Fact]
        public void MissingIssuerFailsValidation()
        {
            using var provider = CreateProvider(new Dictionary<string, string?>
            {
                [@"Identity:Issuer"] = null,
            });

            var options = provider.GetRequiredService<IOptions<MockOptions>>();
            Assert.Throws<OptionsValidationException>(() => _ = options.Value);
        }

        private static ServiceProvider CreateProvider(IDictionary<string, string?>? overrides = null)
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                Args = [],
                ContentRootPath = AppContext.BaseDirectory,
                EnvironmentName = @"Production",
            });

            builder.Configuration.AddJsonFile(
                Path.Combine(AppContext.BaseDirectory, @"appsettings.mock.json"),
                optional: false);

            if (overrides is not null)
            {
                builder.Configuration.AddInMemoryCollection(overrides);
            }

            builder.Configure();
            return builder.Services.BuildServiceProvider();
        }
    }
}
