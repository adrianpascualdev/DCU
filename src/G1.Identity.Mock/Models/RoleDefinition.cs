namespace G1.Identity.Mock.Models
{
    /// <summary>
    /// Define un rol estable compartido por los módulos del proyecto.
    /// </summary>
    internal sealed class RoleDefinition
    {
        /// <summary>
        /// Inicializa una definición de rol.
        /// </summary>
        /// <param name="code">Código estable en inglés.</param>
        /// <param name="label">Etiqueta legible en castellano.</param>
        /// <param name="description">Descripción funcional.</param>
        /// <param name="assignable">Indica si el rol puede asignarse a una persona.</param>
        internal RoleDefinition(string code, string label, string description, bool assignable)
        {
            Code = code;
            Label = label;
            Description = description;
            Assignable = assignable;
        }

        /// <summary>
        /// Obtiene el código estable del rol.
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Obtiene la etiqueta del rol.
        /// </summary>
        public string Label { get; }

        /// <summary>
        /// Obtiene la descripción funcional del rol.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Obtiene un valor que indica si el rol es asignable.
        /// </summary>
        public bool Assignable { get; }
    }
}
