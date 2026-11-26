using System.Text.Json;

using G1.Identity.Mock;
using G1.Identity.Mock.Services;

namespace G1.Identity.ContractTests
{
    /// <summary>
    /// Verifica que la auditoría de seguridad sea completa y minimizada.
    /// </summary>
    public sealed class AuditTests
    {
        [Fact]
        public void SecurityEventsContainTraceabilityWithoutTokensOrPersonalData()
        {
            var service = new AuditService(TimeProvider.System);
            service.Record(Constants.AuditEvents.Logout, @"usr_1", @"ses_1", @"success", @"corr_1");
            service.Record(Constants.AuditEvents.SessionRevoked, @"usr_1", @"ses_2", @"success", @"corr_2");
            service.Record(Constants.AuditEvents.TokenRevoked, @"usr_1", @"ses_2", @"success", @"corr_2");

            var events = service.List();

            Assert.Equal(3, events.Count);
            Assert.All(events, auditEvent => Assert.False(string.IsNullOrWhiteSpace(auditEvent.CorrelationId)));
            Assert.All(events, auditEvent => Assert.False(string.IsNullOrWhiteSpace(auditEvent.ActorUserId)));
            Assert.All(events, auditEvent => Assert.False(string.IsNullOrWhiteSpace(auditEvent.TargetId)));
            Assert.All(events, auditEvent => Assert.False(string.IsNullOrWhiteSpace(auditEvent.Result)));
            Assert.All(events, auditEvent => Assert.NotEqual(default, auditEvent.OccurredAt));

            var serialized = JsonSerializer.Serialize(events);
            Assert.DoesNotContain(@"access_token", serialized, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(@"refresh_token", serialized, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(@"email", serialized, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(@"phone", serialized, StringComparison.OrdinalIgnoreCase);
        }
    }
}
