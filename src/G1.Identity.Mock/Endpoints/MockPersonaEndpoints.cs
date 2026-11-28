using System.Globalization;
using G1.Identity.Mock.Fixtures;
using G1.Identity.Mock.Persistence;
using G1.Identity.Mock.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;

namespace G1.Identity.Mock.Endpoints
{
    /// <summary>
    /// Publica los endpoints del mock para consultar personas ficticias y restaurar fixtures
    /// exclusivamente en los entornos de desarrollo y pruebas.
    /// </summary>
    public static class MockPersonaEndpoints
    {
        private const string MockPersonasPath = @"/api/v1/identity/mock/personas";
        private const string MockResetPath = @"/api/v1/identity/mock/reset";
        private const string ListMockPersonasOperationName = @"listMockPersonas";
        private const string ResetMockOperationName = @"resetMock";
        private const string MockTag = @"Mock";
        private const string PageQueryParameter = @"page";
        private const string PageSizeQueryParameter = @"pageSize";
        private const string DefaultFixtureVersion = @"v1.0.0";
        private const string ActiveStatus = @"active";
        private const string InactiveStatus = @"inactive";
        private const string LockedStatus = @"locked";

        private static readonly SemaphoreSlim _resetLock = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Registra las rutas <c>GET /api/v1/identity/mock/personas</c> y
        /// <c>POST /api/v1/identity/mock/reset</c> en el enrutador de la aplicación.
        /// </summary>
        /// <param name="endpoints">Constructor de rutas de la aplicación.</param>
        /// <returns>El mismo constructor de rutas para encadenamiento fluido.</returns>
        /// <exception cref="ArgumentNullException">
        /// Se produce cuando <paramref name="endpoints"/> es nulo.
        /// </exception>
        public static IEndpointRouteBuilder MapMockPersonaEndpoints(this IEndpointRouteBuilder endpoints)
        {
            ArgumentNullException.ThrowIfNull(endpoints);

            endpoints.MapGet(MockPersonasPath, ListMockPersonasAsync)
                .AllowAnonymous()
                .WithName(ListMockPersonasOperationName)
                .WithTags(MockTag)
                .WithSummary(@"Personas ficticias disponibles")
                .WithDescription(
                    @"Solo en desarrollo y pruebas. Publica las personas, sus roles, permisos y estados para que las pruebas sean deterministas. Fuera de ese entorno devuelve 404.")
                .Produces<MockPersonaListResponse>(StatusCodes.Status200OK, Constants.ContentTypes.Json)
                .ProducesProblem(StatusCodes.Status400BadRequest, Constants.ContentTypes.ProblemJson)
                .ProducesValidationProblem(StatusCodes.Status400BadRequest, Constants.ContentTypes.ProblemJson)
                .ProducesProblem(StatusCodes.Status404NotFound, Constants.ContentTypes.ProblemJson);

            endpoints.MapPost(MockResetPath, ResetMockAsync)
                .AllowAnonymous()
                .DisableAntiforgery()
                .WithName(ResetMockOperationName)
                .WithTags(MockTag)
                .WithSummary(@"Restaurar fixtures")
                .WithDescription(
                    @"Solo en desarrollo y pruebas. Restaura personas, sesiones y tokens al estado inicial de forma idempotente. Fuera de ese entorno devuelve 404.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound, Constants.ContentTypes.ProblemJson);

            return endpoints;
        }

        private static async Task<IResult> ListMockPersonasAsync(
            HttpContext httpContext,
            IHostEnvironment environment,
            G1IdentityDbContext dbContext,
            CancellationToken cancellationToken)
        {
            if (!IsMockEnvironment(environment))
            {
                return await CreateProblemResultAsync(httpContext, Constants.ErrorCodes.UserNotFound, cancellationToken);
            }

            if (!TryParsePagination(httpContext.Request.Query, out var page, out var pageSize))
            {
                return await CreateProblemResultAsync(httpContext, Constants.ErrorCodes.RequestInvalid, cancellationToken);
            }

            var total = await dbContext.MockPersonas
                .AsNoTracking()
                .CountAsync(cancellationToken);

            var fixtureVersion = await dbContext.MockPersonas
                .AsNoTracking()
                .OrderBy(persona => persona.Key)
                .Select(persona => persona.FixtureVersion)
                .FirstOrDefaultAsync(cancellationToken)
                ?? DefaultFixtureVersion;

            var skipCount = ((long)page - 1L) * pageSize;
            if (skipCount >= total)
            {
                return TypedResults.Ok(new MockPersonaListResponse
                {
                    FixtureVersion = fixtureVersion,
                    Items = Array.Empty<MockPersonaItemResponse>(),
                    Page = page,
                    PageSize = pageSize,
                    Total = total,
                });
            }

            var personas = await dbContext.MockPersonas
                .AsNoTracking()
                .OrderBy(persona => persona.Key)
                .Skip((int)skipCount)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var userIds = personas
                .Select(persona => persona.UserId)
                .ToList();

            var activeAssignments = await dbContext.RoleAssignments
                .AsNoTracking()
                .Where(assignment => userIds.Contains(assignment.UserId) && assignment.RevokedAt == null)
                .ToListAsync(cancellationToken);

            var rolesByUser = activeAssignments
                .GroupBy(assignment => assignment.UserId, StringComparer.Ordinal)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .Select(assignment => assignment.RoleCode)
                        .Distinct(StringComparer.Ordinal)
                        .OrderBy(roleCode => roleCode, StringComparer.Ordinal)
                        .ToArray(),
                    StringComparer.Ordinal);

            var distinctRoleCodes = rolesByUser.Values
                .SelectMany(roleCodes => roleCodes)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            var grantedRolePermissions = distinctRoleCodes.Count == 0
                ? new List<RolePermission>()
                : await dbContext.RolePermissions
                    .AsNoTracking()
                    .Where(rolePermission => rolePermission.Granted && distinctRoleCodes.Contains(rolePermission.RoleCode))
                    .ToListAsync(cancellationToken);

            var permissionsByRole = grantedRolePermissions
                .GroupBy(rolePermission => rolePermission.RoleCode, StringComparer.Ordinal)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .Select(rolePermission => rolePermission.PermissionCode)
                        .Distinct(StringComparer.Ordinal)
                        .ToArray(),
                    StringComparer.Ordinal);

