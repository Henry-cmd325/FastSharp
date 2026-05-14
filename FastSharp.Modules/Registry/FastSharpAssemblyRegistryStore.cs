using FastSharp.Modules.Core;
using System.Collections.Concurrent;
using System.Reflection;

namespace FastSharp.Modules.Registry;

public static class FastSharpAssemblyRegistryStore
{
    private static readonly ConcurrentDictionary<Assembly, IFastSharpAssemblyRegistry> Registries = new();

    public static void Register(IFastSharpAssemblyRegistry registry)
    {
        Registries[registry.Assembly] = registry;
    }

    public static bool TryGetRegistry(Assembly assembly, out IFastSharpAssemblyRegistry? registry)
    {
        return Registries.TryGetValue(assembly, out registry);
    }

    public static IFastSharpAssemblyRegistry GetRequiredRegistry(Assembly assembly)
    {
        if (TryGetRegistry(assembly, out var registry) && registry is not null)
        {
            return registry;
        }

        throw new InvalidOperationException($"FastSharp requires source-generated endpoint metadata for assembly '{assembly.GetName().Name}'. Make sure the assembly references the FastSharp generator.");
    }
}

public static class FastSharpModuleInvoker
{
    public static void Map(IEndpointRouteBuilder app, Type moduleType)
    {
        var module = (IFastModule)app.ServiceProvider.GetRequiredService(moduleType);
        module.Map(app);
    }
}
