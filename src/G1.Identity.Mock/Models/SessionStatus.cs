namespace G1.Identity.Mock.Models
{
    /// <summary>
    /// Estado efectivo de una sesión del mock.
    /// </summary>
    internal enum SessionStatus
    {
        /// <summary>
        /// La sesión puede utilizarse y renovarse.
        /// </summary>
        Active = 0,

        /// <summary>
        /// La sesión fue revocada explícitamente.
        /// </summary>
        Revoked = 1,

        /// <summary>
        /// La sesión agotó su tiempo de inactividad o duración absoluta.
        /// </summary>
        Expired = 2,
    }
}
