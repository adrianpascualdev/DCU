using System.Security.Claims;

namespace G1.Identity.Mock.Endpoints
{
    /// <summary>
    /// Contrato que DEV3 debe registrar para resolver una persona seleccionada en el host.
    /// </summary>
    /// <remarks>
    /// La implementación es responsabilidad de DEV3: debe comprobar la persona contra
    /// <c>Fixtures/SeedData.cs</c> y rechazar estados <c>inactive</c> o <c>locked</c>
    /// devolviendo <see langword="null"/>. DEV1 no proporciona una implementación.
    /// </remarks>
    internal interface IPersonaSelectionValidator
    {
        /// <summary>
        /// Resuelve la clave de persona y devuelve el principal que debe abrir la sesión del mock.
        /// </summary>
        /// <param name="personaKey">Clave opaca seleccionada en el selector.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>El principal de la persona activa, o <see langword="null"/> si no es válida.</returns>
        ValueTask<ClaimsPrincipal?> ValidateAsync(
            string personaKey,
            CancellationToken cancellationToken);
    }
}
