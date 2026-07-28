using FastSharp.Modules.Core;
using System.Collections.Concurrent;
using System.Reflection;

namespace FastSharp.Modules.Registry;

/// <summary>
/// Process-wide store that maps assemblies to their source-generated <see cref="IFastSharpAssemblyRegistry"/>.
/// Populated at startup by the generated registry; consumed by the FastSharp engine during DI registration and endpoint mapping.
/// </summary>
public static class FastSharpAssemblyRegistryStore
{
    private static readonly ConcurrentDictionary<Assembly, IFastSharpAssemblyRegistry> _registries = new();

    /// <summary>Registers a source-generated <see cref="IFastSharpAssemblyRegistry"/> for its assembly.</summary>
    /// <param name="registry">The registry to store.</param>
    public static void Register(IFastSharpAssemblyRegistry registry)
    {
        _registries[registry.Assembly] = registry;
    }

    /// <summary>Attempts to retrieve the registry for the given assembly.</summary>
    /// <param name="assembly">The assembly to look up.</param>
    /// <param name="registry">The registry if found; otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if a registry was found; otherwise <see langword="false"/>.</returns>
    public static bool TryGetRegistry(Assembly assembly, out IFastSharpAssemblyRegistry? registry)
    {
        return _registries.TryGetValue(assembly, out registry);
    }

    /// <summary>
    /// Returns the registry for the given assembly, or throws <see cref="InvalidOperationException"/>
    /// if the assembly has no source-generated registry (i.e. it does not reference <c>FastSharp.Generators</c>).
    /// </summary>
    /// <param name="assembly">The assembly to look up.</param>
    public static IFastSharpAssemblyRegistry GetRequiredRegistry(Assembly assembly)
    {
        if (TryGetRegistry(assembly, out var registry) && registry is not null)
        {
            return registry;
        }

        throw new InvalidOperationException($"FastSharp requires source-generated endpoint metadata for assembly '{assembly.GetName().Name}'. Make sure the assembly references the FastSharp generator.");
    }
}

/// <summary>
/// Resolves and invokes a <see cref="Core.IFastModule"/> from DI and maps its endpoints.
/// Called by the source-generated registry; not intended for direct use.
/// </summary>
public static class FastSharpModuleInvoker
{
    /// <summary>Resolves <paramref name="moduleType"/> from DI and maps its endpoints onto <paramref name="app"/>.</summary>
    /// <param name="app">The application's endpoint route builder.</param>
    /// <param name="moduleType">The concrete module type to resolve and invoke.</param>
    public static void Map(IEndpointRouteBuilder app, Type moduleType)
    {
        var module = (IFastModule)app.ServiceProvider.GetRequiredService(moduleType);
        module.Map(app);
    }
}
