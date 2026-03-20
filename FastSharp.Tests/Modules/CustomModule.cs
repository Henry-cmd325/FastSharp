using FastSharp.Modules;
using FastSharp.Modules.Configuration;
using FastSharp.Tests.Context;
using FastSharp.Tests.Endpoints;

namespace FastSharp.Tests.Modules;

public sealed class CustomModule : Module<TestDbContext>
{
    public CustomModule()
    {
        AddCRUD<TestModel, int>("/custom", options => options.DisableEndpoint(GenericEndpoint.GetList));
        IncludeNamespace<PingEndpoint>();
    }
}