            var items = personas
                .Select(persona => BuildPersonaItem(persona, rolesByUser, permissionsByRole))
                .ToArray();

            var response = new MockPersonaListResponse
            {
                FixtureVersion = fixtureVersion,
                Items = items,
                Page = page,
                PageSize = pageSize,
                Total = total,
            };

            return TypedResults.Ok(response);
        }

        private static async Task<IResult> ResetMockAsync(
            HttpContext httpContext,
            IHostEnvironment environment,
            G1IdentityDbContext dbContext,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken)
        {
            if (!IsMockEnvironment(environment))
            {
                return await CreateProblemResultAsync(httpContext, Constants.ErrorCodes.UserNotFound, cancellationToken);
            }

            await _resetLock.WaitAsync(cancellationToken);
            try
            {
                var sessionTokens = await dbContext.SessionTokens.ToListAsync(cancellationToken);
                var sessions = await dbContext.Sessions.ToListAsync(cancellationToken);
                var auditEvents = await dbContext.AuditEvents.ToListAsync(cancellationToken);
                var openIddictTokens = await dbContext.Set<OpenIddictEntityFrameworkCoreToken>().ToListAsync(cancellationToken);
                var openIddictAuthorizations = await dbContext.Set<OpenIddictEntityFrameworkCoreAuthorization>().ToListAsync(cancellationToken);
                var roleAssignments = await dbContext.RoleAssignments.ToListAsync(cancellationToken);
                var rolePermissions = await dbContext.RolePermissions.ToListAsync(cancellationToken);
                var personas = await dbContext.MockPersonas.ToListAsync(cancellationToken);
                var roles = await dbContext.Roles.ToListAsync(cancellationToken);
                var permissions = await dbContext.Permissions.ToListAsync(cancellationToken);
                var catalogSnapshots = await dbContext.CatalogSnapshots.ToListAsync(cancellationToken);

                dbContext.SessionTokens.RemoveRange(sessionTokens);
                dbContext.Sessions.RemoveRange(sessions);
                dbContext.AuditEvents.RemoveRange(auditEvents);
                dbContext.Set<OpenIddictEntityFrameworkCoreToken>().RemoveRange(openIddictTokens);
                dbContext.Set<OpenIddictEntityFrameworkCoreAuthorization>().RemoveRange(openIddictAuthorizations);
                dbContext.RoleAssignments.RemoveRange(roleAssignments);
                dbContext.RolePermissions.RemoveRange(rolePermissions);
                dbContext.MockPersonas.RemoveRange(personas);
                dbContext.Roles.RemoveRange(roles);
                dbContext.Permissions.RemoveRange(permissions);
                dbContext.CatalogSnapshots.RemoveRange(catalogSnapshots);

                await dbContext.SaveChangesAsync(cancellationToken);
                dbContext.ChangeTracker.Clear();

                await SeedData.EnsureSeedDataAsync(dbContext, cancellationToken);
            }
            finally
            {
                _resetLock.Release();
            }

            var logger = loggerFactory.CreateLogger(typeof(MockPersonaEndpoints).FullName ?? nameof(MockPersonaEndpoints));
            logger.LogInformation("Fixtures del mock de identidad restaurados al estado inicial.");

            return TypedResults.NoContent();
        }

