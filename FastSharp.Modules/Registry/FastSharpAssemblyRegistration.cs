using System.Reflection;

namespace FastSharp.Modules.Registry;

public sealed class FastSharpAssemblyRegistration(IEnumerable<Assembly> assemblies)
{
    public IReadOnlyList<Assembly> Assemblies { get; } = [.. assemblies.Distinct()];
}
