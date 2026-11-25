using System.ComponentModel.DataAnnotations;
using G1.Identity.Mock.Resources;

namespace G1.Identity.Mock.Settings
{
    /// <summary>
    /// Duraciones de los tokens del mock definidas en D-004.
    /// </summary>
    public sealed class OpenIdConnectOptions
    {
        /// <summary>
        /// Duración del código de autorización de un solo uso.
        /// </summary>
        [Range(
            typeof(TimeSpan),
            @"00:00:01",
            @"01:00:00",
            ErrorMessageResourceType = typeof(IdentityMessages),
            ErrorMessageResourceName = nameof(IdentityMessages.LifetimeRange))]
        public TimeSpan AuthorizationCode { get; init; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Duración del access token.
        /// </summary>
        [Range(
            typeof(TimeSpan),
            @"00:00:01",
            @"01:00:00",
            ErrorMessageResourceType = typeof(IdentityMessages),
            ErrorMessageResourceName = nameof(IdentityMessages.LifetimeRange))]
        public TimeSpan AccessToken { get; init; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Duración del ID token.
        /// </summary>
        [Range(
            typeof(TimeSpan),
            @"00:00:01",
            @"01:00:00",
            ErrorMessageResourceType = typeof(IdentityMessages),
            ErrorMessageResourceName = nameof(IdentityMessages.LifetimeRange))]
        public TimeSpan IdentityToken { get; init; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Duración del refresh token rotativo.
        /// </summary>
        [Range(
            typeof(TimeSpan),
            @"00:01:00",
            @"24:00:00",
            ErrorMessageResourceType = typeof(IdentityMessages),
            ErrorMessageResourceName = nameof(IdentityMessages.LifetimeRange))]
        public TimeSpan RefreshToken { get; init; } = TimeSpan.FromHours(8);

        /// <summary>
        /// Tolerancia máxima aplicada a <c>exp</c>, <c>nbf</c> e <c>iat</c>.
        /// </summary>
        [Range(
            typeof(TimeSpan),
            @"00:00:00",
            @"00:02:00",
            ErrorMessageResourceType = typeof(IdentityMessages),
            ErrorMessageResourceName = nameof(IdentityMessages.ClockSkewRange))]
        public TimeSpan ClockSkew { get; init; } = TimeSpan.FromMinutes(2);
    }
}
