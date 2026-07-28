using System.Reflection;

namespace FastSharp.Modules.Registry;

/// <summary>
/// Implemented by the source-generated registry that FastSharp emits per assembly.
/// Provides the engine with module and endpoint metadata without runtime reflection scanning.
/// Not intended for manual implementation.
/// </summary>
public interface IFastSharpAssemblyRegistry
{
    /// <summary>Gets the assembly this registry was generated for.</summary>
    Assembly Assembly { get; }

    /// <summary>Registers all modules and endpoints from this assembly into the DI container.</summary>
    /// <param name="services">The application's service collection.</param>
    void RegisterServices(IServiceCollection services);

    /// <summary>Maps all modules from this assembly onto the application's route builder.</summary>
    /// <param name="app">The application's endpoint route builder.</param>
    void MapEndpoints(IEndpointRouteBuilder app);

    /// <summary>Returns all endpoint types whose namespace starts with <paramref name="namespacePrefix"/>.</summary>
    /// <param name="namespacePrefix">The namespace prefix to filter by.</param>
    IReadOnlyList<Type> GetEndpointTypes(string namespacePrefix);
}
