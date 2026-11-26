using System.Text.Json;

using G1.Identity.Mock.Abstractions;
using G1.Identity.Mock.Exceptions;
using G1.Identity.Mock.Extensions;
using G1.Identity.Mock.Filters;
using G1.Identity.Mock.Models;

namespace G1.Identity.Mock.Endpoints
{
    /// <summary>
    /// Publica las operaciones de consulta, revocación y cierre de sesiones propias.
    /// </summary>
    internal sealed class SessionEndpointDefinition : IEndpointDefinition
    {
        /// <inheritdoc />
        public void RegisterEndpoints(WebApplication app)
        {
            ArgumentNullException.ThrowIfNull(app);

            app.MapGet(Constants.Paths.Sessions, ListSessionsAsync)
                .WithName(@"listSessions")
                .WithTags(@"Sessions")
                .RequireAuthorization()
                .RequireRateLimiting(Constants.Policies.Summary);

            app.MapDelete(Constants.Paths.SessionById, RevokeSessionAsync)
                .WithName(@"revokeSession")
                .WithTags(@"Sessions")
                .RequireAuthorization()
                .AddEndpointFilter<CookieAntiforgeryFilter>()
                .RequireRateLimiting(Constants.Policies.Summary);

            app.MapPost(Constants.Paths.RevokeAllSessions, RevokeAllSessionsAsync)
                .WithName(@"revokeAllSessions")
                .WithTags(@"Sessions")
                .RequireAuthorization()
                .AddEndpointFilter<CookieAntiforgeryFilter>()
                .RequireRateLimiting(Constants.Policies.Summary);

            app.MapPost(Constants.Paths.Logout, LogoutAsync)
                .WithName(@"logoutCurrentSession")
                .WithTags(@"Sessions")
                .RequireAuthorization()
                .AddEndpointFilter<CookieAntiforgeryFilter>()
                .RequireRateLimiting(Constants.Policies.Summary);
        }

        private static async Task ListSessionsAsync(
            HttpContext context,
            ISessionService sessionService,
            int page = Constants.Sessions.DefaultPage,
            int pageSize = Constants.Sessions.DefaultPageSize)
        {
            if (!TryGetIdentity(context, out var userId, out var currentSessionId))
            {
                await ErrorMapping.WriteProblemAsync(context, Constants.ErrorCodes.SessionInvalid, context.RequestAborted);
                return;
            }

            if (page < 1 || pageSize is < 1 or > Constants.Sessions.MaximumPageSize)
            {
                await ErrorMapping.WriteProblemAsync(context, Constants.ErrorCodes.RequestInvalid, context.RequestAborted);
                return;
            }

            var sessions = sessionService.ListActive(userId);
            var items = sessions
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(session => new SessionResponse(
                    session.SessionId,
                    string.Equals(session.SessionId, currentSessionId, StringComparison.Ordinal),
                    session.CreatedAt,
                    session.LastSeenAt,
                    session.ExpiresAt,
                    session.ClientId,
                    session.DeviceLabel))
                .ToArray();

            context.Response.Headers[Constants.Headers.CacheControl] = Constants.Headers.NoStore;
            await context.Response.WriteAsJsonAsync(
                new SessionListResponse(items, page, pageSize, sessions.Count),
                context.RequestAborted);
        }

        private static async Task RevokeSessionAsync(
            HttpContext context,
            string sessionId,
            ISessionService sessionService,
            IAuditService auditService)
        {
            if (!TryGetIdentity(context, out var userId, out _))
            {
                await ErrorMapping.WriteProblemAsync(context, Constants.ErrorCodes.SessionInvalid, context.RequestAborted);
                return;
            }

            var result = sessionService.Revoke(userId, sessionId, Constants.Sessions.UserRevokedReason);
            if (result == SessionMutationResult.NotFound)
            {
                await ErrorMapping.WriteProblemAsync(context, Constants.ErrorCodes.UserNotFound, context.RequestAborted);
                return;
            }

            if (result == SessionMutationResult.Revoked)
            {
                RecordRevocation(auditService, context, userId, sessionId);
            }

            context.Response.StatusCode = StatusCodes.Status204NoContent;
        }

        private static async Task RevokeAllSessionsAsync(
            HttpContext context,
            ISessionService sessionService,
            IAuditService auditService)
        {
            if (!TryGetIdentity(context, out var userId, out var currentSessionId))
            {
                await ErrorMapping.WriteProblemAsync(context, Constants.ErrorCodes.SessionInvalid, context.RequestAborted);
                return;
            }

            RevokeAllRequest request;
            try
            {
                request = context.Request.ContentLength is > 0 ||
                    (context.Request.ContentLength is null && context.Request.HasJsonContentType())
                    ? await context.Request.ReadFromJsonAsync<RevokeAllRequest>(context.RequestAborted)
                        ?? new RevokeAllRequest()
                    : new RevokeAllRequest();
            }
            catch (Exception exception) when (exception is JsonException or NotSupportedException)
            {
                await ErrorMapping.WriteProblemAsync(context, Constants.ErrorCodes.RequestInvalid, context.RequestAborted);
                return;
            }

            var changed = sessionService.RevokeAll(
                userId,
                currentSessionId,
                request.KeepCurrentSession,
                Constants.Sessions.UserRevokedReason);
            if (changed > 0)
            {
                RecordRevocation(auditService, context, userId, @"all_sessions");
            }

            context.Response.StatusCode = StatusCodes.Status204NoContent;
        }

        private static async Task LogoutAsync(
            HttpContext context,
            ISessionService sessionService,
            IAuditService auditService)
        {
            if (!TryGetIdentity(context, out var userId, out var currentSessionId))
            {
                await ErrorMapping.WriteProblemAsync(context, Constants.ErrorCodes.SessionInvalid, context.RequestAborted);
                return;
            }

            var result = sessionService.Revoke(userId, currentSessionId, Constants.Sessions.LogoutReason);
            if (result != SessionMutationResult.Revoked)
            {
                await ErrorMapping.WriteProblemAsync(context, Constants.ErrorCodes.SessionInvalid, context.RequestAborted);
                return;
            }

            if (result == SessionMutationResult.Revoked)
            {
                var correlationId = ProblemResponseWriter.GetTraceId(context);
                auditService.Record(Constants.AuditEvents.Logout, userId, currentSessionId, @"success", correlationId);
                auditService.Record(Constants.AuditEvents.TokenRevoked, userId, currentSessionId, @"success", correlationId);
            }

            context.Response.StatusCode = StatusCodes.Status204NoContent;
        }

        private static bool TryGetIdentity(HttpContext context, out string userId, out string sessionId)
        {
            userId = context.User.FindFirst(Constants.Claims.Subject)?.Value ?? string.Empty;
            sessionId = context.User.FindFirst(Constants.Claims.SessionId)?.Value ?? string.Empty;
            return !string.IsNullOrWhiteSpace(userId) && !string.IsNullOrWhiteSpace(sessionId);
        }

        private static void RecordRevocation(
            IAuditService auditService,
            HttpContext context,
            string userId,
            string sessionId)
        {
            var correlationId = ProblemResponseWriter.GetTraceId(context);
            auditService.Record(Constants.AuditEvents.SessionRevoked, userId, sessionId, @"success", correlationId);
            auditService.Record(Constants.AuditEvents.TokenRevoked, userId, sessionId, @"success", correlationId);
        }
    }
}
