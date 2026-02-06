namespace FastSharp.Controllers.Configuration
{
    public class EndpointOptions
    {
        public bool Active { get; set; } = true;
        public Action<RouteHandlerBuilder>? Builder { get; set; }
    }
}