using System;

namespace G1.Identity.Mock.Persistence.Entities
{
    /// <summary>
    /// Vínculo entre un token de OpenIddict y una sesión del mock.
    /// </summary>
    public sealed class SessionToken
    {
        /// <summary>
        /// Identificador del token en OpenIddict.
        /// </summary>
        public string TokenId { get; set; } = string.Empty;

        /// <summary>
        /// Identificador de la sesión asociada (ses_).
        /// </summary>
        public string SessionId { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de token emitido.
        /// </summary>
        public SessionTokenType TokenType { get; set; }

        /// <summary>
        /// Fecha de creación del vínculo (UTC).
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }
    }

    /// <summary>
    /// Tipos de token soportados en el vínculo de sesión.
    /// </summary>
    public enum SessionTokenType
    {
        /// <summary>
        /// Token de acceso.
        /// </summary>
        AccessToken,

        /// <summary>
        /// Token de identidad.
        /// </summary>
        IdToken,

        /// <summary>
        /// Token de refresco.
        /// </summary>
        RefreshToken
    }
}
