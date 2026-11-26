namespace G1.Identity.Mock.Models
{
    /// <summary>
    /// Resultado observable al intentar revocar una sesión.
    /// </summary>
    internal enum SessionMutationResult
    {
        /// <summary>
        /// La sesión fue revocada en esta operación.
        /// </summary>
        Revoked = 0,

        /// <summary>
        /// La sesión ya no estaba activa; la operación es idempotente.
        /// </summary>
        Unchanged = 1,

        /// <summary>
        /// La sesión no existe o pertenece a otra persona.
        /// </summary>
        NotFound = 2,
    }
}
