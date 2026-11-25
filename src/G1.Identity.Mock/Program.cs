using G1.Identity.Mock.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace G1.Identity.Mock
{
    /// <summary>
    /// Punto de entrada del host del mock de identidad.
    /// </summary>
    public sealed class Program
    {
        /// <summary>
        /// Inicia el host ASP.NET Core del mock.
        /// </summary>
        /// <param name="args">Argumentos de línea de comandos.</param>
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Configure().Build();
            app.UseMiddlewares().UseRegisterEndpointDefinitions();

            try
            {
                app.Run();
            }
            catch (Exception exception)
            {
                app.Logger.LogCritical(exception, "Error al iniciar el host del mock de identidad");
                throw;
            }
        }
    }
}
