using FastSharp.Modules;

namespace FastSharp.Tests.Modules;

public sealed class NoContextModule : Module
{
    public NoContextModule()
    {
        Include<NoContextPingEndpoint>();
    }
}
