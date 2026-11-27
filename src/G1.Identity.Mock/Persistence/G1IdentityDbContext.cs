using Microsoft.EntityFrameworkCore;
using G1.Identity.Mock.Persistence.Entities;

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

        /// <summary>
        /// Personas del mock.
        /// </summary>
        public DbSet<MockPersona> MockPersonas { get; set; } = null!;

        /// <summary>
        /// Roles del catálogo.
        /// </summary>
        public DbSet<Role> Roles { get; set; } = null!;

        /// <summary>
        /// Permisos de G1.
        /// </summary>
        public DbSet<Permission> Permissions { get; set; } = null!;

        /// <summary>
        /// Matriz rol-permiso.
        /// </summary>
        public DbSet<RolePermission> RolePermissions { get; set; } = null!;

        /// <summary>
        /// Asignaciones de roles a personas.
        /// </summary>
        public DbSet<RoleAssignment> RoleAssignments { get; set; } = null!;

        /// <summary>
        /// Sesiones activas e históricas.
        /// </summary>
        public DbSet<Session> Sessions { get; set; } = null!;

        /// <summary>
        /// Vínculos entre tokens y sesiones.
        /// </summary>
        public DbSet<SessionToken> SessionTokens { get; set; } = null!;

        /// <summary>
        /// Eventos de auditoría de seguridad.
        /// </summary>
        public DbSet<AuditEvent> AuditEvents { get; set; } = null!;

        /// <summary>
        /// Instantáneas del catálogo versionado.
        /// </summary>
        public DbSet<CatalogSnapshot> CatalogSnapshots { get; set; } = null!;

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Registrar las entidades de OpenIddict (requiere OpenIddict.EntityFrameworkCore)
            modelBuilder.UseOpenIddict();

            // Configuración de MockPersona
            modelBuilder.Entity<MockPersona>(entity =>
            {
                entity.HasKey(e => e.Key);
                entity.HasIndex(e => e.Key).IsUnique();
                entity.HasIndex(e => e.UserId).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();

                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.Email).IsRequired();
                entity.Property(e => e.DisplayName).IsRequired();
                entity.Property(e => e.FixtureVersion).IsRequired();
            });

            // Configuración de Role
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.Code);
                entity.Property(e => e.Code).IsRequired();
                entity.Property(e => e.Label).IsRequired();
                entity.Property(e => e.CatalogVersion).IsRequired();
            });

            // Configuración de Permission
            modelBuilder.Entity<Permission>(entity =>
            {
                entity.HasKey(e => e.Code);
                entity.Property(e => e.Code).IsRequired();
                entity.Property(e => e.Label).IsRequired();
                entity.Property(e => e.CatalogVersion).IsRequired();
            });

            // Configuración de RolePermission (clave compuesta)
            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(e => new { e.RoleCode, e.PermissionCode });
            });

            // Configuración de RoleAssignment
            modelBuilder.Entity<RoleAssignment>(entity =>
            {
                entity.HasKey(e => e.AssignmentId);
                entity.Property(e => e.AssignmentId).IsRequired();
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.RoleCode).IsRequired();
                entity.Property(e => e.GrantedBy).IsRequired();
                entity.Property(e => e.Reason).IsRequired();
            });

            // Configuración de Session
            modelBuilder.Entity<Session>(entity =>
            {
                entity.HasKey(e => e.SessionId);
                entity.Property(e => e.SessionId).IsRequired();
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.ClientId).IsRequired();
                entity.Property(e => e.DeviceLabel).IsRequired();
            });

            // Configuración de SessionToken
            modelBuilder.Entity<SessionToken>(entity =>
            {
                entity.HasKey(e => e.TokenId);
                entity.Property(e => e.TokenId).IsRequired();
                entity.Property(e => e.SessionId).IsRequired();
            });

            // Configuración de AuditEvent
            modelBuilder.Entity<AuditEvent>(entity =>
            {
                entity.HasKey(e => e.EventId);
                entity.Property(e => e.EventId).IsRequired();
                entity.Property(e => e.CorrelationId).IsRequired();
                entity.Ignore(e => e.Metadata);
            });

            // Configuración de CatalogSnapshot
            modelBuilder.Entity<CatalogSnapshot>(entity =>
            {
                entity.HasKey(e => e.CatalogVersion);
                entity.Property(e => e.CatalogVersion).IsRequired();
            });
        }
    }
}
