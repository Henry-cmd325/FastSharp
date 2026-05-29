using FastSharp.Modules.Logging;
using FluentValidation;

namespace FastSharp.Modules.Filters;

public class ValidationFilter<T> : IEndpointFilter where T : class
{
    private static readonly string EntityName = typeof(T).Name;

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();
        if (validator is null)
        {
            return await next(context);
        }

        if (context.Arguments.FirstOrDefault(a => a?.GetType() == typeof(T)) is not T argument)
        {
            // This should not happen if the filter is applied correctly
            return Results.Problem("Validation target not found.", statusCode: 500);
        }

        var validationResult = await validator.ValidateAsync(argument);

        if (!validationResult.IsValid)
        {
            var logger = context.HttpContext.RequestServices.GetService<ILogger<FastSharpEngine>>();
            if (logger is not null)
            {
                FastSharpLogger.LogValidationFailed(logger, EntityName);
            }

            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        return await next(context);
    }
}

