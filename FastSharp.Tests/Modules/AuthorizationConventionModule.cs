using FastSharp.Modules.Core;
using FastSharp.Tests.Context;
using FastSharp.Tests.Endpoints;
using Microsoft.AspNetCore.Builder;

namespace FastSharp.Tests.Modules;

public sealed class AuthorizationConventionModule : Module<TestDbContext>
{
    public AuthorizationConventionModule()
    {
        ConfigureModule("/api/convention", conventions => conventions.RequireAuthorization());
        AddCRUD<TestModel, int>("/items");
        Include<PingEndpoint>();
    }
}
