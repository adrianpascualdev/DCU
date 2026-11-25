using System.ComponentModel.DataAnnotations;
using G1.Identity.Mock.Resources;

namespace G1.Identity.Mock.Settings
{
    /// <summary>
    /// Tiempos de la sesión del mock definidos en D-004.
    /// </summary>
    public sealed class SessionOptions
    {
        /// <summary>
        /// Tiempo de inactividad que permite renovar la sesión.
        /// </summary>
        [Range(
            typeof(TimeSpan),
            @"00:01:00",
            @"48:00:00",
            ErrorMessageResourceType = typeof(IdentityMessages),
            ErrorMessageResourceName = nameof(IdentityMessages.SessionTimeoutRange))]
        public TimeSpan IdleTimeout { get; init; } = TimeSpan.FromMinutes(30);

        /// <summary>
        /// Duración absoluta de la sesión, no renovable.
        /// </summary>
        [Range(
            typeof(TimeSpan),
            @"00:01:00",
            @"48:00:00",
            ErrorMessageResourceType = typeof(IdentityMessages),
            ErrorMessageResourceName = nameof(IdentityMessages.SessionTimeoutRange))]
        public TimeSpan AbsoluteTimeout { get; init; } = TimeSpan.FromHours(12);
    }
}
