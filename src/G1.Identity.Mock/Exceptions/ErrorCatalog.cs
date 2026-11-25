using G1.Identity.Mock.Resources;

namespace G1.Identity.Mock.Exceptions
{
    /// <summary>
    /// Traduce los códigos estables del contrato a sus textos localizados.
    /// </summary>
    internal static class ErrorCatalog
    {
        /// <summary>
        /// Obtiene el título RFC 9457 asociado al código de error.
        /// </summary>
        /// <param name="errorCode">Código estable del contrato.</param>
        /// <returns>El título localizado del error.</returns>
        internal static string GetTitle(string errorCode) => errorCode switch
        {
            Constants.ErrorCodes.SessionRequired => IdentityMessages.ErrorSessionRequiredTitle,
            Constants.ErrorCodes.SessionInvalid => IdentityMessages.ErrorSessionInvalidTitle,
            Constants.ErrorCodes.PermissionDenied => IdentityMessages.ErrorPermissionDeniedTitle,
            Constants.ErrorCodes.UserNotFound => IdentityMessages.ErrorUserNotFoundTitle,
            Constants.ErrorCodes.RequestInvalid => IdentityMessages.ErrorRequestInvalidTitle,
            Constants.ErrorCodes.RateLimited => IdentityMessages.ErrorRateLimitedTitle,
            _ => IdentityMessages.ErrorInternalTitle,
        };

        /// <summary>
        /// Obtiene el detalle RFC 9457 asociado al código de error.
        /// </summary>
        /// <param name="errorCode">Código estable del contrato.</param>
        /// <returns>El detalle localizado del error.</returns>
        internal static string GetDetail(string errorCode) => errorCode switch
        {
            Constants.ErrorCodes.SessionRequired => IdentityMessages.ErrorSessionRequiredDetail,
            Constants.ErrorCodes.SessionInvalid => IdentityMessages.ErrorSessionInvalidDetail,
            Constants.ErrorCodes.PermissionDenied => IdentityMessages.ErrorPermissionDeniedDetail,
            Constants.ErrorCodes.UserNotFound => IdentityMessages.ErrorUserNotFoundDetail,
            Constants.ErrorCodes.RequestInvalid => IdentityMessages.ErrorRequestInvalidDetail,
            Constants.ErrorCodes.RateLimited => IdentityMessages.ErrorRateLimitedDetail,
            _ => IdentityMessages.ErrorInternalDetail,
        };

        /// <summary>
        /// Obtiene el identificador URI del tipo RFC 9457 asociado al código de error.
        /// </summary>
        /// <param name="errorCode">Código estable del contrato.</param>
        /// <returns>El identificador URI documentado en el contrato.</returns>
        internal static string GetType(string errorCode) => errorCode switch
        {
            Constants.ErrorCodes.SessionRequired => Constants.ErrorTypes.SessionRequired,
            Constants.ErrorCodes.SessionInvalid => Constants.ErrorTypes.SessionInvalid,
            Constants.ErrorCodes.PermissionDenied => Constants.ErrorTypes.PermissionDenied,
            Constants.ErrorCodes.UserNotFound => Constants.ErrorTypes.UserNotFound,
            Constants.ErrorCodes.RequestInvalid => Constants.ErrorTypes.RequestInvalid,
            Constants.ErrorCodes.RateLimited => Constants.ErrorTypes.RateLimited,
            _ => Constants.ErrorTypes.InternalError,
        };
    }
}
