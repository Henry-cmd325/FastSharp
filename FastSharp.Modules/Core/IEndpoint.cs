namespace FastSharp.Modules.Core;

/// <summary>
/// Defines a custom endpoint or group of endpoints that can be registered on a module's route group.
/// Implement this interface to add non-CRUD routes to a <see cref="Module"/> via <c>Include&lt;TEndpoint&gt;()</c>.
/// </summary>
public interface IEndpoint
{
    /// <summary>Maps the endpoint's routes onto the module's <see cref="RouteGroupBuilder"/>.</summary>
    /// <param name="app">The route group provided by the parent module.</param>
    public void Map(RouteGroupBuilder app);
}
