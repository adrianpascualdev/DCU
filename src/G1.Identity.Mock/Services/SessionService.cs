using System.Collections.Concurrent;

using G1.Identity.Mock.Abstractions;
using G1.Identity.Mock.Models;
using Microsoft.Extensions.Options;
using SessionSettings = G1.Identity.Mock.Settings.SessionOptions;

namespace G1.Identity.Mock.Services
{
    /// <inheritdoc />
    internal sealed class SessionService : ISessionService
    {
        private readonly ConcurrentDictionary<string, SessionState> _sessions = new(StringComparer.Ordinal);
        private readonly ConcurrentDictionary<string, string> _refreshTokenOwners = new(StringComparer.Ordinal);
        private readonly TimeProvider _timeProvider;
        private readonly SessionSettings _options;

        /// <summary>
        /// Inicializa el servicio de sesiones en memoria.
        /// </summary>
        public SessionService(IOptions<SessionSettings> options, TimeProvider timeProvider)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(timeProvider);
            _options = options.Value;
            _timeProvider = timeProvider;
        }

        /// <inheritdoc />
        public SessionSnapshot Create(
            string sessionId,
            string userId,
            string clientId,
            IReadOnlyList<string> authenticationMethods,
            string deviceLabel)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);
            ArgumentException.ThrowIfNullOrWhiteSpace(userId);
            ArgumentException.ThrowIfNullOrWhiteSpace(clientId);
            ArgumentNullException.ThrowIfNull(authenticationMethods);
            ArgumentException.ThrowIfNullOrWhiteSpace(deviceLabel);

            var now = _timeProvider.GetUtcNow();
            var state = new SessionState(
                sessionId,
                userId,
                clientId,
                now,
                now.Add(_options.IdleTimeout),
                now.Add(_options.AbsoluteTimeout),
                authenticationMethods.ToArray(),
                deviceLabel);
            if (!_sessions.TryAdd(sessionId, state))
            {
                throw new InvalidOperationException(@"El identificador de sesión ya existe.");
            }

            return state.ToSnapshot();
        }

        /// <inheritdoc />
        public IReadOnlyList<SessionSnapshot> ListActive(string userId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userId);
            var now = _timeProvider.GetUtcNow();

            return _sessions.Values
                .Where(state => string.Equals(state.UserId, userId, StringComparison.Ordinal))
                .Select(state => state.Read(now))
                .Where(snapshot => snapshot.Status == SessionStatus.Active)
                .OrderByDescending(snapshot => snapshot.LastSeenAt)
                .ToArray();
        }

        /// <inheritdoc />
        public bool Touch(string sessionId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);
            return _sessions.TryGetValue(sessionId, out var state)
                && state.Touch(_timeProvider.GetUtcNow(), _options.IdleTimeout);
        }

        /// <inheritdoc />
        public SessionMutationResult Revoke(string userId, string sessionId, string reason)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userId);
            ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);
            ArgumentException.ThrowIfNullOrWhiteSpace(reason);

            if (!_sessions.TryGetValue(sessionId, out var state)
                || !string.Equals(state.UserId, userId, StringComparison.Ordinal))
            {
                return SessionMutationResult.NotFound;
            }

            return state.Revoke(_timeProvider.GetUtcNow(), reason);
        }

        /// <inheritdoc />
        public int RevokeAll(string userId, string currentSessionId, bool keepCurrentSession, string reason)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userId);
            ArgumentException.ThrowIfNullOrWhiteSpace(currentSessionId);
            ArgumentException.ThrowIfNullOrWhiteSpace(reason);
            var now = _timeProvider.GetUtcNow();
            var changed = 0;

            foreach (var state in _sessions.Values.Where(
                         state => string.Equals(state.UserId, userId, StringComparison.Ordinal)))
            {
                if (keepCurrentSession
                    && string.Equals(state.SessionId, currentSessionId, StringComparison.Ordinal))
                {
                    continue;
                }

                if (state.Revoke(now, reason) == SessionMutationResult.Revoked)
                {
                    changed++;
                }
            }

            return changed;
        }

        /// <inheritdoc />
        public bool LinkRefreshToken(string sessionId, string tokenId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);
            ArgumentException.ThrowIfNullOrWhiteSpace(tokenId);
            if (!_refreshTokenOwners.TryAdd(tokenId, sessionId))
            {
                return false;
            }

            if (_sessions.TryGetValue(sessionId, out var state)
                && state.LinkRefreshToken(tokenId, _timeProvider.GetUtcNow()))
            {
                return true;
            }

            _refreshTokenOwners.TryRemove(tokenId, out _);
            return false;
        }

        /// <inheritdoc />
        public bool RotateRefreshToken(string sessionId, string previousTokenId, string nextTokenId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);
            ArgumentException.ThrowIfNullOrWhiteSpace(previousTokenId);
            ArgumentException.ThrowIfNullOrWhiteSpace(nextTokenId);
            if (!_refreshTokenOwners.TryGetValue(previousTokenId, out var owner)
                || !string.Equals(owner, sessionId, StringComparison.Ordinal)
                || !_refreshTokenOwners.TryAdd(nextTokenId, sessionId))
            {
                return false;
            }

            if (_sessions.TryGetValue(sessionId, out var state)
                && state.RotateRefreshToken(previousTokenId, nextTokenId, _timeProvider.GetUtcNow()))
            {
                return true;
            }

            _refreshTokenOwners.TryRemove(nextTokenId, out _);
            return false;
        }

        /// <inheritdoc />
        public bool IsRefreshTokenActive(string sessionId, string tokenId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);
            ArgumentException.ThrowIfNullOrWhiteSpace(tokenId);
            return _refreshTokenOwners.TryGetValue(tokenId, out var owner)
                && string.Equals(owner, sessionId, StringComparison.Ordinal)
                && _sessions.TryGetValue(sessionId, out var state)
                && state.IsRefreshTokenActive(tokenId, _timeProvider.GetUtcNow());
        }

        private sealed class SessionState
        {
            private readonly object _lock = new();
            private readonly HashSet<string> _activeRefreshTokens = new(StringComparer.Ordinal);
            private readonly HashSet<string> _usedRefreshTokens = new(StringComparer.Ordinal);
            private DateTimeOffset _expiresAt;
            private DateTimeOffset _lastSeenAt;
            private DateTimeOffset? _revokedAt;
            private string? _revocationReason;
            private SessionStatus _status;

            internal SessionState(
                string sessionId,
                string userId,
                string clientId,
                DateTimeOffset createdAt,
                DateTimeOffset expiresAt,
                DateTimeOffset absoluteExpiresAt,
                IReadOnlyList<string> authenticationMethods,
                string deviceLabel)
            {
                SessionId = sessionId;
                UserId = userId;
                ClientId = clientId;
                CreatedAt = createdAt;
                _lastSeenAt = createdAt;
                _expiresAt = expiresAt < absoluteExpiresAt ? expiresAt : absoluteExpiresAt;
                AbsoluteExpiresAt = absoluteExpiresAt;
                AuthenticationMethods = authenticationMethods;
                DeviceLabel = deviceLabel;
                _status = SessionStatus.Active;
            }

            internal string SessionId { get; }

            internal string UserId { get; }

            private string ClientId { get; }

            private DateTimeOffset CreatedAt { get; }

            private DateTimeOffset AbsoluteExpiresAt { get; }

            private IReadOnlyList<string> AuthenticationMethods { get; }

            private string DeviceLabel { get; }

            internal SessionSnapshot Read(DateTimeOffset now)
            {
                lock (_lock)
                {
                    ExpireIfNeeded(now);
                    return ToSnapshotUnsafe();
                }
            }

            internal SessionSnapshot ToSnapshot()
            {
                lock (_lock)
                {
                    return ToSnapshotUnsafe();
                }
            }

            internal bool Touch(DateTimeOffset now, TimeSpan idleTimeout)
            {
                lock (_lock)
                {
                    ExpireIfNeeded(now);
                    if (_status != SessionStatus.Active)
                    {
                        return false;
                    }

                    _lastSeenAt = now;
                    var idleExpiry = now.Add(idleTimeout);
                    _expiresAt = idleExpiry < AbsoluteExpiresAt ? idleExpiry : AbsoluteExpiresAt;
                    return true;
                }
            }

            internal SessionMutationResult Revoke(DateTimeOffset now, string reason)
            {
                lock (_lock)
                {
                    ExpireIfNeeded(now);
                    if (_status != SessionStatus.Active)
                    {
                        return SessionMutationResult.Unchanged;
                    }

                    _status = SessionStatus.Revoked;
                    _revokedAt = now;
                    _revocationReason = reason;
                    _activeRefreshTokens.Clear();
                    return SessionMutationResult.Revoked;
                }
            }

            internal bool LinkRefreshToken(string tokenId, DateTimeOffset now)
            {
                lock (_lock)
                {
                    ExpireIfNeeded(now);
                    return _status == SessionStatus.Active
                        && _usedRefreshTokens.Add(tokenId)
                        && _activeRefreshTokens.Add(tokenId);
                }
            }

            internal bool RotateRefreshToken(string previousTokenId, string nextTokenId, DateTimeOffset now)
            {
                lock (_lock)
                {
                    ExpireIfNeeded(now);
                    if (_status != SessionStatus.Active
                        || string.Equals(previousTokenId, nextTokenId, StringComparison.Ordinal)
                        || !_activeRefreshTokens.Contains(previousTokenId)
                        || !_usedRefreshTokens.Add(nextTokenId))
                    {
                        return false;
                    }

                    _activeRefreshTokens.Remove(previousTokenId);
                    _activeRefreshTokens.Add(nextTokenId);
                    return true;
                }
            }

            internal bool IsRefreshTokenActive(string tokenId, DateTimeOffset now)
            {
                lock (_lock)
                {
                    ExpireIfNeeded(now);
                    return _status == SessionStatus.Active && _activeRefreshTokens.Contains(tokenId);
                }
            }

            private void ExpireIfNeeded(DateTimeOffset now)
            {
                if (_status == SessionStatus.Active && (now >= _expiresAt || now >= AbsoluteExpiresAt))
                {
                    _status = SessionStatus.Expired;
                    _activeRefreshTokens.Clear();
                }
            }

            private SessionSnapshot ToSnapshotUnsafe()
            {
                return new SessionSnapshot(
                    SessionId,
                    UserId,
                    ClientId,
                    CreatedAt,
                    _lastSeenAt,
                    _expiresAt,
                    AbsoluteExpiresAt,
                    _status,
                    AuthenticationMethods.ToArray(),
                    DeviceLabel,
                    _revokedAt,
                    _revocationReason);
            }
        }
    }
}
