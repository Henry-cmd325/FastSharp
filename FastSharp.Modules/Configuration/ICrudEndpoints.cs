using Microsoft.EntityFrameworkCore;

namespace FastSharp.Modules.Configuration
{
    public interface ICrudEndpoints<TDbContext> where TDbContext : DbContext
    {
        Type EntityType { get; }
        Type KeyType { get; }

        public void DisableEndpoint(GenericEndpoint endpointName);

        public void ConfigureEndpoint(GenericEndpoint endpointName, Action<RouteHandlerBuilder> configure);

        public void ConfigureGroup(Action<RouteGroupBuilder> configure);

        public void Map(RouteGroupBuilder group);
    }
}