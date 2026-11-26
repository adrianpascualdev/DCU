namespace G1.Identity.Mock.Models
{
    /// <summary>
    /// Evento de auditoría minimizado, sin tokens ni datos personales.
    /// </summary>
    internal sealed record AuditEvent(
        string EventType,
        string ActorUserId,
        string TargetId,
        string Result,
        string CorrelationId,
        DateTimeOffset OccurredAt);
}
