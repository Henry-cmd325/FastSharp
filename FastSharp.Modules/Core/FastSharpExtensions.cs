using FastSharp.Modules.Filters;

namespace FastSharp.Modules.Core;

public static class FastSharpExtensions
{
    public static RouteHandlerBuilder WithValidation<T>(this RouteHandlerBuilder builder) where T : class
    {
        return builder.AddEndpointFilter<ValidationFilter<T>>();
    }
}