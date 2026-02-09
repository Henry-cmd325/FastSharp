using FastSharp.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace FastSharp.Tests.Endpoints
{
    public sealed class PingEndpoint : IFastEndpoint
    {
        public void Map(RouteGroupBuilder app)
        {
            app.MapGet("/custom/ping", () => "pong");
        }
    }
}
