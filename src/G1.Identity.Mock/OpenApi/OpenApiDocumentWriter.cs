using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;

namespace G1.Identity.Mock.OpenApi
{
    /// <summary>
    /// Publica el contrato generado y calcula su ETag sin alterar su contenido.
    /// </summary>
    internal static class OpenApiDocumentWriter
    {
        /// <summary>
        /// Comprueba la versión del contrato generado y devuelve el JSON publicable.
        /// </summary>
        /// <param name="json">Contrato generado desde el YAML normativo.</param>
        /// <returns>El documento serializado como JSON.</returns>
        internal static string Write(string json)
        {
            ArgumentNullException.ThrowIfNull(json);

            var node = JsonNode.Parse(json) as JsonObject
                ?? throw new InvalidOperationException(@"No se pudo interpretar el documento OpenAPI incrustado.");

            if (!string.Equals(
                    node[Constants.OpenApi.SpecificationField]?.GetValue<string>(),
                    Constants.OpenApi.SpecificationVersion,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $@"El contrato incrustado debe declarar OpenAPI {Constants.OpenApi.SpecificationVersion}.");
            }

            return json;
        }

        /// <summary>
        /// Calcula una ETag fuerte sobre el contenido del documento.
        /// </summary>
        /// <param name="json">Documento serializado.</param>
        /// <returns>La ETag entre comillas, lista para la cabecera HTTP.</returns>
        internal static string ComputeETag(string json)
        {
            ArgumentNullException.ThrowIfNull(json);

            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(json));
            return $@"""{Convert.ToHexString(hash)}""";
        }
    }
}
