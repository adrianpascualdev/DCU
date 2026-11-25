using Microsoft.OpenApi;

namespace G1.Identity.Mock.OpenApi
{
    /// <summary>
    /// Comprueba que el documento publicado conserva los invariantes del contrato acordado.
    /// </summary>
    internal static class ContractDocumentInvariants
    {
        private static readonly string[] ExpectedTags =
        {
            @"OIDC",
            @"Identity",
            @"Sessions",
            @"Catalog",
            @"Mock",
        };

        /// <summary>
        /// Verifica metadatos, seguridad, etiquetas, ejemplos y cabeceras del contrato.
        /// </summary>
        /// <param name="document">Documento OpenAPI que se va a publicar.</param>
        internal static void Verify(OpenApiDocument document)
        {
            ArgumentNullException.ThrowIfNull(document);

            VerifyMetadata(document);
            VerifySecuritySchemes(document);
            VerifyTags(document);
            VerifyOperations(document);
            VerifyResponseHeaders(document);
        }

        private static void VerifyMetadata(OpenApiDocument document)
        {
            var info = document.Info;
            if (info is null
                || !string.Equals(info.Version, Constants.OpenApi.ApiVersion, StringComparison.Ordinal)
                || info.Extensions?.ContainsKey(@"x-reviewed-at") != true
                || info.Extensions?.ContainsKey(@"x-persistence-exception") != true)
            {
                throw new InvalidOperationException(@"El documento publicado no conserva la versión y las extensiones de gobernanza del contrato.");
            }

            if (document.Components?.Schemas is null || document.Components.Schemas.Count == 0)
            {
                throw new InvalidOperationException(@"El documento publicado no contiene los esquemas del contrato.");
            }
        }

        private static void VerifySecuritySchemes(OpenApiDocument document)
        {
            var schemes = document.Components?.SecuritySchemes;
            if (schemes is null
                || !schemes.TryGetValue(@"cookieAuth", out var cookie)
                || cookie.Type != SecuritySchemeType.ApiKey
                || cookie.In != ParameterLocation.Cookie
                || !schemes.TryGetValue(@"bearerAuth", out var bearer)
                || bearer.Type != SecuritySchemeType.Http
                || !string.Equals(bearer.Scheme, @"bearer", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(@"El documento publicado no conserva los esquemas cookieAuth y bearerAuth.");
            }
        }

        private static void VerifyTags(OpenApiDocument document)
        {
            var actual = document.Tags?
                .Select(tag => tag.Name)
                .ToHashSet(StringComparer.Ordinal)
                ?? [];

            if (actual.Count != ExpectedTags.Length || !ExpectedTags.All(actual.Contains))
            {
                throw new InvalidOperationException(@"El documento publicado no conserva las etiquetas acordadas.");
            }
        }

        private static void VerifyOperations(OpenApiDocument document)
        {
            var operations = new List<OpenApiOperation>();
            if (document.Paths is not null)
            {
                foreach (var pathItem in document.Paths.Values)
                {
                    if (pathItem?.Operations is not null)
                    {
                        operations.AddRange(pathItem.Operations.Values);
                    }
                }
            }

            if (operations.Count != 19)
            {
                throw new InvalidOperationException($@"El documento publicado debe contener 19 operaciones; se encontraron {operations.Count}.");
            }

            foreach (var operation in operations)
            {
                var responses = operation.Responses ?? [];
                var success = responses.Where(response => IsSuccessStatus(response.Key)).ToList();
                var errors = responses.Where(response => !IsSuccessStatus(response.Key)).ToList();

                if (success.Count == 0 || !success.Any(response => HasExample(response.Value)))
                {
                    throw new InvalidOperationException($@"La operación {operation.OperationId} no documenta un ejemplo de éxito.");
                }

                if (errors.Count == 0 || !errors.Any(response => HasExample(response.Value)))
                {
                    throw new InvalidOperationException($@"La operación {operation.OperationId} no documenta los ejemplos de error.");
                }
            }
        }

        private static void VerifyResponseHeaders(OpenApiDocument document)
        {
            VerifyHeader(document, @"/api/v1/identity/me", HttpMethod.Get, @"200", @"Cache-Control");
            VerifyHeader(document, @"/api/v1/identity/catalog/roles", HttpMethod.Get, @"200", @"ETag");

            if (document.Components?.Headers?.ContainsKey(@"ETag") != true)
            {
                throw new InvalidOperationException(@"El documento publicado no define la cabecera ETag reutilizable.");
            }
        }

        private static void VerifyHeader(
            OpenApiDocument document,
            string path,
            HttpMethod method,
            string statusCode,
            string header)
        {
            var documented = document.Paths is not null
                && document.Paths.TryGetValue(path, out var pathItem)
                && pathItem.Operations is not null
                && pathItem.Operations.TryGetValue(method, out var operation)
                && operation.Responses is not null
                && operation.Responses.TryGetValue(statusCode, out var response)
                && response.Headers?.ContainsKey(header) == true;

            if (!documented)
            {
                throw new InvalidOperationException($@"La respuesta {statusCode} de {method.Method} {path} no documenta {header}.");
            }
        }

        private static bool IsSuccessStatus(string statusCode)
        {
            return int.TryParse(statusCode, out var code) && code < 400;
        }

        private static bool HasExample(IOpenApiResponse response)
        {
            if (response.Extensions?.ContainsKey(@"x-success-example") == true)
            {
                return true;
            }

            return response.Content?.Values.Any(media =>
                media.Example is not null
                || media.Examples is { Count: > 0 }) == true;
        }
    }
}
