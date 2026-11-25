using Microsoft.EntityFrameworkCore;

namespace G1.Identity.Mock.Persistence
{
    /// <summary>
    /// Contexto de base de datos para el mock de identidad.
    /// </summary>
    /// <remarks>
    /// Utiliza EF Core InMemory para propósitos académicos y de desarrollo (ADR-0002).
    /// Los datos son volátiles y se pierden al reiniciar el proceso.
    /// </remarks>
    public sealed class G1IdentityDbContext : DbContext
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="G1IdentityDbContext"/>.
        /// </summary>
        /// <param name="options">Las opciones para configurar el contexto.</param>
        public G1IdentityDbContext(DbContextOptions<G1IdentityDbContext> options)
            : base(options)
        {
        }

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Registrar las entidades de OpenIddict (requiere OpenIddict.EntityFrameworkCore)
            modelBuilder.UseOpenIddict();
        }
    }
}
