using G1.Identity.Mock.Endpoints;
using Microsoft.AspNetCore.Antiforgery;

namespace G1.Identity.Mock.Filters
{
    /// <summary>
    /// Valida antiforgery en escrituras autenticadas mediante cookie y permite Bearer sin CSRF.
    /// </summary>
    internal sealed class CookieAntiforgeryFilter : IEndpointFilter
    {
        /// <inheritdoc />
        public async ValueTask<object?> InvokeAsync(
            EndpointFilterInvocationContext context,
            EndpointFilterDelegate next)
        {
            if (!RequiresValidation(context.HttpContext.Request))
            {
                return await next(context);
            }

            var antiforgery = context.HttpContext.RequestServices.GetRequiredService<IAntiforgery>();
            try
            {
                await antiforgery.ValidateRequestAsync(context.HttpContext);
            }
            catch (AntiforgeryValidationException)
            {
                await ErrorMapping.WriteProblemAsync(
                    context.HttpContext,
                    Constants.ErrorCodes.RequestInvalid,
                    context.HttpContext.RequestAborted);
                return Results.Empty;
            }

            return await next(context);
        }

        /// <summary>
        /// Exige antiforgery siempre que viaje la cookie de sesión, aunque también se envíe
        /// una cabecera Bearer. Así una cabecera falsa no puede desactivar la protección CSRF.
        /// </summary>
        internal static bool RequiresValidation(HttpRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.Cookies.ContainsKey(Constants.Antiforgery.SessionCookieName))
            {
                return true;
            }

            var authorization = request.Headers.Authorization.ToString();
            return !authorization.StartsWith(@"Bearer ", StringComparison.OrdinalIgnoreCase);
        }
    }
}
