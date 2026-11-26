using System.Text.RegularExpressions;

namespace G1.Identity.ContractTests
{
    /// <summary>
    /// Comprueba que el repositorio no contiene credenciales reales conocidas.
    /// Las cabeceras HTTP de T066 se ejercitan en PipelineIntegrationTests y
    /// SwaggerAvailabilityTests; la respuesta /me se añadirá cuando DEV3 publique T036.
    /// </summary>
    public sealed partial class SecurityHeadersAndSecretScanTests
    {
        private static readonly string[] ExcludedDirectories =
        [
            @".git",
            @"bin",
            @"node_modules",
            @"obj",
        ];

        private static readonly string[] TextExtensions =
        [
            @".cs",
            @".csproj",
            @".json",
            @".md",
            @".props",
            @".targets",
            @".ts",
            @".tsx",
            @".yaml",
            @".yml",
        ];

        [Fact]
        public void RepositoryDoesNotContainPrivateKeysOrProviderTokens()
        {
            var repositoryRoot = FindRepositoryRoot();
            var findings = Directory
                .EnumerateFiles(repositoryRoot, @"*", SearchOption.AllDirectories)
                .Where(IsScannableFile)
                .SelectMany(path => FindSecrets(repositoryRoot, path))
                .ToArray();

            Assert.Empty(findings);
        }

        private static string FindRepositoryRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);

            while (directory is not null)
            {
                if (Directory.Exists(Path.Combine(directory.FullName, @".git"))
                    && Directory.Exists(Path.Combine(directory.FullName, @"specs")))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            throw new DirectoryNotFoundException(@"No se encontró la raíz del repositorio para escanear secretos.");
        }

        private static bool IsScannableFile(string path)
        {
            var segments = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return !segments.Any(segment => ExcludedDirectories.Contains(segment, StringComparer.OrdinalIgnoreCase))
                && TextExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase);
        }

        private static IEnumerable<string> FindSecrets(string repositoryRoot, string path)
        {
            var content = File.ReadAllText(path);
            foreach (Match match in SecretPattern().Matches(content))
            {
                var line = 1;
                for (var index = 0; index < match.Index; index++)
                {
                    if (content[index] == '\n')
                    {
                        line++;
                    }
                }

                yield return $@"{Path.GetRelativePath(repositoryRoot, path)}:{line}: {match.Groups[@"kind"].Value}";
            }
        }

        [GeneratedRegex(
            @"(?<kind>-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----|AKIA[0-9A-Z]{16}|github_pat_[A-Za-z0-9_]{20,}|gh[opsur]_[A-Za-z0-9]{36,}|xox[baprs]-[A-Za-z0-9-]{10,})",
            RegexOptions.CultureInvariant)]
        private static partial Regex SecretPattern();
    }
}
