namespace G1.Identity.Mock.Models
{
    /// <summary>
    /// Resumen público de una sesión sin direcciones IP ni secretos.
    /// </summary>
    internal sealed record SessionResponse(
        string SessionId,
        bool Current,
        DateTimeOffset CreatedAt,
        DateTimeOffset LastSeenAt,
        DateTimeOffset ExpiresAt,
        string ClientId,
        string DeviceLabel);
}
