using FastSharp.Modules.Core;
using FastSharp.Tests.Endpoints;

namespace FastSharp.Tests.Modules;

public sealed class NoContextModule : Module
{
    public NoContextModule()
    {
        Include<NoContextPingEndpoint>();
    }
}