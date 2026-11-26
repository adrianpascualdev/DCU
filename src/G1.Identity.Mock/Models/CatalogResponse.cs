namespace G1.Identity.Mock.Models
{
    /// <summary>
    /// Representa una versión publicada de un catálogo de identidad.
    /// </summary>
    /// <typeparam name="TItem">Tipo de los elementos publicados.</typeparam>
    internal sealed class CatalogResponse<TItem>
    {
        /// <summary>
        /// Inicializa una respuesta de catálogo.
        /// </summary>
        /// <param name="catalogVersion">Versión estable del catálogo.</param>
        /// <param name="provisional">Indica si la versión está pendiente de ratificación.</param>
        /// <param name="items">Elementos del catálogo.</param>
        internal CatalogResponse(string catalogVersion, bool provisional, IReadOnlyList<TItem> items)
        {
            CatalogVersion = catalogVersion;
            Provisional = provisional;
            Items = items;
        }

        /// <summary>
        /// Obtiene la versión estable del catálogo.
        /// </summary>
        public string CatalogVersion { get; }

        /// <summary>
        /// Obtiene un valor que indica si el catálogo está pendiente de ratificación.
        /// </summary>
        public bool Provisional { get; }

        /// <summary>
        /// Obtiene los elementos publicados.
        /// </summary>
        public IReadOnlyList<TItem> Items { get; }
    }
}
