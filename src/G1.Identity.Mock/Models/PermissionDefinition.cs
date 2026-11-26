namespace G1.Identity.Mock.Models
{
    /// <summary>
    /// Define un permiso atómico publicado por su módulo propietario.
    /// </summary>
    internal sealed class PermissionDefinition
    {
        /// <summary>
        /// Inicializa una definición de permiso.
        /// </summary>
        /// <param name="code">Código estable con espacio de nombres.</param>
        /// <param name="label">Etiqueta legible en castellano.</param>
        /// <param name="ownerModule">Módulo propietario del permiso.</param>
        /// <param name="description">Descripción funcional.</param>
        internal PermissionDefinition(string code, string label, string ownerModule, string description)
        {
            Code = code;
            Label = label;
            OwnerModule = ownerModule;
            Description = description;
        }

        /// <summary>
        /// Obtiene el código estable del permiso.
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Obtiene la etiqueta del permiso.
        /// </summary>
        public string Label { get; }

        /// <summary>
        /// Obtiene el módulo propietario del permiso.
        /// </summary>
        public string OwnerModule { get; }

        /// <summary>
        /// Obtiene la descripción funcional del permiso.
        /// </summary>
        public string Description { get; }
    }
}
