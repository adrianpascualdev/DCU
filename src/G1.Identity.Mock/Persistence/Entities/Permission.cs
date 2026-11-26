namespace G1.Identity.Mock.Persistence.Entities
{
    /// <summary>
    /// Permiso atómico propiedad de G1.
    /// </summary>
    public sealed class Permission
    {
        /// <summary>
        /// Código único y estable con espacio de nombres.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Etiqueta en castellano.
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Módulo propietario (ej. G1).
        /// </summary>
        public string OwnerModule { get; set; } = string.Empty;

        /// <summary>
        /// Descripción en castellano.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Versión del catálogo.
        /// </summary>
        public string CatalogVersion { get; set; } = string.Empty;
    }
}
