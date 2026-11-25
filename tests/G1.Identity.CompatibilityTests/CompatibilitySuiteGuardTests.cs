namespace G1.Identity.CompatibilityTests
{
    /// <summary>
    /// Guardia de configuración mientras las comparaciones de compatibilidad se implementan en otra fase.
    /// </summary>
    public sealed class CompatibilitySuiteGuardTests
    {
        [Fact]
        public void CompatibilitySuiteRunsWithXunit()
        {
            var assembly = typeof(CompatibilitySuiteGuardTests).Assembly;

            Assert.Equal(@"G1.Identity.CompatibilityTests", assembly.GetName().Name);
            Assert.Contains(
                assembly.GetReferencedAssemblies(),
                reference => reference.Name?.StartsWith(@"xunit", StringComparison.Ordinal) == true);
        }
    }
}
