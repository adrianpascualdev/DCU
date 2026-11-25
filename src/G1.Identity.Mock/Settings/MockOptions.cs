using System.ComponentModel.DataAnnotations;
using G1.Identity.Mock.Resources;

namespace G1.Identity.Mock.Settings
{
    /// <summary>
    /// Opciones generales del mock de identidad enlazadas a la sección <c>Identity</c>.
    /// </summary>
    /// <remarks>
    /// Los valores son los acordados para desarrollo y pruebas en D-004 y no incluyen secretos.
    /// </remarks>
    public sealed class MockOptions
    {
        /// <summary>
        /// Emisor de los tokens de identidad.
        /// </summary>
        [Required(
            ErrorMessageResourceType = typeof(IdentityMessages),
            ErrorMessageResourceName = nameof(IdentityMessages.IssuerRequired))]
        public required Uri Issuer { get; init; }

        /// <summary>
        /// Audiencia provisional de la API de identidad.
        /// </summary>
        [Required(
            ErrorMessageResourceType = typeof(IdentityMessages),
            ErrorMessageResourceName = nameof(IdentityMessages.AudienceRequired))]
        public required string Audience { get; init; }

        /// <summary>
        /// Identificador del cliente usado por el mock.
        /// </summary>
        [Required(
            ErrorMessageResourceType = typeof(IdentityMessages),
            ErrorMessageResourceName = nameof(IdentityMessages.ClientIdRequired))]
        public required string ClientId { get; init; }

        /// <summary>
        /// Ámbitos OIDC expuestos por el mock.
        /// </summary>
        [Required(
            ErrorMessageResourceType = typeof(IdentityMessages),
            ErrorMessageResourceName = nameof(IdentityMessages.ScopesRequired))]
        [MinLength(
            1,
            ErrorMessageResourceType = typeof(IdentityMessages),
            ErrorMessageResourceName = nameof(IdentityMessages.ScopesRequired))]
        public required IReadOnlyList<string> Scopes { get; init; }

        /// <summary>
        /// Duraciones de los tokens definidas en D-004.
        /// </summary>
        [Required(
            ErrorMessageResourceType = typeof(IdentityMessages),
            ErrorMessageResourceName = nameof(IdentityMessages.LifetimesRequired))]
        public required OpenIdConnectOptions Lifetimes { get; init; }

        /// <summary>
        /// Tiempos de inactividad y duración absoluta de la sesión.
        /// </summary>
        [Required(
            ErrorMessageResourceType = typeof(IdentityMessages),
            ErrorMessageResourceName = nameof(IdentityMessages.SessionRequiredOption))]
        public required SessionOptions Session { get; init; }
    }
}
