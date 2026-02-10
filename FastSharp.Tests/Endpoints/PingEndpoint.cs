using FastSharp.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace FastSharp.Tests.Endpoints
{
    public sealed class PingEndpoint : IEndpoint
    {
        public void Map(RouteGroupBuilder app)
        {
            app.MapGet("/custom/ping", () => "pong");
        }
    }
}
