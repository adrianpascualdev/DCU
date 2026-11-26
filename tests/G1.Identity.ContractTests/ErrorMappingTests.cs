using System.Text.Json;
using G1.Identity.Mock;
using G1.Identity.Mock.Endpoints;
using Microsoft.AspNetCore.Http;

namespace G1.Identity.ContractTests
{
    /// <summary>
    /// Verifica el mapeo centralizado de errores de negocio al contrato RFC 9457.
    /// </summary>
    public sealed class ErrorMappingTests
    {
        public static TheoryData<string, int> ApiErrors => new()
        {
            { Constants.ErrorCodes.SessionRequired, StatusCodes.Status401Unauthorized },
            { Constants.ErrorCodes.SessionInvalid, StatusCodes.Status401Unauthorized },
            { Constants.ErrorCodes.PermissionDenied, StatusCodes.Status403Forbidden },
            { Constants.ErrorCodes.UserNotFound, StatusCodes.Status404NotFound },
            { Constants.ErrorCodes.RequestInvalid, StatusCodes.Status400BadRequest },
            { Constants.ErrorCodes.RateLimited, StatusCodes.Status429TooManyRequests },
            { Constants.ErrorCodes.InternalError, StatusCodes.Status500InternalServerError },
        };

        [Theory]
        [MemberData(nameof(ApiErrors))]
        public void ApiErrorCodesMapToTheirDocumentedStatus(string errorCode, int expectedStatus)
        {
            Assert.Equal(expectedStatus, ErrorMapping.GetStatusCode(errorCode));
        }

        [Theory]
        [InlineData("invalid_request")]
        [InlineData("invalid_client")]
        [InlineData("invalid_grant")]
        [InlineData("access_denied")]
        public void OAuthCodesAreNotExposedAsBusinessApiCodes(string oauthCode)
        {
            Assert.Equal(StatusCodes.Status500InternalServerError, ErrorMapping.GetStatusCode(oauthCode));
            Assert.Equal(Constants.ErrorCodes.InternalError, ErrorMapping.NormalizeErrorCode(oauthCode));
        }

        [Fact]
        public async Task WriteProblemAsyncProducesTheDocumentedProblemJsonSchema()
        {
            var context = new DefaultHttpContext();
            context.Request.Path = "/api/v1/identity/me";
            context.Response.Body = new MemoryStream();
            context.Items[Constants.HttpItems.CorrelationId] = "correlacion-prueba-123";

            await ErrorMapping.WriteProblemAsync(
                context,
                Constants.ErrorCodes.SessionRequired,
                TestContext.Current.CancellationToken);

            context.Response.Body.Position = 0;
            using var problem = await JsonDocument.ParseAsync(
                context.Response.Body,
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
            Assert.StartsWith(
                Constants.ContentTypes.ProblemJson,
                context.Response.ContentType,
                StringComparison.OrdinalIgnoreCase);
            Assert.Equal(Constants.ErrorCodes.SessionRequired, problem.RootElement.GetProperty("code").GetString());
            Assert.Equal("correlacion-prueba-123", problem.RootElement.GetProperty("traceId").GetString());
            Assert.Equal("/api/v1/identity/me", problem.RootElement.GetProperty("instance").GetString());
            Assert.Equal(401, problem.RootElement.GetProperty("status").GetInt32());
            Assert.False(string.IsNullOrWhiteSpace(problem.RootElement.GetProperty("type").GetString()));
            Assert.False(string.IsNullOrWhiteSpace(problem.RootElement.GetProperty("title").GetString()));
            Assert.False(string.IsNullOrWhiteSpace(problem.RootElement.GetProperty("detail").GetString()));
        }

        [Fact]
        public async Task UnknownCodeIsSanitizedBeforeWritingTheResponse()
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            context.Items[Constants.HttpItems.CorrelationId] = "correlacion-prueba-456";

            await ErrorMapping.WriteProblemAsync(
                context,
                "identity.secret.internal_exception",
                TestContext.Current.CancellationToken);

            context.Response.Body.Position = 0;
            using var problem = await JsonDocument.ParseAsync(
                context.Response.Body,
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
            Assert.Equal(Constants.ErrorCodes.InternalError, problem.RootElement.GetProperty("code").GetString());
            Assert.DoesNotContain("secret", problem.RootElement.ToString(), StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("exception", problem.RootElement.ToString(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
