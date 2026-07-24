using FastSharp.Modules.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FastSharp.Tests.Endpoints;

public sealed class LifecyclePingEndpoint : IEndpoint
{
    public void Map(RouteGroupBuilder app)
    {
        app.MapGet("/ping", () => Results.Ok("lifecycle-pong"));
    }
}

public sealed class LifecycleDetailsEndpoint : IEndpoint
{
    public void Map(RouteGroupBuilder app)
    {
        app.MapGet("/details", () => Results.Ok("lifecycle-details"));
    }
}

public sealed class LegacyOrderingEndpoint : IEndpoint
{
    public static bool WasMapped { get; set; }

    public void Map(RouteGroupBuilder app)
    {
        WasMapped = true;
        app.MapGet("/legacy-include", () => Results.Ok());
    }
}

public sealed class NewOrderingEndpoint : IEndpoint
{
    public void Map(RouteGroupBuilder app)
    {
        app.MapGet("/new-include", () => Results.Ok());
    }
}
