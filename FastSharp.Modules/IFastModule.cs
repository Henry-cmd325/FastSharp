namespace FastSharp.Modules;

public interface IFastModule
{
    internal void Map(IEndpointRouteBuilder app);
}
