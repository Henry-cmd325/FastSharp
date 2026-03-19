namespace FastSharp.Modules.Configuration;

public class EndpointOptions
{
    public Type? ResponseType { get; set; } = null;
    public Type? RequestType { get; set; } = null;
    public bool Active { get; set; } = true;
    public Action<RouteHandlerBuilder>? Builder { get; set; }
}