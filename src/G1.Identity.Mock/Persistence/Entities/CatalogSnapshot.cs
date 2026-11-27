using System;
using System.Collections.Generic;

namespace G1.Identity.Mock.Persistence.Entities
{
    /// <summary>
    /// Versión publicada del catálogo de roles y permisos.
    /// </summary>
    public sealed class CatalogSnapshot
    {
        /// <summary>
        /// Versión del catálogo (semver o fecha).
        /// </summary>
        public string CatalogVersion { get; set; } = string.Empty;

        /// <summary>
        /// Indica si el catálogo es provisional (no ratificado).
        /// </summary>
        public bool Provisional { get; set; }

        /// <summary>
        /// Fecha de publicación del catálogo (UTC).
        /// </summary>
        public DateTimeOffset PublishedAt { get; set; }

        /// <summary>
        /// Lista completa de roles en esta versión.
        /// </summary>
        public List<Role> Roles { get; set; } = new List<Role>();

        /// <summary>
        /// Lista de permisos propiedad de G1 en esta versión.
        /// </summary>
        public List<Permission> Permissions { get; set; } = new List<Permission>();
    }
}
