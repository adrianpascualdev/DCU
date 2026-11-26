namespace G1.Identity.Mock.Persistence.Entities
{
    /// <summary>
    /// Identidad ficticia que representa a un usuario del club en el mock.
    /// </summary>
    public sealed class MockPersona
    {
        /// <summary>
        /// Clave estable del fixture, única.
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// Identificador opaco, único, estable (usr_).
        /// </summary>
        public string UserId { get; init; } = string.Empty;

        /// <summary>
        /// Nombre visible ficticio.
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Correo ficticio válido, único.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Estado de verificación del correo.
        /// </summary>
        public bool EmailVerified { get; set; }

        /// <summary>
        /// Teléfono ficticio (opcional).
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// Estado de verificación del teléfono.
        /// </summary>
        public bool PhoneVerified { get; set; }

        /// <summary>
        /// Estado de la persona en el mock.
        /// </summary>
        public PersonaStatus Status { get; set; } = PersonaStatus.Active;

        /// <summary>
        /// Indica si la persona es menor de edad (minimización adicional).
        /// </summary>
        public bool IsMinor { get; set; }

        /// <summary>
        /// Métodos de autenticación usados en el fixture.
        /// </summary>
        public List<string> AuthenticationMethods { get; set; } = new List<string>();

        /// <summary>
        /// Etiquetas de caso (minor, unverified, etc.).
        /// </summary>
        public List<string> Labels { get; set; } = new List<string>();

        /// <summary>
        /// Versión del conjunto de fixtures.
        /// </summary>
        public string FixtureVersion { get; set; } = string.Empty;

        /// <summary>
        /// Indica si la persona puede completar el flujo de autorización.
        /// </summary>
        /// <returns>True si el estado es Active.</returns>
        public bool CanAuthorize()
        {
            return Status == PersonaStatus.Active;
        }

        /// <summary>
        /// Genera un resumen de la persona para terceros, excluyendo datos sensibles.
        /// </summary>
        /// <returns>Cadena con el resumen seguro.</returns>
        public string ToThirdPartySummary()
        {
            return $"User: {DisplayName} (ID: {UserId})";
        }
    }

    /// <summary>
    /// Estados posibles de una persona en el mock.
    /// </summary>
    public enum PersonaStatus
    {
        /// <summary>
        /// Persona activa y capaz de autorizar.
        /// </summary>
        Active,

        /// <summary>
        /// Persona inactiva.
        /// </summary>
        Inactive,

        /// <summary>
        /// Persona bloqueada.
        /// </summary>
        Locked
    }
}