        private static bool IsMockEnvironment(IHostEnvironment environment)
        {
            return environment.IsDevelopment()
                || environment.IsEnvironment(Constants.Environments.Testing);
        }

        private static bool TryParsePagination(IQueryCollection query, out int page, out int pageSize)
        {
            page = Constants.Sessions.DefaultPage;
            pageSize = Constants.Sessions.DefaultPageSize;

            if (!TryParseIntegerQueryParameter(query, PageQueryParameter, Constants.Sessions.DefaultPage, out page))
            {
                return false;
            }

            if (!TryParseIntegerQueryParameter(query, PageSizeQueryParameter, Constants.Sessions.DefaultPageSize, out pageSize))
            {
                return false;
            }

            return page >= 1 && pageSize >= 1 && pageSize <= Constants.Sessions.MaximumPageSize;
        }

        private static bool TryParseIntegerQueryParameter(
            IQueryCollection query,
            string parameterName,
            int defaultValue,
            out int value)
        {
            value = defaultValue;
            if (!query.TryGetValue(parameterName, out var rawValues))
            {
                return true;
            }

            if (rawValues.Count != 1)
            {
                return false;
            }

            var rawValue = rawValues[0];
            if (String.IsNullOrWhiteSpace(rawValue))
            {
                return false;
            }

            return Int32.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        private static MockPersonaItemResponse BuildPersonaItem(
            MockPersona persona,
            IReadOnlyDictionary<string, string[]> rolesByUser,
            IReadOnlyDictionary<string, string[]> permissionsByRole)
        {
            var roles = rolesByUser.TryGetValue(persona.UserId, out var assignedRoles)
                ? assignedRoles
                : Array.Empty<string>();

            var effectivePermissions = CalculateEffectivePermissions(persona, roles, permissionsByRole);

            return new MockPersonaItemResponse
            {
                Key = persona.Key,
                UserId = persona.UserId,
                DisplayName = persona.DisplayName,
                Email = persona.Email,
                EmailVerified = persona.EmailVerified,
                PhoneVerified = persona.PhoneVerified,
                Status = MapStatus(persona.Status),
                IsMinor = persona.IsMinor,
                Roles = roles,
                Permissions = effectivePermissions,
                AuthenticationMethods = persona.AuthenticationMethods.ToArray(),
                Labels = persona.Labels.ToArray(),
            };
        }

        private static string[] CalculateEffectivePermissions(
            MockPersona persona,
            IReadOnlyCollection<string> roles,
            IReadOnlyDictionary<string, string[]> permissionsByRole)
        {
            if (!persona.CanAuthorize() || !persona.EmailVerified || roles.Count == 0)
            {
                return Array.Empty<string>();
            }

            var effectivePermissions = new HashSet<string>(StringComparer.Ordinal);
            foreach (var roleCode in roles)
            {
                if (permissionsByRole.TryGetValue(roleCode, out var rolePermissions))
                {
                    foreach (var permissionCode in rolePermissions)
                    {
                        effectivePermissions.Add(permissionCode);
                    }
                }
            }

            return effectivePermissions
                .OrderBy(permissionCode => permissionCode, StringComparer.Ordinal)
                .ToArray();
        }

        private static string MapStatus(PersonaStatus status)
        {
            return status switch
            {
                PersonaStatus.Active => ActiveStatus,
                PersonaStatus.Inactive => InactiveStatus,
                PersonaStatus.Locked => LockedStatus,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(status),
                    status,
                    @"El estado de la persona del mock no es válido."),
            };
        }

        private static async Task<IResult> CreateProblemResultAsync(
            HttpContext httpContext,
            string errorCode,
            CancellationToken cancellationToken)
        {
            await ErrorMapping.WriteProblemAsync(httpContext, errorCode, cancellationToken);
            return TypedResults.Empty;
        }

        private sealed class MockPersonaListResponse
        {
            public string FixtureVersion { get; set; } = String.Empty;

            public IReadOnlyList<MockPersonaItemResponse> Items { get; set; } = Array.Empty<MockPersonaItemResponse>();

            public int Page { get; set; }

            public int PageSize { get; set; }

            public int Total { get; set; }
        }

        private sealed class MockPersonaItemResponse
        {
            public string Key { get; set; } = String.Empty;

            public string UserId { get; set; } = String.Empty;

            public string DisplayName { get; set; } = String.Empty;

            public string Email { get; set; } = String.Empty;

            public bool EmailVerified { get; set; }

            public bool PhoneVerified { get; set; }

            public string Status { get; set; } = String.Empty;

            public bool IsMinor { get; set; }

            public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();

            public IReadOnlyList<string> Permissions { get; set; } = Array.Empty<string>();

            public IReadOnlyList<string> AuthenticationMethods { get; set; } = Array.Empty<string>();

            public IReadOnlyList<string> Labels { get; set; } = Array.Empty<string>();
        }
    }
}
