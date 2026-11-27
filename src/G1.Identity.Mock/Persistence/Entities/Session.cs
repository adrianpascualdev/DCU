using System;
using System.Collections.Generic;

namespace G1.Identity.Mock.Persistence.Entities
{
    /// <summary>
    /// Periodo de autenticación de una persona en el mock.
    /// </summary>
    public sealed class Session
    {
        /// <summary>
        /// Identificador opaco de la sesión (ses_).
        /// </summary>
        public string SessionId { get; set; } = string.Empty;

        /// <summary>
        /// Identificador del usuario asociado (usr_).
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Identificador del cliente OAuth que inició la sesión (cli_).
        /// </summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// Fecha de creación de la sesión (UTC).
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// Fecha de última actividad detectada (UTC).
        /// </summary>
        public DateTimeOffset LastSeenAt { get; set; }

        /// <summary>
        /// Fecha de expiración por inactividad (UTC).
        /// </summary>
        public DateTimeOffset ExpiresAt { get; set; }

        /// <summary>
        /// Fecha de expiración absoluta (UTC).
        /// </summary>
        public DateTimeOffset AbsoluteExpiresAt { get; set; }

        /// <summary>
        /// Estado actual de la sesión.
        /// </summary>
        public SessionStatus Status { get; set; } = SessionStatus.Active;

        /// <summary>
        /// Métodos de autenticación usados para abrir la sesión.
        /// </summary>
        public List<string> AuthenticationMethods { get; set; } = new List<string>();

        /// <summary>
        /// Etiqueta descriptiva del dispositivo, sanitizada.
        /// </summary>
        public string DeviceLabel { get; set; } = string.Empty;

        /// <summary>
        /// Fecha de revocación si aplica (UTC).
        /// </summary>
        public DateTimeOffset? RevokedAt { get; set; }

        /// <summary>
        /// Motivo de la revocación.
        /// </summary>
        public string? RevocationReason { get; set; }

        /// <summary>
        /// Indica si la sesión está activa y no ha expirado ni ha sido revocada.
        /// </summary>
        /// <returns>True si la sesión es válida para operar.</returns>
        public bool IsActive()
        {
            return Status == SessionStatus.Active && 
                   DateTimeOffset.UtcNow < ExpiresAt && 
                   DateTimeOffset.UtcNow < AbsoluteExpiresAt;
        }

        /// <summary>
        /// Valida que las fechas de expiración sean consistentes.
        /// </summary>
        /// <returns>True si ExpiresAt es menor o igual a AbsoluteExpiresAt.</returns>
        public bool HasValidExpirations()
        {
            return ExpiresAt <= AbsoluteExpiresAt;
        }
    }

    /// <summary>
    /// Estados posibles de una sesión.
    /// </summary>
    public enum SessionStatus
    {
        /// <summary>
        /// Sesión válida y en uso.
        /// </summary>
        Active,

        /// <summary>
        /// Sesión revocada explícitamente.
        /// </summary>
        Revoked,

        /// <summary>
        /// Sesión expirada por inactividad o tiempo absoluto.
        /// </summary>
        Expired
    }
}
