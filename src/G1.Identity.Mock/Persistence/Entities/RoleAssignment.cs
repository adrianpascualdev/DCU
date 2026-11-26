namespace G1.Identity.Mock.Persistence.Entities
{
    /// <summary>
    /// Asignación auditada de un rol a una persona.
    /// </summary>
    public sealed class RoleAssignment
    {
        /// <summary>
        /// Identificador opaco de la asignación (asg_).
        /// </summary>
        public string AssignmentId { get; set; } = string.Empty;

        /// <summary>
        /// Identificador del usuario (FK lógica).
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Código del rol (FK lógica).
        /// </summary>
        public string RoleCode { get; set; } = string.Empty;

        /// <summary>
        /// Referencia opaca de ámbito (opcional).
        /// </summary>
        public string? ScopeRef { get; set; }

        /// <summary>
        /// Usuario que concedió el rol.
        /// </summary>
        public string GrantedBy { get; set; } = string.Empty;

        /// <summary>
        /// Fecha y hora de concesión (UTC).
        /// </summary>
        public DateTimeOffset GrantedAt { get; set; }

        /// <summary>
        /// Motivo legible de la asignación.
        /// </summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Fecha y hora de revocación (UTC).
        /// </summary>
        public DateTimeOffset? RevokedAt { get; set; }

        /// <summary>
        /// Indica si la asignación está activa (no revocada).
        /// </summary>
        /// <returns>True si RevokedAt es nulo.</returns>
        public bool IsActive()
        {
            return RevokedAt == null;
        }
    }
}
