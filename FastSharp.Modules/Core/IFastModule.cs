namespace FastSharp.Modules.Core;

/// <summary>
/// Marker interface implemented by all FastSharp modules.
/// The <c>Map</c> method is internal and invoked exclusively by the FastSharp engine at startup.
/// </summary>
public interface IFastModule
{
    internal void Map(IEndpointRouteBuilder app);
}
