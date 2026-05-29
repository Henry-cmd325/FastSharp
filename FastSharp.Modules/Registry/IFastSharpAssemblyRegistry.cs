using System.Reflection;

namespace FastSharp.Modules.Registry;

public interface IFastSharpAssemblyRegistry
{
    Assembly Assembly { get; }

    void RegisterServices(IServiceCollection services);

    void MapEndpoints(IEndpointRouteBuilder app);

    IReadOnlyList<Type> GetEndpointTypes(string namespacePrefix);
}
