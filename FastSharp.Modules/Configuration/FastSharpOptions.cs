namespace FastSharp.Modules.Configuration;

/// <summary>
/// Global configuration for FastSharp generated endpoints.
/// </summary>
public class FastSharpOptions
{
    /// <summary>
    /// The default maximum number of items a generated GetList endpoint may return.
    /// </summary>
    /// <remarks>
    /// Applied as the upper bound when no pagination parameters are supplied (so an
    /// unbounded <c>GET /items</c> returns at most this many rows) and when the
    /// <c>pageSize</c> query parameter exceeds it. A per-endpoint override configured via
    /// <see cref="ICrudEndpoints{TDbContext}.GetList(int, System.Action{Microsoft.AspNetCore.Builder.RouteHandlerBuilder}?)"/>
    /// takes precedence over this value. Defaults to <c>100</c>.
    /// </remarks>
    public int MaxPageSize { get; set; } = 100;
}
