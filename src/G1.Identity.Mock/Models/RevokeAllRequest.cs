namespace G1.Identity.Mock.Models
{
    /// <summary>
    /// Opciones para revocar todas las sesiones de una persona.
    /// </summary>
    internal sealed class RevokeAllRequest
    {
        /// <summary>
        /// Obtiene o establece si la sesión desde la que se ejecuta la operación permanece activa.
        /// </summary>
        public bool KeepCurrentSession { get; init; } = true;
    }
}
