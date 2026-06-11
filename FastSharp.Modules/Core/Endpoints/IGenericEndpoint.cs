using FastSharp.Modules.Configuration;

namespace FastSharp.Modules.Core.Endpoints;

internal interface IGenericEndpoint
{
    bool IsActive { get; }

    void Configure(EndpointOptions options);

    void Map(RouteGroupBuilder app, EndpointOptions allOptions);
}
