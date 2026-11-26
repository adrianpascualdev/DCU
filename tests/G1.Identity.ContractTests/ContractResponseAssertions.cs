using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema;
using YamlDotNet.RepresentationModel;

namespace G1.Identity.ContractTests
{
    /// <summary>Contrasta las respuestas HTTP con el esquema del contrato normativo.</summary>
    internal static class ContractResponseAssertions
    {
        internal static async Task MatchesAsync(
            HttpResponseMessage response,
            string path,
            string method,
            HttpStatusCode expectedStatus,
            CancellationToken cancellationToken)
        {
            Assert.Equal(expectedStatus, response.StatusCode);

            using var reader = new StreamReader(Path.Combine(AppContext.BaseDirectory, @"openapi.v1.yaml"));
            var yaml = new YamlStream();
            yaml.Load(reader);
            var contract = (YamlMappingNode)yaml.Documents.Single().RootNode;
            var operation = Map(Map(Map(contract, @"paths"), path), method.ToLowerInvariant());
            var responses = Map(operation, @"responses");
            var definition = Resolve(contract, responses[((int)expectedStatus).ToString()]);

            if (!TryGet(definition, @"content", out var content))
            {
                Assert.Empty(await response.Content.ReadAsByteArrayAsync(cancellationToken));
                return;
            }

            var mediaType = Map(content).Keys.Single();
            Assert.Equal(mediaType, response.Content.Headers.ContentType?.MediaType);
            var schema = Map(Map(content), mediaType)[@"schema"];

            var definitions = new JsonObject();
            foreach (var (name, definitionSchema) in Map(Map(contract, @"components"), @"schemas"))
            {
                definitions[name] = Convert(definitionSchema);
            }

            var root = new JsonObject
            {
                [@"$schema"] = @"https://json-schema.org/draft/2020-12/schema",
                [@"$defs"] = definitions,
                [@"$ref"] = RewriteReference(Scalar(Map(schema)[@"$ref"])),
            };
            var validator = JsonSchema.FromText(root.ToJsonString());
            using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var result = validator.Evaluate(body.RootElement);
            Assert.True(result.IsValid, $@"{method} {path} ({(int)expectedStatus}) no cumple el esquema {Scalar(Map(schema)[@"$ref"])}.");
            if (mediaType == @"application/problem+json")
            {
                Assert.Equal((int)expectedStatus, body.RootElement.GetProperty(@"status").GetInt32());
            }
        }

        private static JsonNode Convert(YamlNode node)
        {
            if (node is YamlMappingNode mapping)
            {
                var result = new JsonObject();
                foreach (var pair in mapping.Children)
                {
                    var key = Scalar(pair.Key);
                    result[key] = key == @"$ref"
                        ? JsonValue.Create(RewriteReference(Scalar(pair.Value)))
                        : Convert(pair.Value);
                }
                return result;
            }

            if (node is YamlSequenceNode sequence)
            {
                var result = new JsonArray();
                foreach (var item in sequence.Children)
                {
                    result.Add(Convert(item));
                }
                return result;
            }

            var value = Scalar(node);
            if (value is @"true" or @"false" || int.TryParse(value, out _))
            {
                return JsonNode.Parse(value)!;
            }
            return JsonValue.Create(value)!;
        }

        private static string RewriteReference(string reference) =>
            reference.Replace(@"#/components/schemas/", @"#/$defs/", StringComparison.Ordinal);

        private static YamlNode Resolve(YamlNode contract, YamlNode node)
        {
            if (!TryGet(node, @"$ref", out var reference))
            {
                return node;
            }

            return Scalar(reference).TrimStart('#', '/').Split('/')
                .Aggregate(contract, (current, part) => Map(current)[part]);
        }

        private static bool TryGet(YamlNode node, string key, out YamlNode value) =>
            Map(node).TryGetValue(key, out value!);

        private static Dictionary<string, YamlNode> Map(YamlNode node) =>
            ((YamlMappingNode)node).Children.ToDictionary(pair => Scalar(pair.Key), pair => pair.Value);

        private static Dictionary<string, YamlNode> Map(YamlNode node, string key) =>
            Map(Map(node), key);

        private static Dictionary<string, YamlNode> Map(Dictionary<string, YamlNode> node, string key) =>
            Map(node[key]);

        private static string Scalar(YamlNode node) => ((YamlScalarNode)node).Value!;
    }
}
