namespace G1.Identity.Mock.Persistence.Entities
{
    /// <summary>
    /// Rol del catálogo provisional v1.
    /// </summary>
    public sealed class Role
    {
        /// <summary>
        /// Código único y estable (snake_case).
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Etiqueta en castellano.
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Descripción en castellano.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Indica si puede asignarse en la administración futura.
        /// </summary>
        public bool Assignable { get; set; }

        /// <summary>
        /// Versión del catálogo.
        /// </summary>
        public string CatalogVersion { get; set; } = string.Empty;
    }
}
