namespace FastSharp.Modules.Logging;

/// <summary>
/// Marker type used by ASP.NET Core DI to resolve <see cref="Microsoft.Extensions.Logging.ILogger{TCategoryName}"/>
/// for FastSharp generic CRUD endpoint handlers.
/// </summary>
public sealed class FastSharpEngine;
