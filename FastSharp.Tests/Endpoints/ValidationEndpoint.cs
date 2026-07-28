using FastSharp.Modules.Core;
using FastSharp.Tests.Dtos;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FastSharp.Tests.Endpoints;

public sealed class ValidationEndpoint : IEndpoint
{
    public void Map(RouteGroupBuilder app)
    {
        app.MapPost("/validation", (ValidationRequest request) => Results.Ok(request))
            .WithValidation<ValidationRequest>();

        app.MapPost("/validation/missing-target", () => Results.Ok("missing-target"))
            .WithValidation<ValidationRequest>();
    }
}
