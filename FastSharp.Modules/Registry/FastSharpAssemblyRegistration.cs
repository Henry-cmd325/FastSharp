using System.Reflection;

namespace FastSharp.Modules.Registry;

/// <summary>
/// Holds the assemblies registered via <c>AddFastSharpEndpoints</c> so they can be
/// retrieved at map time without passing them through the pipeline manually.
/// Registered as a singleton in DI by the FastSharp engine.
/// </summary>
public sealed class FastSharpAssemblyRegistration(IEnumerable<Assembly> assemblies)
{
    /// <summary>Gets the distinct assemblies registered with FastSharp.</summary>
    public IReadOnlyList<Assembly> Assemblies { get; } = [.. assemblies.Distinct()];
}
