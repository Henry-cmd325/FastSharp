namespace FastSharp.Modules.Core;

/// <summary>
/// Defines the route prefix and shared conventions for a <see cref="Module"/>.
/// </summary>
public sealed class ModuleConfiguration
{
    /// <summary>
    /// Gets or sets the URL prefix for the module route group. Defaults to <c>"/api"</c>.
    /// </summary>
    public string Prefix { get; set; } = "/api";

    /// <summary>
    /// Gets or sets the shared endpoint conventions applied to the module route group.
    /// Use this for metadata, authorization policies, filters, and OpenAPI settings.
    /// </summary>
    public Action<IEndpointConventionBuilder>? Conventions { get; set; }
}
