using G1.Identity.Mock.Models;

namespace G1.Identity.Mock.Abstractions
{
    /// <summary>
    /// Gestiona el ciclo de vida de sesiones y refresh tokens del mock.
    /// </summary>
    internal interface ISessionService
    {
        /// <summary>
        /// Crea una sesión nueva sin permitir reutilizar un identificador revocado.
        /// </summary>
        SessionSnapshot Create(
            string sessionId,
            string userId,
            string clientId,
            IReadOnlyList<string> authenticationMethods,
            string deviceLabel);

        /// <summary>
        /// Obtiene las sesiones activas de una persona.
        /// </summary>
        IReadOnlyList<SessionSnapshot> ListActive(string userId);

        /// <summary>
        /// Actualiza la actividad de una sesión activa.
        /// </summary>
        bool Touch(string sessionId);

        /// <summary>
        /// Revoca una sesión propia y los refresh tokens vinculados.
        /// </summary>
        SessionMutationResult Revoke(string userId, string sessionId, string reason);

        /// <summary>
        /// Revoca las sesiones de una persona y devuelve cuántas cambiaron de estado.
        /// </summary>
        int RevokeAll(string userId, string currentSessionId, bool keepCurrentSession, string reason);

        /// <summary>
        /// Vincula un refresh token opaco a una sesión activa.
        /// </summary>
        bool LinkRefreshToken(string sessionId, string tokenId);

        /// <summary>
        /// Sustituye un refresh token activo por el siguiente sin permitir reutilizar ninguno de los dos.
        /// </summary>
        bool RotateRefreshToken(string sessionId, string previousTokenId, string nextTokenId);

        /// <summary>
        /// Comprueba si un refresh token sigue activo dentro de su sesión.
        /// </summary>
        bool IsRefreshTokenActive(string sessionId, string tokenId);
    }
}
