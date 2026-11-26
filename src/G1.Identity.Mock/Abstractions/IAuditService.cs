using G1.Identity.Mock.Models;

namespace G1.Identity.Mock.Abstractions
{
    /// <summary>
    /// Registra eventos de seguridad minimizados para comprobaciones del mock.
    /// </summary>
    internal interface IAuditService
    {
        /// <summary>
        /// Registra un evento sin incluir tokens ni datos personales.
        /// </summary>
        void Record(string eventType, string actorUserId, string targetId, string result, string correlationId);

        /// <summary>
        /// Obtiene una copia ordenada de los eventos registrados.
        /// </summary>
        IReadOnlyList<AuditEvent> List();

        /// <summary>
        /// Elimina los eventos para restaurar los fixtures del mock.
        /// </summary>
        void Reset();
    }
}
