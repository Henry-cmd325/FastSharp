namespace FastSharp.Modules.Configuration;

/// <summary>
/// Holds the activation state and route handler configuration for a single generated CRUD endpoint.
/// Created and managed internally by FastSharp — not intended for direct instantiation.
/// </summary>
public class EndpointOptions
{
    /// <summary>Gets or sets whether the endpoint is registered as a route. Defaults to <see langword="true"/>.</summary>
    public bool Active { get; set; } = true;

    /// <summary>Gets or sets an optional action to further configure the endpoint's <see cref="RouteHandlerBuilder"/>.</summary>
    public Action<RouteHandlerBuilder>? Builder { get; set; }
}