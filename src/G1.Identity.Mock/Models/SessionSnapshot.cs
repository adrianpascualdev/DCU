namespace G1.Identity.Mock.Models
{
    /// <summary>
    /// Vista inmutable y minimizada de una sesión.
    /// </summary>
    internal sealed record SessionSnapshot(
        string SessionId,
        string UserId,
        string ClientId,
        DateTimeOffset CreatedAt,
        DateTimeOffset LastSeenAt,
        DateTimeOffset ExpiresAt,
        DateTimeOffset AbsoluteExpiresAt,
        SessionStatus Status,
        IReadOnlyList<string> AuthenticationMethods,
        string DeviceLabel,
        DateTimeOffset? RevokedAt,
        string? RevocationReason);
}
