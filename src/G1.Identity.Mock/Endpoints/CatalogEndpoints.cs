using G1.Identity.Mock.Extensions;
using G1.Identity.Mock.Models;

namespace G1.Identity.Mock.Endpoints
{
    /// <summary>
    /// Publica los catálogos versionados de roles y permisos propiedad de G1.
    /// </summary>
    internal sealed class CatalogEndpointDefinition : IEndpointDefinition
    {
        private static readonly CatalogResponse<RoleDefinition> RoleCatalog = new(
            Constants.Catalog.Version,
            provisional: true,
            new RoleDefinition[]
            {
                new(Constants.Roles.ClubAdmin, @"Administrador del club", @"Administración de identidad y configuración de G1.", assignable: true),
                new(Constants.Roles.SportsManager, @"Gestor deportivo", @"Coordinación deportiva; sus permisos de G2 los define G2.", assignable: true),
                new(Constants.Roles.Coach, @"Entrenador", @"Entrenador de equipo o categoría.", assignable: true),
                new(Constants.Roles.Player, @"Deportista", @"Deportista, incluido menor de edad.", assignable: true),
                new(Constants.Roles.Tutor, @"Familia o tutor", @"Tutor legal o familiar de un deportista.", assignable: true),
                new(Constants.Roles.Member, @"Socio", @"Socio del club.", assignable: true),
                new(Constants.Roles.Sponsor, @"Patrocinador", @"Patrocinador del club.", assignable: true),
                new(Constants.Roles.Supporter, @"Simpatizante", @"Simpatizante sin permisos de identidad.", assignable: true),
                new(Constants.Roles.KitStaff, @"Personal de material", @"Equipaciones y material, coordinado con G4.", assignable: true),
                new(Constants.Roles.CommunicationsStaff, @"Personal de comunicación", @"Comunicaciones, coordinado con G4.", assignable: true),
            });

        private static readonly CatalogResponse<PermissionDefinition> PermissionCatalog = new(
            Constants.Catalog.Version,
            provisional: true,
            new PermissionDefinition[]
            {
                new(Constants.Permissions.UserSummaryRead, @"Leer resumen de usuarios", Constants.Catalog.OwnerModule, @"Consultar el resumen mínimo de otro usuario."),
                new(Constants.Permissions.UserLookup, @"Resolver usuarios por lote", Constants.Catalog.OwnerModule, @"Resolver hasta 100 identificadores por petición."),
                new(Constants.Permissions.IdentityAdmin, @"Administrar identidad", Constants.Catalog.OwnerModule, @"Reservado para operaciones administrativas futuras; no habilita endpoints de fase A."),
            });

        /// <inheritdoc />
        public void RegisterEndpoints(WebApplication app)
        {
            ArgumentNullException.ThrowIfNull(app);

            app.MapGet(Constants.Paths.RoleCatalog, GetRoles)
                .WithName(@"listRoles")
                .WithTags(@"Catalog")
                .WithSummary(@"Catálogo de roles")
                .WithDescription(@"Publica los diez códigos de rol comunes con su etiqueta y descripción.")
                .RequireAuthorization()
                .RequireRateLimiting(Constants.Policies.Summary);

            app.MapGet(Constants.Paths.PermissionCatalog, GetPermissions)
                .WithName(@"listPermissions")
                .WithTags(@"Catalog")
                .WithSummary(@"Catálogo de permisos")
                .WithDescription(@"Publica únicamente los permisos cuya evaluación pertenece a G1.")
                .RequireAuthorization()
                .RequireRateLimiting(Constants.Policies.Summary);
        }

        private static IResult GetRoles(HttpContext context, CancellationToken cancellationToken)
        {
            return GetCatalog(context, RoleCatalog, Constants.Catalog.RolesEntityTag, cancellationToken);
        }

        private static IResult GetPermissions(HttpContext context, CancellationToken cancellationToken)
        {
            return GetCatalog(context, PermissionCatalog, Constants.Catalog.PermissionsEntityTag, cancellationToken);
        }

        private static IResult GetCatalog<TItem>(
            HttpContext context,
            CatalogResponse<TItem> catalog,
            string entityTag,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            context.Response.Headers[Constants.Headers.ETag] = entityTag;

            var candidates = context.Request.Headers[Constants.Headers.IfNoneMatch]
                .SelectMany(value => (value ?? string.Empty).Split(','))
                .Select(value => value.Trim());
            if (candidates.Any(value => value == @"*"
                || string.Equals(value, entityTag, StringComparison.Ordinal)
                || string.Equals(value, @"W/" + entityTag, StringComparison.Ordinal)))
            {
                return TypedResults.StatusCode(StatusCodes.Status304NotModified);
            }

            return TypedResults.Ok(catalog);
        }
    }
}
