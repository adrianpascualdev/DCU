using Microsoft.EntityFrameworkCore;
using G1.Identity.Mock.Persistence;
using G1.Identity.Mock.Persistence.Entities;

namespace G1.Identity.Mock.Fixtures
{
    /// <summary>
    /// Semilla de datos para el mock de identidad.
    /// </summary>
    public static class SeedData
    {
        private const string CatalogVersion = "v1.0.0-provisional";
        private const string FixtureVersion = "v1.0.0";

        /// <summary>
        /// Asegura que los datos base (personas, roles, permisos) estén presentes en la base de datos.
        /// </summary>
        /// <param name="context">El contexto de base de datos.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        /// <exception cref="ArgumentNullException">Se produce cuando <paramref name="context"/> es nulo.</exception>
        public static async Task EnsureSeedDataAsync(G1IdentityDbContext context, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(context);

            await SeedRolesAsync(context, cancellationToken);
            await SeedPermissionsAsync(context, cancellationToken);
            await SeedRolePermissionsAsync(context, cancellationToken);
            await SeedPersonasAsync(context, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);
        }

        private static async Task SeedRolesAsync(G1IdentityDbContext context, CancellationToken cancellationToken)
        {
            if (await context.Roles.AnyAsync(cancellationToken))
            {
                return;
            }

            var roles = new List<Role>
            {
                new Role { Code = "club_admin", Label = "Administrador del Club", Description = "Acceso total a la configuración del club.", Assignable = true, CatalogVersion = CatalogVersion },
                new Role { Code = "sports_manager", Label = "Gestor Deportivo", Description = "Gestión de secciones y competiciones.", Assignable = true, CatalogVersion = CatalogVersion },
                new Role { Code = "coach", Label = "Entrenador", Description = "Gestión de entrenamientos y convocatorias.", Assignable = true, CatalogVersion = CatalogVersion },
                new Role { Code = "player", Label = "Jugador", Description = "Acceso a ficha deportiva y horarios.", Assignable = true, CatalogVersion = CatalogVersion },
                new Role { Code = "tutor", Label = "Tutor/Familiar", Description = "Gestión de menores a cargo.", Assignable = true, CatalogVersion = CatalogVersion },
                new Role { Code = "member", Label = "Socio", Description = "Acceso a servicios del socio y votaciones.", Assignable = true, CatalogVersion = CatalogVersion },
                new Role { Code = "sponsor", Label = "Patrocinador", Description = "Acceso a portal de patrocinadores.", Assignable = true, CatalogVersion = CatalogVersion },
                new Role { Code = "supporter", Label = "Simpatizante", Description = "Acceso a contenidos exclusivos y tienda.", Assignable = true, CatalogVersion = CatalogVersion },
                new Role { Code = "kit_staff", Label = "Responsable de Material", Description = "Gestión de inventario y equipaciones.", Assignable = true, CatalogVersion = CatalogVersion },
                new Role { Code = "communications_staff", Label = "Responsable de Comunicaciones", Description = "Gestión de noticias y redes sociales.", Assignable = true, CatalogVersion = CatalogVersion }
            };

            await context.Roles.AddRangeAsync(roles, cancellationToken);
        }

        private static async Task SeedPermissionsAsync(G1IdentityDbContext context, CancellationToken cancellationToken)
        {
            if (await context.Permissions.AnyAsync(cancellationToken))
            {
                return;
            }

            var permissions = new List<Permission>
            {
                new Permission { Code = "identity.users.summary.read", Label = "Ver resumen de usuarios", OwnerModule = "G1", Description = "Permite ver el nombre y estado de otros usuarios.", CatalogVersion = CatalogVersion },
                new Permission { Code = "identity.users.lookup", Label = "Buscar usuarios", OwnerModule = "G1", Description = "Permite buscar usuarios por su identificador único.", CatalogVersion = CatalogVersion },
                new Permission { Code = "identity.admin", Label = "Administración de identidad", OwnerModule = "G1", Description = "Acceso total a la gestión de usuarios y seguridad.", CatalogVersion = CatalogVersion }
            };

            await context.Permissions.AddRangeAsync(permissions, cancellationToken);
        }

        private static async Task SeedRolePermissionsAsync(G1IdentityDbContext context, CancellationToken cancellationToken)
        {
            if (await context.RolePermissions.AnyAsync(cancellationToken))
            {
                return;
            }

            var matrix = new List<RolePermission>
            {
                // club_admin
                new RolePermission { RoleCode = "club_admin", PermissionCode = "identity.users.summary.read", Granted = true },
                new RolePermission { RoleCode = "club_admin", PermissionCode = "identity.users.lookup", Granted = true },
                new RolePermission { RoleCode = "club_admin", PermissionCode = "identity.admin", Granted = true },

                // sports_manager
                new RolePermission { RoleCode = "sports_manager", PermissionCode = "identity.users.summary.read", Granted = true },
                new RolePermission { RoleCode = "sports_manager", PermissionCode = "identity.users.lookup", Granted = true },

                // communications_staff
                new RolePermission { RoleCode = "communications_staff", PermissionCode = "identity.users.lookup", Granted = true }
            };

            await context.RolePermissions.AddRangeAsync(matrix, cancellationToken);
        }

