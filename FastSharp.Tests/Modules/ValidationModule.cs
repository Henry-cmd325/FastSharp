using FastSharp.Modules.Core;
using FastSharp.Tests.Endpoints;

namespace FastSharp.Tests.Modules;

public sealed class ValidationModule : Module
{
    public ValidationModule()
    {
        Include<ValidationEndpoint>();
    }
}