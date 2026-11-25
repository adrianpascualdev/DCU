using System.Text;
using G1.Identity.Mock.Extensions;
using G1.Identity.Mock.OpenApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace G1.Identity.Mock.Endpoints
{
    /// <summary>
    /// Publica Swagger UI y el documento OpenAPI solo en Development y Testing.
    /// </summary>
    internal sealed class DocsEndpointDefinition : IEndpointDefinition
    {
        /// <summary>
        /// Registra la documentación interactiva y el documento OpenAPI del contrato.
        /// </summary>
        /// <param name="app">Instancia de la aplicación web.</param>
        public void RegisterEndpoints(WebApplication app)
        {
            ArgumentNullException.ThrowIfNull(app);

            if (!IsDocumentationEnabled(app.Environment))
            {
                return;
            }

            app.MapGet(Constants.Paths.OpenApiDocument, GetOpenApiDocumentAsync)
                .ExcludeFromDescription();

            app.UseSwaggerUI(options =>
            {
                options.RoutePrefix = Constants.OpenApi.SwaggerRoutePrefix;
                options.SwaggerEndpoint(Constants.Paths.OpenApiDocument, Constants.OpenApi.SwaggerTitle);
                options.DocumentTitle = Constants.OpenApi.SwaggerTitle;
            });
        }

        private static bool IsDocumentationEnabled(IHostEnvironment environment)
        {
            return environment.IsDevelopment()
                || string.Equals(
                    environment.EnvironmentName,
                    Constants.Environments.Testing,
                    StringComparison.OrdinalIgnoreCase);
        }

        private static async Task<IResult> GetOpenApiDocumentAsync(
            HttpContext context,
            CancellationToken cancellationToken)
        {
            var provider = context.RequestServices.GetRequiredService<ContractDocumentProvider>();
            var json = OpenApiDocumentWriter.Write(await provider.GetJsonAsync(cancellationToken).ConfigureAwait(false));
            var etag = OpenApiDocumentWriter.ComputeETag(json);

            context.Response.Headers[Constants.Headers.CacheControl] = Constants.OpenApi.CacheControl;
            context.Response.Headers[Constants.Headers.ETag] = etag;

            if (context.Request.Headers[Constants.Headers.IfNoneMatch]
                .Any(value => string.Equals(value, etag, StringComparison.Ordinal)))
            {
                return Results.StatusCode(StatusCodes.Status304NotModified);
            }

            return Results.Text(json, Constants.ContentTypes.Json, Encoding.UTF8);
        }
    }
}
