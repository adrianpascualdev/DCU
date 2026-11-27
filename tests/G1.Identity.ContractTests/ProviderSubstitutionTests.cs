using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace G1.Identity.ContractTests
{
    /// <summary>
    /// Verifica la sustitución por configuración (SC-003/FR-037) con ID tokens y
    /// documentos OIDC de prueba. El transporte controlado no ejecuta el flujo PKCE.
    /// </summary>
    public sealed class ProviderSubstitutionTests
    {
        private const string FirstIssuer = "https://first-provider.example.test";
        private const string SecondIssuer = "https://second-provider.example.test";
        private const string FirstClientId = "consumer-test-first";
        private const string SecondClientId = "consumer-test-second";
        private const string Subject = "usr_provider_substitution";
        private const string Nonce = "provider-substitution-nonce";
        private static readonly DateTime ValidationTime = new(2026, 11, 27, 12, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Comprueba que el mismo consumidor identifica al usuario con cada proveedor
        /// y que deja de confiar en el emisor, cliente y firma anteriores.
        /// </summary>
        [Fact]
        public async Task SameConsumerAcceptsBothProvidersAndRejectsPreviousTrustSettings()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            using var firstRsa = RSA.Create(2048);
            using var secondRsa = RSA.Create(2048);
            var firstKey = new RsaSecurityKey(firstRsa) { KeyId = "first-test-key" };
            var secondKey = new RsaSecurityKey(secondRsa) { KeyId = "second-test-key" };
            var documents = new Dictionary<Uri, object>();
            AddProviderDocuments(documents, FirstIssuer, firstKey);
            AddProviderDocuments(documents, SecondIssuer, secondKey);

            using var http = new HttpClient(new DiscoveryTransport(documents));
            var consumer = new IdentityConsumer(http);
            var firstConfiguration = CreateConfiguration(FirstIssuer, FirstClientId);
            var secondConfiguration = CreateConfiguration(SecondIssuer, SecondClientId);
            var firstToken = CreateIdToken(FirstIssuer, FirstClientId, firstKey);
            var secondToken = CreateIdToken(SecondIssuer, SecondClientId, secondKey);

            Assert.Equal(Subject, await consumer.ResolveUserIdAsync(firstConfiguration, firstToken, Nonce, cancellationToken));
            Assert.Equal(Subject, await consumer.ResolveUserIdAsync(secondConfiguration, secondToken, Nonce, cancellationToken));
            Assert.Null(await consumer.ResolveUserIdAsync(secondConfiguration, firstToken, Nonce, cancellationToken));

            // Cambia un valor por caso para que cada comprobación sea independiente.
            var oldIssuerToken = CreateIdToken(FirstIssuer, SecondClientId, secondKey);
            var oldClientToken = CreateIdToken(SecondIssuer, FirstClientId, secondKey);
            var oldKeyWithCurrentId = new RsaSecurityKey(firstRsa) { KeyId = secondKey.KeyId };
            var oldSignatureToken = CreateIdToken(SecondIssuer, SecondClientId, oldKeyWithCurrentId);

            Assert.Null(await consumer.ResolveUserIdAsync(secondConfiguration, oldIssuerToken, Nonce, cancellationToken));
            Assert.Null(await consumer.ResolveUserIdAsync(secondConfiguration, oldClientToken, Nonce, cancellationToken));
            Assert.Null(await consumer.ResolveUserIdAsync(secondConfiguration, oldSignatureToken, Nonce, cancellationToken));
        }

        private static IConfiguration CreateConfiguration(string issuer, string clientId)
        {
            return new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Oidc:Issuer"] = issuer,
                ["Oidc:ClientId"] = clientId,
            }).Build();
        }

        private static void AddProviderDocuments(IDictionary<Uri, object> documents, string issuer, RsaSecurityKey signingKey)
        {
            var discoveryUri = new Uri($"{issuer}/.well-known/openid-configuration");
            var jwksUri = new Uri($"{issuer}/.well-known/jwks.json");
            var publicKey = JsonWebKeyConverter.ConvertFromRSASecurityKey(
                new RsaSecurityKey(signingKey.Rsa.ExportParameters(includePrivateParameters: false)) { KeyId = signingKey.KeyId });

            documents.Add(discoveryUri, new
            {
                issuer,
                authorization_endpoint = $"{issuer}/connect/authorize",
                token_endpoint = $"{issuer}/connect/token",
                userinfo_endpoint = $"{issuer}/connect/userinfo",
                revocation_endpoint = $"{issuer}/connect/revoke",
                end_session_endpoint = $"{issuer}/connect/logout",
                jwks_uri = jwksUri,
                response_types_supported = new[] { "code" },
                grant_types_supported = new[] { "authorization_code", "refresh_token" },
                code_challenge_methods_supported = new[] { "S256" },
                scopes_supported = new[] { "openid", "profile", "email", "offline_access", "identity.api" },
                subject_types_supported = new[] { "public" },
                id_token_signing_alg_values_supported = new[] { SecurityAlgorithms.RsaSha256 },
            });
            documents.Add(jwksUri, new
            {
                keys = new[]
                {
                    new { kty = publicKey.Kty, kid = publicKey.Kid, use = "sig", alg = SecurityAlgorithms.RsaSha256, n = publicKey.N, e = publicKey.E },
                },
            });
        }

        private static string CreateIdToken(string issuer, string clientId, RsaSecurityKey signingKey)
        {
            var issuedAt = ValidationTime.AddMinutes(-1);
            return new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
            {
                Issuer = issuer,
                Audience = clientId,
                IssuedAt = issuedAt,
                NotBefore = issuedAt,
                Expires = issuedAt.AddMinutes(5),
                SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.RsaSha256),
                Claims = new Dictionary<string, object>
                {
                    ["sub"] = Subject,
                    ["nonce"] = Nonce,
                    ["auth_time"] = new DateTimeOffset(issuedAt).ToUnixTimeSeconds(),
                    ["amr"] = new[] { "mock_persona" },
                    ["sid"] = "ses_provider_substitution",
                },
            });
        }

        private sealed record DiscoveryDocument(
            [property: JsonPropertyName("issuer")] string Issuer,
            [property: JsonPropertyName("jwks_uri")] Uri JwksUri);

        private sealed class DiscoveryTransport(IReadOnlyDictionary<Uri, object> documents) : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Assert.Equal(HttpMethod.Get, request.Method);
                Assert.NotNull(request.RequestUri);
                Assert.True(documents.TryGetValue(request.RequestUri, out var document), $"Documento OIDC inesperado: {request.RequestUri}");

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(document),
                });
            }
        }

        private sealed class IdentityConsumer(HttpClient http)
        {
            public async Task<string?> ResolveUserIdAsync(
                IConfiguration configuration, string idToken, string expectedNonce, CancellationToken cancellationToken)
            {
                var issuer = configuration["Oidc:Issuer"];
                var clientId = configuration["Oidc:ClientId"];
                var discovery = await http.GetFromJsonAsync<DiscoveryDocument>(
                    $"{issuer}/.well-known/openid-configuration", cancellationToken);
                if (discovery is null || !string.Equals(discovery.Issuer, issuer, StringComparison.Ordinal))
                {
                    return null;
                }

                var jwks = new JsonWebKeySet(await http.GetStringAsync(discovery.JwksUri, cancellationToken));
                var result = await new JsonWebTokenHandler().ValidateTokenAsync(idToken, new TokenValidationParameters
                {
                    ValidIssuer = issuer,
                    ValidAudience = clientId,
                    ValidAlgorithms = new[] { SecurityAlgorithms.RsaSha256 },
                    IssuerSigningKeys = jwks.GetSigningKeys(),
                    ValidateIssuerSigningKey = true,
                    LifetimeValidator = (notBefore, expires, _, _) => notBefore <= ValidationTime && expires > ValidationTime,
                });

                if (!result.IsValid || result.ClaimsIdentity.FindFirst("nonce")?.Value != expectedNonce)
                {
                    return null;
                }

                return result.ClaimsIdentity.FindFirst("sub")?.Value;
            }
        }
    }
}
