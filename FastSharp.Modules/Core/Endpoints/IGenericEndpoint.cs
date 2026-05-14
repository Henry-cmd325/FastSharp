using FastSharp.Modules.Configuration;

namespace FastSharp.Modules.Core.Endpoints
{
    internal interface IGenericEndpoint
    {
        void Configure(EndpointOptions options);
        void Map(RouteGroupBuilder app, EndpointOptions allOptions);
    }
}
