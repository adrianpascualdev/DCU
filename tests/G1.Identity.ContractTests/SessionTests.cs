using G1.Identity.Mock;
using G1.Identity.Mock.Models;
using G1.Identity.Mock.Services;
using Microsoft.Extensions.Options;
using SessionSettings = G1.Identity.Mock.Settings.SessionOptions;

namespace G1.Identity.ContractTests
{
    /// <summary>
    /// Verifica el ciclo de vida, la propiedad y la revocación de sesiones.
    /// </summary>
    public sealed class SessionTests
    {
        [Fact]
        public void RevokingOwnSessionAlsoRevokesItsRefreshTokensAndIsIdempotent()
        {
            var fixture = CreateFixture();
            fixture.Service.Create(@"ses_1", @"usr_1", @"cli_1", new[] { @"mock_persona" }, @"Navegador");
            Assert.True(fixture.Service.LinkRefreshToken(@"ses_1", @"rft_1"));

            Assert.Equal(
                SessionMutationResult.Revoked,
                fixture.Service.Revoke(@"usr_1", @"ses_1", Constants.Sessions.UserRevokedReason));
            Assert.False(fixture.Service.IsRefreshTokenActive(@"ses_1", @"rft_1"));
            Assert.Equal(
                SessionMutationResult.Unchanged,
                fixture.Service.Revoke(@"usr_1", @"ses_1", Constants.Sessions.UserRevokedReason));
        }

        [Fact]
        public void ForeignSessionIsReportedAsNotFound()
        {
            var fixture = CreateFixture();
            fixture.Service.Create(@"ses_1", @"usr_owner", @"cli_1", new[] { @"mock_persona" }, @"Navegador");

            var result = fixture.Service.Revoke(
                @"usr_other",
                @"ses_1",
                Constants.Sessions.UserRevokedReason);

            Assert.Equal(SessionMutationResult.NotFound, result);
        }

        [Fact]
        public void RevokeAllCanKeepCurrentSessionActive()
        {
            var fixture = CreateFixture();
            fixture.Service.Create(@"ses_current", @"usr_1", @"cli_1", new[] { @"mock_persona" }, @"Actual");
            fixture.Service.Create(@"ses_other", @"usr_1", @"cli_1", new[] { @"mock_persona" }, @"Otro");

            var changed = fixture.Service.RevokeAll(
                @"usr_1",
                @"ses_current",
                keepCurrentSession: true,
                Constants.Sessions.UserRevokedReason);

            Assert.Equal(1, changed);
            Assert.True(fixture.Service.Touch(@"ses_current"));
            Assert.False(fixture.Service.Touch(@"ses_other"));
        }

        [Fact]
        public void SessionExpiresAfterIdleTimeoutAndCannotBeTouched()
        {
            var fixture = CreateFixture();
            fixture.Service.Create(@"ses_1", @"usr_1", @"cli_1", new[] { @"mock_persona" }, @"Navegador");

            fixture.Time.Advance(TimeSpan.FromMinutes(31));

            Assert.False(fixture.Service.Touch(@"ses_1"));
            Assert.Empty(fixture.Service.ListActive(@"usr_1"));
        }

        [Fact]
        public void RevokedSessionIdentifierCannotBeReused()
        {
            var fixture = CreateFixture();
            fixture.Service.Create(@"ses_1", @"usr_1", @"cli_1", new[] { @"mock_persona" }, @"Navegador");
            fixture.Service.Revoke(@"usr_1", @"ses_1", Constants.Sessions.UserRevokedReason);

            Assert.Throws<InvalidOperationException>(
                () => fixture.Service.Create(
                    @"ses_1",
                    @"usr_1",
                    @"cli_1",
                    new[] { @"mock_persona" },
                    @"Navegador"));
        }

        private static SessionFixture CreateFixture()
        {
            var time = new TestTimeProvider(new DateTimeOffset(2026, 11, 26, 10, 0, 0, TimeSpan.Zero));
            var options = Options.Create(new SessionSettings
            {
                IdleTimeout = TimeSpan.FromMinutes(30),
                AbsoluteTimeout = TimeSpan.FromHours(12),
            });
            return new SessionFixture(new SessionService(options, time), time);
        }

        private sealed record SessionFixture(SessionService Service, TestTimeProvider Time);

        private sealed class TestTimeProvider : TimeProvider
        {
            private DateTimeOffset _utcNow;

            internal TestTimeProvider(DateTimeOffset utcNow)
            {
                _utcNow = utcNow;
            }

            public override DateTimeOffset GetUtcNow()
            {
                return _utcNow;
            }

            internal void Advance(TimeSpan duration)
            {
                _utcNow = _utcNow.Add(duration);
            }
        }
    }
}
