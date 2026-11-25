using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace G1.Identity.Mock.OpenApi
{
    /// <summary>
    /// Publica en tiempo de ejecución el contrato normativo sin duplicar sus definiciones.
    /// </summary>
    internal sealed class ContractDocumentTransformer : IOpenApiDocumentTransformer
    {
        private readonly ContractDocumentProvider _provider;

        /// <summary>
        /// Inicializa el transformador con el proveedor del contrato incrustado.
        /// </summary>
        /// <param name="provider">Proveedor del contrato normativo.</param>
        public ContractDocumentTransformer(ContractDocumentProvider provider)
        {
            ArgumentNullException.ThrowIfNull(provider);
            _provider = provider;
        }

        /// <summary>
        /// Sustituye el documento generado por el contrato acordado y valida sus invariantes.
        /// </summary>
        /// <param name="document">Documento generado por ASP.NET Core.</param>
        /// <param name="context">Contexto de transformación del documento.</param>
        /// <param name="cancellationToken">Token de cancelación de la operación.</param>
        /// <returns>Una tarea que finaliza cuando el documento ha sido transformado.</returns>
        public async Task TransformAsync(
            OpenApiDocument document,
            OpenApiDocumentTransformerContext context,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(document);
            ArgumentNullException.ThrowIfNull(context);

            if (!string.Equals(context.DocumentName, Constants.OpenApi.DocumentName, StringComparison.Ordinal))
            {
                return;
            }

            var contract = await _provider.GetDocumentAsync(cancellationToken).ConfigureAwait(false);

            document.Info = contract.Info;
            document.JsonSchemaDialect = contract.JsonSchemaDialect;
            document.Servers = contract.Servers;
            document.Paths = contract.Paths;
            document.Webhooks = contract.Webhooks;
            document.Components = contract.Components;
            document.Security = contract.Security;
            document.Tags = contract.Tags;
            document.ExternalDocs = contract.ExternalDocs;

            if (contract.Extensions is not null)
            {
                document.Extensions ??= new Dictionary<string, IOpenApiExtension>();
                foreach (var extension in contract.Extensions)
                {
                    document.Extensions[extension.Key] = extension.Value;
                }
            }

            ContractDocumentInvariants.Verify(document);
        }
    }
}
