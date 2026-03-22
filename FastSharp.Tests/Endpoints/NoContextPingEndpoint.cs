using FastSharp.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace FastSharp.Tests.Modules;

public sealed class NoContextPingEndpoint : IEndpoint
{
    public void Map(RouteGroupBuilder app)
    {
        app.MapGet("/nocontext/ping", () => "pong-no-context");
    }
}
