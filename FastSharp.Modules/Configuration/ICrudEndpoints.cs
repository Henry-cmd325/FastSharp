using Microsoft.EntityFrameworkCore;

namespace FastSharp.Modules.Configuration
{
    public interface ICrudEndpoints<TDbContext> where TDbContext : DbContext
    {
        Type EntityType { get; }
        Type KeyType { get; }

        public ICrudEndpoints<TDbContext> DisableEndpoint(GenericEndpoint endpointName);

        public ICrudEndpoints<TDbContext> ConfigureEndpoint(GenericEndpoint endpointName, Action<RouteHandlerBuilder> configure);

        public ICrudEndpoints<TDbContext> ConfigureGroup(Action<RouteGroupBuilder> configure);

        internal void Map(RouteGroupBuilder group);
    }
}