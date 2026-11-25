using System.Reflection;
using System.Text;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;

namespace G1.Identity.Mock.OpenApi
{
    /// <summary>
    /// Carga una copia del contrato normativo incrustado en el ensamblado.
    /// </summary>
    internal sealed class ContractDocumentProvider
    {
        private const string ResourceName = @"G1.Identity.Mock.OpenApi.ContractDocument.json";

        private readonly Lazy<Task<string>> _json;
        private readonly Lazy<Task<OpenApiDocument>> _document;

        /// <summary>
        /// Inicializa el proveedor con carga diferida del contrato.
        /// </summary>
        public ContractDocumentProvider()
        {
            _json = new Lazy<Task<string>>(LoadJsonAsync, LazyThreadSafetyMode.ExecutionAndPublication);
            _document = new Lazy<Task<OpenApiDocument>>(
                LoadDocumentAsync,
                LazyThreadSafetyMode.ExecutionAndPublication);
        }

        /// <summary>
        /// Devuelve el contrato normativo tal como se generó desde el YAML acordado.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación de la operación.</param>
        /// <returns>El documento OpenAPI en formato JSON.</returns>
        public async Task<string> GetJsonAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _json.Value.ConfigureAwait(false);
        }

        /// <summary>
        /// Devuelve una copia independiente del contrato normativo.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación de la operación.</param>
        /// <returns>El documento OpenAPI cargado desde el recurso incrustado.</returns>
        public async Task<OpenApiDocument> GetDocumentAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var document = await _document.Value.ConfigureAwait(false);
            return new OpenApiDocument(document);
        }

        private static async Task<string> LoadJsonAsync()
        {
            var assembly = typeof(ContractDocumentProvider).Assembly;
            await using var resource = assembly.GetManifestResourceStream(ResourceName)
                ?? throw new InvalidOperationException($@"No se encontró el recurso incrustado {ResourceName}.");
            using var reader = new StreamReader(resource, Encoding.UTF8);
            return await reader.ReadToEndAsync().ConfigureAwait(false);
        }

        private async Task<OpenApiDocument> LoadDocumentAsync()
        {
            var json = await _json.Value.ConfigureAwait(false);
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var settings = new OpenApiReaderSettings
            {
                LoadExternalRefs = true,
            };
            var (document, diagnostic) = await OpenApiDocument
                .LoadAsync(stream, @"json", settings, CancellationToken.None)
                .ConfigureAwait(false);

            if (document is null)
            {
                throw new InvalidOperationException($@"No se pudo cargar el contrato incrustado {ResourceName}.");
            }

            var errors = diagnostic?.Errors ?? [];
            if (errors.Count > 0)
            {
                var detail = string.Join(Environment.NewLine, errors.Select(error => error.Message));
                throw new InvalidOperationException($@"El contrato incrustado no es válido: {detail}");
            }

            return document;
        }
    }
}
