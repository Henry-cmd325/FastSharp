namespace FastSharp.Modules.Core;

public interface IEndpoint
{
    public void Map(RouteGroupBuilder app);
}
