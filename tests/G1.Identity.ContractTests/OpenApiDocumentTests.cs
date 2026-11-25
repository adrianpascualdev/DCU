using YamlDotNet.RepresentationModel;

namespace G1.Identity.ContractTests
{
    /// <summary>
    /// Valida el contrato normativo OpenAPI 3.1 usado como fuente del documento publicado.
    /// </summary>
    public sealed class OpenApiDocumentTests
    {
        private static readonly string[] ExpectedTags =
        {
            @"OIDC",
            @"Identity",
            @"Sessions",
            @"Catalog",
            @"Mock",
        };

        private static readonly string[] HttpMethods =
        {
            @"get",
            @"post",
            @"put",
            @"delete",
            @"patch",
        };

        [Fact]
        public void ContractIsOpenApi31WithGovernanceMetadata()
        {
            var contract = LoadContract();

            Assert.Equal(@"3.1.0", Scalar(contract, @"openapi"));
            Assert.Equal(@"1.0.0", Scalar(contract, @"info", @"version"));
            Assert.False(string.IsNullOrWhiteSpace(Scalar(contract, @"info", @"x-reviewed-at")));
            Assert.Contains(@"ADR-0002", Scalar(contract, @"info", @"x-persistence-exception"), StringComparison.Ordinal);
            Assert.Equal(23, Map(contract, @"components", @"schemas").Count);

            var tags = Sequence(contract, @"tags")
                .Select(tag => Scalar(tag, @"name"))
                .ToArray();
            Assert.Equal(ExpectedTags, tags);
        }

        [Fact]
        public void ContractDeclaresCookieAndBearerSecuritySchemes()
        {
            var contract = LoadContract();
            var schemes = Map(contract, @"components", @"securitySchemes");

            var cookie = Map(schemes[@"cookieAuth"]);
            Assert.Equal(@"apiKey", Scalar(cookie, @"type"));
            Assert.Equal(@"cookie", Scalar(cookie, @"in"));
            Assert.Equal(@"__Host-g1.session", Scalar(cookie, @"name"));

            var bearer = Map(schemes[@"bearerAuth"]);
            Assert.Equal(@"http", Scalar(bearer, @"type"));
            Assert.Equal(@"bearer", Scalar(bearer, @"scheme"));
        }

        [Fact]
        public void AllNineteenOperationsHaveSuccessAndErrorExamples()
        {
            var contract = LoadContract();
            var paths = Map(contract, @"paths");
            var operations = paths
                .SelectMany(path => Map(path.Value)
                    .Where(entry => HttpMethods.Contains(entry.Key, StringComparer.OrdinalIgnoreCase))
                    .Select(entry => (
                        OperationId: Scalar(Map(entry.Value), @"operationId"),
                        Operation: entry.Value)))
                .ToList();

            Assert.Equal(19, operations.Count);
            Assert.Equal(19, operations.Select(operation => operation.OperationId).Distinct(StringComparer.Ordinal).Count());

            foreach (var operation in operations)
            {
                var responses = Map(operation.Operation, @"responses")
                    .ToDictionary(
                        entry => entry.Key,
                        entry => ResolveReference(contract, entry.Value),
                        StringComparer.Ordinal);

                var success = responses.Where(response => IsSuccessStatus(response.Key)).ToArray();
                var errors = responses.Where(response => !IsSuccessStatus(response.Key)).ToArray();

                Assert.NotEmpty(success);
                Assert.NotEmpty(errors);
                Assert.True(
                    success.Any(response => HasExample(response.Value)),
                    $@"La operación {operation.OperationId} no documenta un ejemplo de éxito.");
                Assert.True(
                    errors.Any(response => HasExample(response.Value)),
                    $@"La operación {operation.OperationId} no documenta un ejemplo de error.");
            }
        }

        [Fact]
        public void EmptySuccessResponsesUseTheAgreedExtension()
        {
            var contract = LoadContract();
            var paths = Map(contract, @"paths");

            var emptyResponses = paths
                .SelectMany(path => Map(path.Value)
                    .Where(entry => HttpMethods.Contains(entry.Key, StringComparer.OrdinalIgnoreCase))
                    .SelectMany(entry => Map(entry.Value, @"responses")))
                .Select(response => (
                    response.Key,
                    Value: ResolveReference(contract, response.Value)))
                .Where(response => Map(response.Value).TryGetValue(@"description", out var description)
                    && Scalar(description).Contains(@"vacía", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            Assert.NotEmpty(emptyResponses);
            Assert.All(emptyResponses, response => Assert.True(
                Map(response.Value).ContainsKey(@"x-success-example"),
                $@"La respuesta {response.Key} sin cuerpo debe conservar x-success-example."));
        }

        private static YamlMappingNode LoadContract()
        {
            var path = Path.Combine(AppContext.BaseDirectory, @"openapi.v1.yaml");
            using var reader = new StreamReader(path);
            var stream = new YamlStream();
            stream.Load(reader);

            return (YamlMappingNode)stream.Documents.Single().RootNode;
        }

        private static YamlNode Node(YamlNode node, params string[] keys)
        {
            var current = node;
            foreach (var key in keys)
            {
                current = Map(current)[key];
            }

            return current;
        }

        private static IReadOnlyDictionary<string, YamlNode> Map(YamlNode node, params string[] keys)
        {
            var target = (YamlMappingNode)Node(node, keys);
            return target.Children.ToDictionary(
                entry => Scalar(entry.Key),
                entry => entry.Value,
                StringComparer.Ordinal);
        }

        private static IReadOnlyDictionary<string, YamlNode> Map(
            IReadOnlyDictionary<string, YamlNode> map,
            string key)
        {
            return Map(map[key]);
        }

        private static IReadOnlyList<YamlNode> Sequence(YamlNode node, params string[] keys)
        {
            return ((YamlSequenceNode)Node(node, keys)).Children.ToArray();
        }

        private static string Scalar(IReadOnlyDictionary<string, YamlNode> map, string key)
        {
            return Scalar(map[key]);
        }

        private static string Scalar(YamlNode node, params string[] keys)
        {
            return ((YamlScalarNode)Node(node, keys)).Value
                ?? throw new InvalidOperationException(@"El contrato contiene un escalar vacío.");
        }

        private static YamlNode ResolveReference(YamlNode root, YamlNode node)
        {
            var map = Map(node);
            if (!map.TryGetValue(@"$ref", out var reference))
            {
                return node;
            }

            var segments = Scalar(reference).TrimStart('#', '/').Split('/');
            return segments.Aggregate(root, (current, segment) => Node(current, segment));
        }

        private static bool HasExample(YamlNode response)
        {
            var map = Map(response);
            if (map.ContainsKey(@"x-success-example"))
            {
                return true;
            }

            if (!map.TryGetValue(@"content", out var content))
            {
                return false;
            }

            return Map(content).Values.Any(media =>
            {
                var mediaMap = Map(media);
                return mediaMap.ContainsKey(@"example") || mediaMap.ContainsKey(@"examples");
            });
        }

        private static bool IsSuccessStatus(string statusCode)
        {
            return int.TryParse(statusCode, out var status) && status is >= 100 and < 400;
        }
    }
}
