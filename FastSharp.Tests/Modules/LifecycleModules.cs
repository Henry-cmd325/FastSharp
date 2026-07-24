using FastSharp.Modules.Core;
using FastSharp.Tests.Context;
using FastSharp.Tests.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FastSharp.Tests.Modules;

public sealed class LifecycleCustomOnlyModule : Module
{
    protected override void Configure(ModuleConfiguration configuration)
    {
        configuration.Prefix = "/api/lifecycle";
        configuration.Conventions = routes => routes.WithTags("Lifecycle");
    }

    protected override void AddRoutes(RouteGroupBuilder routes)
    {
        routes.MapGet("/inline", () => Results.Ok("lifecycle-inline"));
        Include<LifecyclePingEndpoint>();
    }
}

public sealed class LifecycleMixedModule : Module<TestDbContext>
{
    protected override void Configure(ModuleConfiguration configuration)
    {
        configuration.Prefix = "/api/lifecycle-mixed";
    }

    protected override void AddRoutes(RouteGroupBuilder routes)
    {
        AddCRUD<TestModel, int>("/items");
        Include<LifecycleDetailsEndpoint>();
        routes.MapGet("/inline", () => Results.Ok("lifecycle-mixed-inline"));
    }
}

public sealed class RouteOrderingModule : Module<TestDbContext>
{
    public static bool AddRoutesObservedLegacyEndpoint { get; private set; }

    public RouteOrderingModule()
    {
        ConfigureModule("/api/route-ordering", _ => { });
        AddCRUD<TestModel, int>("/legacy-crud");
        Include<LegacyOrderingEndpoint>();
    }

    protected override void AddRoutes(RouteGroupBuilder routes)
    {
        AddRoutesObservedLegacyEndpoint = LegacyOrderingEndpoint.WasMapped;
        if (!AddRoutesObservedLegacyEndpoint)
        {
            throw new InvalidOperationException("Queued endpoints must map before AddRoutes is invoked.");
        }

        AddCRUD<TestModel, int>("/new-crud");
        Include<NewOrderingEndpoint>();
        routes.MapGet("/new-inline", () => Results.Ok());
    }
}
