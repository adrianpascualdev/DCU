using G1.Identity.Mock;
using G1.Identity.Mock.Exceptions;
using G1.Identity.Mock.Resources;

namespace G1.Identity.ContractTests
{
    /// <summary>
    /// Comprueba que el catálogo de errores coincide con los códigos y tipos publicados en el contrato.
    /// </summary>
    public sealed class StableErrorCodesTests
    {
        public static TheoryData<string, string, string, string> ContractErrors => new()
        {
            {
                Constants.ErrorCodes.SessionRequired,
                Constants.ErrorTypes.SessionRequired,
                IdentityMessages.ErrorSessionRequiredTitle,
                IdentityMessages.ErrorSessionRequiredDetail
            },
            {
                Constants.ErrorCodes.SessionInvalid,
                Constants.ErrorTypes.SessionInvalid,
                IdentityMessages.ErrorSessionInvalidTitle,
                IdentityMessages.ErrorSessionInvalidDetail
            },
            {
                Constants.ErrorCodes.PermissionDenied,
                Constants.ErrorTypes.PermissionDenied,
                IdentityMessages.ErrorPermissionDeniedTitle,
                IdentityMessages.ErrorPermissionDeniedDetail
            },
            {
                Constants.ErrorCodes.UserNotFound,
                Constants.ErrorTypes.UserNotFound,
                IdentityMessages.ErrorUserNotFoundTitle,
                IdentityMessages.ErrorUserNotFoundDetail
            },
            {
                Constants.ErrorCodes.RequestInvalid,
                Constants.ErrorTypes.RequestInvalid,
                IdentityMessages.ErrorRequestInvalidTitle,
                IdentityMessages.ErrorRequestInvalidDetail
            },
            {
                Constants.ErrorCodes.RateLimited,
                Constants.ErrorTypes.RateLimited,
                IdentityMessages.ErrorRateLimitedTitle,
                IdentityMessages.ErrorRateLimitedDetail
            },
            {
                Constants.ErrorCodes.InternalError,
                Constants.ErrorTypes.InternalError,
                IdentityMessages.ErrorInternalTitle,
                IdentityMessages.ErrorInternalDetail
            },
        };

        [Fact]
        public void ContractDefinesSevenDistinctStableCodes()
        {
            var codes = new[]
            {
                Constants.ErrorCodes.SessionRequired,
                Constants.ErrorCodes.SessionInvalid,
                Constants.ErrorCodes.PermissionDenied,
                Constants.ErrorCodes.UserNotFound,
                Constants.ErrorCodes.RequestInvalid,
                Constants.ErrorCodes.RateLimited,
                Constants.ErrorCodes.InternalError,
            };

            Assert.Equal(7, codes.Distinct(StringComparer.Ordinal).Count());
            Assert.All(codes, code => Assert.StartsWith(@"identity.", code, StringComparison.Ordinal));
        }

        [Theory]
        [MemberData(nameof(ContractErrors))]
        public void CatalogMapsCodeToContractTypeTitleAndDetail(
            string code,
            string expectedType,
            string expectedTitle,
            string expectedDetail)
        {
            Assert.Equal(expectedType, ErrorCatalog.GetType(code));
            Assert.Equal(expectedTitle, ErrorCatalog.GetTitle(code));
            Assert.Equal(expectedDetail, ErrorCatalog.GetDetail(code));
            Assert.StartsWith(@"https://identity.example.test/errors/", expectedType, StringComparison.Ordinal);
        }

        [Theory]
        [InlineData(@"identity.session.required", @"https://identity.example.test/errors/session-required")]
        [InlineData(@"identity.session.invalid", @"https://identity.example.test/errors/session-invalid")]
        [InlineData(@"identity.permission.denied", @"https://identity.example.test/errors/permission-denied")]
        [InlineData(@"identity.user.not_found", @"https://identity.example.test/errors/user-not-found")]
        [InlineData(@"identity.request.invalid", @"https://identity.example.test/errors/request-invalid")]
        [InlineData(@"identity.rate_limited", @"https://identity.example.test/errors/rate-limited")]
        [InlineData(@"identity.internal_error", @"https://identity.example.test/errors/internal")]
        public void ErrorTypeUrisMatchThePublishedContract(string code, string expectedType)
        {
            Assert.Equal(expectedType, ErrorCatalog.GetType(code));
        }

        [Fact]
        public void UnknownCodeFallsBackToInternalErrorWithoutInternalDetails()
        {
            Assert.Equal(Constants.ErrorTypes.InternalError, ErrorCatalog.GetType(@"identity.desconocido"));
            Assert.Equal(IdentityMessages.ErrorInternalTitle, ErrorCatalog.GetTitle(@"identity.desconocido"));
            Assert.Equal(IdentityMessages.ErrorInternalDetail, ErrorCatalog.GetDetail(@"identity.desconocido"));
            Assert.DoesNotContain(@"Exception", IdentityMessages.ErrorInternalDetail, StringComparison.OrdinalIgnoreCase);
        }
    }
}
