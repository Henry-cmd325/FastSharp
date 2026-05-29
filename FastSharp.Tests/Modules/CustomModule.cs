using FastSharp.Modules.Configuration;
using FastSharp.Modules.Core;
using FastSharp.Tests.Context;
using FastSharp.Tests.Endpoints;

namespace FastSharp.Tests.Modules;

public sealed class CustomModule : Module<TestDbContext>
{
    public CustomModule()
    {
        AddCRUD<TestModel, int>("/custom", options => options.DisableEndpoint(GenericEndpoint.GetList));
        Include<PingEndpoint>();
    }
}