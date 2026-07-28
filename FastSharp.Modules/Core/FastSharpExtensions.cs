using FastSharp.Modules.Filters;

namespace FastSharp.Modules.Core;

/// <summary>Provides extension methods for configuring FastSharp route handlers.</summary>
public static class FastSharpExtensions
{
    /// <summary>
    /// Adds FluentValidation to the route handler for the specified request type.
    /// If no <c>IValidator&lt;T&gt;</c> is registered in DI, the filter is a no-op.
    /// </summary>
    /// <typeparam name="T">The request type to validate.</typeparam>
    /// <param name="builder">The route handler builder to add validation to.</param>
    /// <returns>The same <see cref="RouteHandlerBuilder"/> for chaining.</returns>
    public static RouteHandlerBuilder WithValidation<T>(this RouteHandlerBuilder builder) where T : class
    {
        return builder.AddEndpointFilter<ValidationFilter<T>>();
    }
}