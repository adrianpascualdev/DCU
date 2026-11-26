namespace G1.Identity.Mock.Models
{
    /// <summary>
    /// Página de sesiones propias publicada por la API.
    /// </summary>
    internal sealed record SessionListResponse(
        IReadOnlyList<SessionResponse> Items,
        int Page,
        int PageSize,
        int Total);
}
