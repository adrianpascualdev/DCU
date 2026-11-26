using System.Collections.Concurrent;

using G1.Identity.Mock.Abstractions;
using G1.Identity.Mock.Models;

namespace G1.Identity.Mock.Services
{
    /// <inheritdoc />
    internal sealed class AuditService : IAuditService
    {
        private readonly ConcurrentQueue<AuditEvent> _events = new();
        private readonly TimeProvider _timeProvider;

        /// <summary>
        /// Inicializa el registro de auditoría con un proveedor de tiempo comprobable.
        /// </summary>
        public AuditService(TimeProvider timeProvider)
        {
            _timeProvider = timeProvider;
        }

        /// <inheritdoc />
        public void Record(string eventType, string actorUserId, string targetId, string result, string correlationId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(eventType);
            ArgumentException.ThrowIfNullOrWhiteSpace(actorUserId);
            ArgumentException.ThrowIfNullOrWhiteSpace(targetId);
            ArgumentException.ThrowIfNullOrWhiteSpace(result);
            ArgumentException.ThrowIfNullOrWhiteSpace(correlationId);

            _events.Enqueue(new AuditEvent(
                eventType,
                actorUserId,
                targetId,
                result,
                correlationId,
                _timeProvider.GetUtcNow()));
        }

        /// <inheritdoc />
        public IReadOnlyList<AuditEvent> List()
        {
            return _events.ToArray();
        }

        /// <inheritdoc />
        public void Reset()
        {
            _events.Clear();
        }
    }
}
