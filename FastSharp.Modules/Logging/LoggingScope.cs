using Microsoft.Extensions.Logging;

namespace FastSharp.Modules.Logging;

internal static class LoggingScope
{
    public static IDisposable BeginEntityScope(ILogger logger, string entityName, object? id = null)
    {
        var state = new Dictionary<string, object>(id is null ? 1 : 2)
        {
            ["Entity"] = entityName
        };

        if (id is not null)
        {
            state["Id"] = id;
        }

        return logger.BeginScope(state)!;
    }

    public static string FormatId<TKey>(TKey id) => id?.ToString() ?? string.Empty;
}
