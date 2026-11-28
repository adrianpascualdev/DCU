using System.Security.Claims;
using G1.Identity.Mock.Endpoints;
using G1.Identity.Mock.Persistence;

namespace G1.Identity.Mock.Auth
{
    /// <summary>
    /// Valida la persona seleccionada en el selector del mock contra la persistencia
    /// y construye el principal autenticado con sus roles, permisos efectivos y minimización.
    /// </summary>
    internal sealed class PersonaSelectionValidator : IPersonaSelectionValidator
    {
        private readonly G1IdentityDbContext _dbContext;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="PersonaSelectionValidator"/>.
        /// </summary>
        /// <param name="dbContext">Contexto de persistencia del mock de identidad.</param>
        /// <exception cref="ArgumentNullException">
        /// Se produce cuando <paramref name="dbContext"/> es nulo.
        /// </exception>
        public PersonaSelectionValidator(G1IdentityDbContext dbContext)
        {
            ArgumentNullException.ThrowIfNull(dbContext);

            _dbContext = dbContext;
        }

        /// <inheritdoc />
        public async ValueTask<ClaimsPrincipal?> ValidateAsync(
            string personaKey,
            CancellationToken cancellationToken)
        {
            return await MockPersonaAuthenticationHandler.ResolvePrincipalAsync(
                _dbContext,
                personaKey,
                MockPersonaAuthenticationExtensions.SchemeName,
                cancellationToken).ConfigureAwait(false);
        }
    }
}
