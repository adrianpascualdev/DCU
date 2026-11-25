using System.Threading.RateLimiting;
using G1.Identity.Mock;
using G1.Identity.Mock.Exceptions;
using G1.Identity.Mock.Resources;
using G1.Identity.Mock.Settings;
using Microsoft.AspNetCore.RateLimiting;
using SessionSettings = G1.Identity.Mock.Settings.SessionOptions;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Configuración de los servicios del host del mock de identidad.
    /// </summary>
    public static class BuilderExtensions
    {
        /// <summary>
        /// Registra la configuración, las opciones validadas y los servicios de infraestructura.
        /// </summary>
        /// <param name="builder">Constructor de la aplicación web.</param>
        /// <returns>El constructor de la aplicación configurado.</returns>
        /// <exception cref="ArgumentNullException">
        /// Se produce cuando <paramref name="builder"/> es nulo.
        /// </exception>
        public static WebApplicationBuilder Configure(this WebApplicationBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            /* Leer Configuración */
            // WebApplication.CreateBuilder ya carga appsettings.json, appsettings.{Environment}.json
            // y variables de entorno en ese orden de prioridad.

            /* Configuración de telemetría */
            // Se conservan los proveedores y niveles definidos en appsettings.json.

            /* Registro de settings/options */
            builder.ConfigureIdentityOptions();

            /* Servicios de infraestructura */
            builder.Services
                .AddOpenApi()
                .AddProblemDetails()
                .AddExceptionHandler<GlobalExceptionHandler>()
                .ConfigureCors(builder.Configuration)
                .ConfigureRateLimiter()
                .ConfigureAntiforgery();

            return builder;
        }

        private static WebApplicationBuilder ConfigureIdentityOptions(this WebApplicationBuilder builder)
        {
            builder.Services.AddOptionsWithValidateOnStart<MockOptions>()
                .Bind(builder.Configuration.GetSection(Constants.Configuration.Identity))
                .ValidateDataAnnotations()
                .Validate(
                    options => options.Issuer is { IsAbsoluteUri: true } issuer
                        && string.Equals(issuer.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase),
                    IdentityMessages.IssuerMustBeHttps);

            builder.Services.AddOptionsWithValidateOnStart<OpenIdConnectOptions>()
                .Bind(builder.Configuration.GetSection(Constants.Configuration.Lifetimes))
                .ValidateDataAnnotations();

            builder.Services.AddOptionsWithValidateOnStart<SessionSettings>()
                .Bind(builder.Configuration.GetSection(Constants.Configuration.Session))
                .ValidateDataAnnotations()
                .Validate(
                    options => options.IdleTimeout < options.AbsoluteTimeout,
                    IdentityMessages.IdleLessThanAbsolute);

            builder.Services.AddOptionsWithValidateOnStart<CorsOptions>()
                .Bind(builder.Configuration.GetSection(Constants.Configuration.Cors))
                .Validate(
                    options => options.AllowedOrigins.All(IsNotWildcard),
                    IdentityMessages.AllowedOriginsNoWildcard)
                .Validate(
                    options => options.AllowedOrigins.All(IsAbsoluteHttpOrigin),
                    IdentityMessages.AllowedOriginsMustBeAbsolute);

            return builder;
        }

        private static IServiceCollection ConfigureCors(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var origins = configuration
                .GetSection(Constants.Configuration.Cors)
                .Get<CorsOptions>()?
                .AllowedOrigins
                ?? Array.Empty<string>();

            services.AddCors(options =>
            {
                options.AddPolicy(
                    Constants.Policies.Cors,
                    policy => policy
                        .WithOrigins(origins.ToArray())
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials());
            });

            return services;
        }

        private static IServiceCollection ConfigureRateLimiter(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.OnRejected = async (context, cancellationToken) =>
                {
                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        ProblemResponseWriter.SetRetryAfter(context.HttpContext.Response, retryAfter);
                    }

                    await ProblemResponseWriter.WriteAsync(
                        context.HttpContext,
                        StatusCodes.Status429TooManyRequests,
                        Constants.ErrorCodes.RateLimited,
                        cancellationToken);
                };

                AddFixedWindowPolicy(options, Constants.Policies.Authorization, Constants.RateLimits.AuthorizationPermitLimit);
                AddFixedWindowPolicy(options, Constants.Policies.Token, Constants.RateLimits.TokenPermitLimit);
                AddFixedWindowPolicy(options, Constants.Policies.MockLogin, Constants.RateLimits.MockLoginPermitLimit);
                AddFixedWindowPolicy(options, Constants.Policies.Registration, Constants.RateLimits.RegistrationPermitLimit);
                AddFixedWindowPolicy(options, Constants.Policies.Summary, Constants.RateLimits.SummaryPermitLimit);
            });

            return services;
        }

        private static void AddFixedWindowPolicy(RateLimiterOptions options, string policyName, int permitLimit)
        {
            options.AddFixedWindowLimiter(policyName, limiter =>
            {
                limiter.PermitLimit = permitLimit;
                limiter.Window = TimeSpan.FromSeconds(Constants.RateLimits.WindowSeconds);
                limiter.QueueLimit = 0;
                limiter.AutoReplenishment = true;
            });
        }

        private static IServiceCollection ConfigureAntiforgery(this IServiceCollection services)
        {
            services.AddAntiforgery(options =>
            {
                options.HeaderName = Constants.Headers.AntiforgeryToken;
                options.Cookie.Name = Constants.Antiforgery.CookieName;
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.SuppressXFrameOptionsHeader = true;
            });

            return services;
        }

        private static bool IsNotWildcard(string origin)
        {
            return !string.Equals(origin, @"*", StringComparison.Ordinal);
        }

        private static bool IsAbsoluteHttpOrigin(string origin)
        {
            return Uri.TryCreate(origin, UriKind.Absolute, out var uri)
                && (string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase));
        }
    }
}
