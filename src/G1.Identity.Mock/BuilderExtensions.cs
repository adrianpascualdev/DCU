namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Configuración de los servicios del host del mock de identidad.
    /// </summary>
    public static class BuilderExtensions
    {
        /// <summary>
        /// Registra los servicios del host.
        /// </summary>
        /// <param name="builder">Constructor de la aplicación web.</param>
        /// <returns>El constructor de la aplicación configurado.</returns>
        public static WebApplicationBuilder Configure(this WebApplicationBuilder builder)
        {
            /* Servicios de infraestructura */
            builder.Services.AddOpenApi();

            return builder;
        }
    }
}
