namespace FastSharp.Modules.Core;

public interface IFastModule
{
    internal void Map(IEndpointRouteBuilder app);
}
