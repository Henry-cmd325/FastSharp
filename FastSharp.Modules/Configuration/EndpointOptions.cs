namespace FastSharp.Modules.Configuration
{
    public class EndpointOptions
    {
        public bool Active { get; set; } = true;
        public Action<RouteHandlerBuilder>? Builder { get; set; }
    }
}