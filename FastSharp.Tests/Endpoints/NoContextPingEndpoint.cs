using FastSharp.Modules.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace FastSharp.Tests.Endpoints;

public sealed class NoContextPingEndpoint : IEndpoint
{
    public void Map(RouteGroupBuilder app)
    {
        app.MapGet("/nocontext/ping", () => "pong-no-context");
    }
}
