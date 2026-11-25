namespace G1.Identity.Mock.Settings
{
    /// <summary>
    /// Orígenes permitidos por la política CORS del mock.
    /// </summary>
    /// <remarks>
    /// La lista se define por entorno en <c>Identity:Cors</c>. Una lista vacía significa que no se
    /// permiten peticiones de origen cruzado; nunca se usa el comodín con credenciales.
    /// </remarks>
    public sealed class CorsOptions
    {
        /// <summary>
        /// Orígenes absolutos autorizados para peticiones con credenciales.
        /// </summary>
        public IReadOnlyList<string> AllowedOrigins { get; init; } = Array.Empty<string>();
    }
}
