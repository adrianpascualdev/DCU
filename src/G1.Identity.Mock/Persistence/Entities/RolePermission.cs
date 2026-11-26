namespace G1.Identity.Mock.Persistence.Entities
{
    /// <summary>
    /// Matriz explícita rol-permiso.
    /// </summary>
    public sealed class RolePermission
    {
        /// <summary>
        /// Código del rol (FK lógica).
        /// </summary>
        public string RoleCode { get; set; } = string.Empty;

        /// <summary>
        /// Código del permiso (FK lógica).
        /// </summary>
        public string PermissionCode { get; set; } = string.Empty;

        /// <summary>
        /// Indica si el permiso está concedido.
        /// </summary>
        public bool Granted { get; set; }
    }
}
