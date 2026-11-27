using System;
using System.Collections.Generic;

namespace G1.Identity.Mock.Persistence.Entities
{
    /// <summary>
    /// Traza de seguridad del mock de identidad.
    /// </summary>
    public sealed class AuditEvent
    {
        /// <summary>
        /// Identificador opaco del evento (evt_).
        /// </summary>
        public string EventId { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de evento de seguridad.
        /// </summary>
        public AuditEventType Type { get; set; }

        /// <summary>
        /// Identificador del actor que realiza la acción (usr_ o system).
        /// </summary>
        public string? ActorId { get; set; }

        /// <summary>
        /// Identificador del objetivo de la acción (usr_, ses_ o cli_).
        /// </summary>
        public string? TargetId { get; set; }

        /// <summary>
        /// Resultado de la operación.
        /// </summary>
        public AuditEventResult Result { get; set; }

        /// <summary>
        /// Identificador de correlación para trazabilidad.
        /// </summary>
        public string CorrelationId { get; set; } = string.Empty;

        /// <summary>
        /// Marca temporal del evento (UTC).
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// Metadatos adicionales del evento.
        /// </summary>
        /// <remarks>
        /// No debe contener tokens, secretos ni datos personales innecesarios.
        /// </remarks>
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Añade un metadato al evento validando que no sea un dato sensible.
        /// </summary>
        /// <param name="key">Clave del metadato.</param>
        /// <param name="value">Valor del metadato.</param>
        /// <exception cref="InvalidOperationException">Si la clave se considera sensible.</exception>
        public void AddMetadata(string key, string value)
        {
            var lowerKey = key.ToLowerInvariant();
            if (lowerKey.Contains("token") || 
                lowerKey.Contains("password") || 
                lowerKey.Contains("secret") || 
                lowerKey.Contains("health"))
            {
                throw new InvalidOperationException($"No se permite registrar el metadato sensible: {key}");
            }

            Metadata[key] = value;
        }
    }

    /// <summary>
    /// Tipos de eventos de auditoría.
    /// </summary>
    public enum AuditEventType
    {
        /// <summary>
        /// Intento de autenticación.
        /// </summary>
        Authentication,

        /// <summary>
        /// Emisión de un token.
        /// </summary>
        TokenIssued,

        /// <summary>
        /// Revocación de un token.
        /// </summary>
        TokenRevoked,

        /// <summary>
        /// Cierre de sesión.
        /// </summary>
        Logout,

        /// <summary>
        /// Revocación de una sesión.
        /// </summary>
        SessionRevoked,

        /// <summary>
        /// Acceso denegado por política.
        /// </summary>
        AccessDenied,

        /// <summary>
        /// Reinicio de los datos del mock.
        /// </summary>
        MockReset
    }

    /// <summary>
    /// Resultados posibles de un evento de auditoría.
    /// </summary>
    public enum AuditEventResult
    {
        /// <summary>
        /// Operación exitosa.
        /// </summary>
        Success,

        /// <summary>
        /// Error técnico en la operación.
        /// </summary>
        Failure,

        /// <summary>
        /// Operación denegada por reglas de negocio o seguridad.
        /// </summary>
        Denied
    }
}