        private static async Task SeedPersonasAsync(G1IdentityDbContext context, CancellationToken cancellationToken)
        {
            if (await context.MockPersonas.AnyAsync(cancellationToken))
            {
                return;
            }

            var now = DateTimeOffset.UtcNow;
            var personas = new List<(MockPersona Persona, string? RoleCode)>
            {
                (new MockPersona { Key = "admin.club", UserId = "usr_admin_club", DisplayName = "Admin G1", Email = "admin@mock.g1.com", EmailVerified = true, PhoneVerified = true, FixtureVersion = FixtureVersion, AuthenticationMethods = { "pwd", "mock_persona" } }, "club_admin"),
                (new MockPersona { Key = "sports.manager", UserId = "usr_sports_manager", DisplayName = "Gestor Deportivo", Email = "sports@mock.g1.com", EmailVerified = true, PhoneVerified = true, FixtureVersion = FixtureVersion, AuthenticationMethods = { "pwd", "mock_persona" } }, "sports_manager"),
                (new MockPersona { Key = "coach.ana", UserId = "usr_coach_ana", DisplayName = "Ana Entrenadora", Email = "ana.coach@mock.g1.com", EmailVerified = true, PhoneVerified = true, FixtureVersion = FixtureVersion, AuthenticationMethods = { "pwd", "mock_persona" } }, "coach"),
                (new MockPersona { Key = "player.minor", UserId = "usr_player_minor", DisplayName = "Jugador Menor", Email = "minor@mock.g1.com", EmailVerified = true, PhoneVerified = true, IsMinor = true, FixtureVersion = FixtureVersion, AuthenticationMethods = { "pwd", "mock_persona" }, Labels = { "minor" } }, "player"),
                (new MockPersona { Key = "member.active", UserId = "usr_member_active", DisplayName = "Socio Activo", Email = "member@mock.g1.com", EmailVerified = true, PhoneVerified = true, FixtureVersion = FixtureVersion, AuthenticationMethods = { "pwd", "mock_persona" } }, "member"),
                (new MockPersona { Key = "member.phone-unverified", UserId = "usr_member_unverified_phone", DisplayName = "Socio Teléfono Pendiente", Email = "unverified.phone@mock.g1.com", EmailVerified = true, PhoneVerified = false, Phone = "600000001", FixtureVersion = FixtureVersion, AuthenticationMethods = { "pwd", "mock_persona" }, Labels = { "unverified" } }, "member"),
                (new MockPersona { Key = "tutor.familiar", UserId = "usr_tutor_familiar", DisplayName = "Tutor Familiar", Email = "tutor@mock.g1.com", EmailVerified = true, PhoneVerified = true, FixtureVersion = FixtureVersion, AuthenticationMethods = { "pwd", "mock_persona" } }, "tutor"),
                (new MockPersona { Key = "sponsor.acme", UserId = "usr_sponsor_acme", DisplayName = "Patrocinios ACME", Email = "sponsor@mock.g1.com", EmailVerified = true, PhoneVerified = true, FixtureVersion = FixtureVersion, AuthenticationMethods = { "pwd", "mock_persona" } }, "sponsor"),
                (new MockPersona { Key = "supporter.fan", UserId = "usr_supporter_fan", DisplayName = "Simpatizante Fan", Email = "fan@mock.g1.com", EmailVerified = true, PhoneVerified = true, FixtureVersion = FixtureVersion, AuthenticationMethods = { "pwd", "mock_persona" } }, "supporter"),
                (new MockPersona { Key = "kit.staff", UserId = "usr_kit_staff", DisplayName = "Encargado Material", Email = "kit@mock.g1.com", EmailVerified = true, PhoneVerified = true, FixtureVersion = FixtureVersion, AuthenticationMethods = { "pwd", "mock_persona" } }, "kit_staff"),
                (new MockPersona { Key = "comms.staff", UserId = "usr_comms_staff", DisplayName = "Prensa Club", Email = "comms@mock.g1.com", EmailVerified = true, PhoneVerified = true, FixtureVersion = FixtureVersion, AuthenticationMethods = { "pwd", "mock_persona" } }, "communications_staff"),
                (new MockPersona { Key = "user.no-role", UserId = "usr_no_role", DisplayName = "Usuario Sin Rol", Email = "no.role@mock.g1.com", EmailVerified = true, PhoneVerified = true, FixtureVersion = FixtureVersion, AuthenticationMethods = { "pwd", "mock_persona" } }, null),
                (new MockPersona { Key = "user.unverified", UserId = "usr_unverified_email", DisplayName = "Usuario Email Pendiente", Email = "unverified.email@mock.g1.com", EmailVerified = false, PhoneVerified = true, FixtureVersion = FixtureVersion, AuthenticationMethods = { "pwd", "mock_persona" }, Labels = { "unverified" } }, "member"),
                (new MockPersona { Key = "user.inactive", UserId = "usr_inactive", DisplayName = "Usuario Inactivo", Email = "inactive@mock.g1.com", EmailVerified = true, PhoneVerified = true, Status = PersonaStatus.Inactive, FixtureVersion = FixtureVersion, AuthenticationMethods = { "pwd", "mock_persona" } }, "member"),
                (new MockPersona { Key = "user.locked", UserId = "usr_locked", DisplayName = "Usuario Bloqueado", Email = "locked@mock.g1.com", EmailVerified = true, PhoneVerified = true, Status = PersonaStatus.Locked, FixtureVersion = FixtureVersion, AuthenticationMethods = { "pwd", "mock_persona" } }, "member")
            };

            foreach (var item in personas)
            {
                await context.MockPersonas.AddAsync(item.Persona, cancellationToken);

                if (item.RoleCode != null)
                {
                    var assignment = new RoleAssignment
                    {
                        AssignmentId = $"asg_{item.Persona.UserId}_{item.RoleCode}",
                        UserId = item.Persona.UserId,
                        RoleCode = item.RoleCode,
                        GrantedBy = "system",
                        GrantedAt = now,
                        Reason = "Seeding inicial del mock"
                    };
                    await context.RoleAssignments.AddAsync(assignment, cancellationToken);
                }
            }
        }
    }
}
