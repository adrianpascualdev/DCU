using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using G1.Identity.Mock.Extensions;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Abstractions;

namespace G1.Identity.Mock.Endpoints
{
    /// <summary>
    /// Publica el selector mock del host en una ruta interna no versionada, solo en Development/Testing.
    /// </summary>
    internal sealed class SelectorEndpointDefinition : IEndpointDefinition
    {
        /// <inheritdoc />
        public void RegisterEndpoints(WebApplication app)
        {
            ArgumentNullException.ThrowIfNull(app);

            if (!IsSelectorEnabled(app.Environment))
            {
                return;
            }

            var selector = app.MapGroup(Constants.Paths.Selector)
                .WithTags("Selector")
                .ExcludeFromDescription();

            selector.MapGet(string.Empty, ShowAsync);
            selector.MapPost(string.Empty, SubmitAsync);
        }

        /// <summary>
        /// Indica si el selector puede publicarse en el entorno indicado.
        /// </summary>
        /// <param name="environment">Entorno de ejecución.</param>
        /// <returns><see langword="true"/> únicamente en Development o Testing.</returns>
        internal static bool IsSelectorEnabled(IHostEnvironment environment)
        {
            ArgumentNullException.ThrowIfNull(environment);

            return environment.IsDevelopment()
                || string.Equals(
                    environment.EnvironmentName,
                    Constants.Environments.Testing,
                    StringComparison.OrdinalIgnoreCase);
        }

        private static async Task<IResult> ShowAsync(
            HttpContext context,
            IWebHostEnvironment environment,
            IAuthorizationContextBinder binder,
            IAuthorizationContextValidator validator,
            IAntiforgery antiforgery,
            CancellationToken cancellationToken)
        {
            context.Response.Headers[Constants.Headers.CacheControl] = Constants.Headers.NoStore;

            var requestUri = context.Request.Query[@"requestUri"].ToString();
            var clientId = context.Request.Query[@"clientId"].ToString();

            if (!await validator.IsValidAsync(requestUri, clientId, cancellationToken).ConfigureAwait(false)
                || !await binder.MatchesAsync(context, requestUri, clientId, cancellationToken).ConfigureAwait(false))
            {
                return Results.StatusCode(StatusCodes.Status400BadRequest);
            }

            var webRoot = environment.WebRootPath
                ?? Path.Combine(environment.ContentRootPath, "wwwroot");
            var indexPath = Path.Combine(webRoot, "selector", "index.html");
            if (!File.Exists(indexPath))
            {
                return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
            }

            var html = await File.ReadAllTextAsync(indexPath, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
            const string rootElement = @"<div id=""root""></div>";
            if (!html.Contains(rootElement, StringComparison.Ordinal))
            {
                return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
            }

            var tokens = antiforgery.GetAndStoreTokens(context);
            var rootWithContext = string.Concat(
                @"<div id=""root"" ",
                Constants.Selector.RequestUriAttribute,
                @"=""",
                HtmlEncoder.Default.Encode(requestUri),
                @""" ",
                Constants.Selector.ClientIdAttribute,
                @"=""",
                HtmlEncoder.Default.Encode(clientId),
                @""" ",
                Constants.Selector.AntiforgeryAttribute,
                @"=""",
                HtmlEncoder.Default.Encode(tokens.RequestToken ?? string.Empty),
                @"""></div>");

            html = html
                .Replace(rootElement, rootWithContext, StringComparison.Ordinal)
                .Replace(
                    @"src=""./assets/",
                    $@"src=""{Constants.Paths.SelectorAssets}/assets/",
                    StringComparison.Ordinal)
                .Replace(
                    @"href=""./assets/",
                    $@"href=""{Constants.Paths.SelectorAssets}/assets/",
                    StringComparison.Ordinal);

            return Results.Text(html, @"text/html", Encoding.UTF8);
        }

        private static async Task<IResult> SubmitAsync(
            HttpContext context,
            IAuthorizationContextBinder binder,
            IAuthorizationContextValidator validator,
            IAntiforgery antiforgery,
            IAuthenticationSchemeProvider schemes,
            TimeProvider time,
            IServiceProvider services,
            CancellationToken cancellationToken)
        {
            context.Response.Headers[Constants.Headers.CacheControl] = Constants.Headers.NoStore;

            try
            {
                await antiforgery.ValidateRequestAsync(context).ConfigureAwait(false);
            }
            catch (AntiforgeryValidationException)
            {
                return Results.StatusCode(StatusCodes.Status400BadRequest);
            }

            if (!context.Request.HasFormContentType)
            {
                return Results.StatusCode(StatusCodes.Status400BadRequest);
            }

            var form = await context.Request.ReadFormAsync(cancellationToken).ConfigureAwait(false);

            // La selección de identidad solo se acepta por POST y nunca desde una URL de retorno.
            if (form.ContainsKey(@"returnUrl")
                || form.ContainsKey(@"return_url")
                || form.ContainsKey(@"redirect_uri"))
            {
                return Results.StatusCode(StatusCodes.Status400BadRequest);
            }

            var requestUri = form[@"requestUri"].ToString();
            var clientId = form[@"clientId"].ToString();
            var personaKey = form[@"personaKey"].ToString();

            if (string.IsNullOrWhiteSpace(personaKey) || personaKey.Length > 128)
            {
                return Results.StatusCode(StatusCodes.Status400BadRequest);
            }

            if (!await validator.IsValidAsync(requestUri, clientId, cancellationToken).ConfigureAwait(false)
                || !await binder.TryConsumeAsync(context, requestUri, clientId, cancellationToken).ConfigureAwait(false))
            {
                return Results.StatusCode(StatusCodes.Status400BadRequest);
            }

            // DEV3 es responsable de esta implementación; si aún no está registrada, fallamos cerrados.
            var personas = services.GetService<IPersonaSelectionValidator>();
            if (personas is null)
            {
                return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
            }

            var principal = await personas.ValidateAsync(personaKey, cancellationToken).ConfigureAwait(false);
            if (principal is null
                || (principal.FindFirst(OpenIddictConstants.Claims.Subject) is null
                    && principal.FindFirst(ClaimTypes.NameIdentifier) is null))
            {
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            }

            if (await schemes.GetSchemeAsync(CookieAuthenticationDefaults.AuthenticationScheme)
                    .ConfigureAwait(false) is null)
            {
                return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
            }

            await context.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = false,
                    ExpiresUtc = time.GetUtcNow().Add(Constants.Selector.BindingLifetime),
                }).ConfigureAwait(false);

            // Destino fijo construido por el servidor: ningún parámetro del formulario controla la URL.
            // 303 evita que una recarga repita el POST de selección.
            var location = QueryHelpers.AddQueryString(
                context.Request.PathBase + Constants.Paths.Authorize,
                new Dictionary<string, string?>
                {
                    [@"client_id"] = clientId,
                    [@"request_uri"] = requestUri,
                });
            context.Response.Headers.Location = location;

            return Results.StatusCode(StatusCodes.Status303SeeOther);
        }
    }
}
