namespace FastSharp.Modules.Logging;

internal static partial class FastSharpLogger
{
    public const string CategoryName = "FastSharp.Modules";

    [LoggerMessage(EventId = 1000, Level = LogLevel.Information, Message = "[FastSharp] Starting module scan for {AssemblyCount} assembly/assemblies.")]
    public static partial void LogStartingModuleScan(ILogger logger, int assemblyCount);

    [LoggerMessage(EventId = 1001, Level = LogLevel.Information, Message = "[FastSharp] Scanning assembly {AssemblyName} for modules and endpoints.")]
    public static partial void LogScanningAssembly(ILogger logger, string assemblyName);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Information, Message = "[FastSharp] Completed module scan.")]
    public static partial void LogCompletedModuleScan(ILogger logger);

    [LoggerMessage(EventId = 1003, Level = LogLevel.Information, Message = "[FastSharp] Mapping module {ModuleName} at route prefix {RoutePrefix}.")]
    public static partial void LogMappingModule(ILogger logger, string moduleName, string routePrefix);

    [LoggerMessage(EventId = 1004, Level = LogLevel.Information, Message = "[FastSharp] Mapping CRUD endpoints for entity {EntityName} -> {Route}. Enabled verbs: {EnabledVerbs}.")]
    public static partial void LogMappingCrudEndpoints(ILogger logger, string entityName, string route, string enabledVerbs);

    [LoggerMessage(EventId = 1005, Level = LogLevel.Information, Message = "[FastSharp] Mapping custom endpoint {EndpointName} in module route group {RoutePrefix}.")]
    public static partial void LogMappingCustomEndpoint(ILogger logger, string endpointName, string routePrefix);

    [LoggerMessage(EventId = 2000, Level = LogLevel.Debug, Message = "Searching for {EntityName} with ID {Id}.")]
    public static partial void LogGetById(ILogger logger, string entityName, string id);

    [LoggerMessage(EventId = 2001, Level = LogLevel.Debug, Message = "{EntityName} with ID {Id} was not found.")]
    public static partial void LogEntityNotFound(ILogger logger, string entityName, string id);

    [LoggerMessage(EventId = 2002, Level = LogLevel.Debug, Message = "Retrieved {EntityName} with ID {Id}.")]
    public static partial void LogGetByIdSuccess(ILogger logger, string entityName, string id);

    [LoggerMessage(EventId = 2003, Level = LogLevel.Debug, Message = "Applying list query filters on {EntityName}. Page={Page}, PageSize={PageSize}.")]
    public static partial void LogGetListPaged(ILogger logger, string entityName, int page, int pageSize);

    [LoggerMessage(EventId = 2004, Level = LogLevel.Debug, Message = "Retrieving all records for {EntityName}.")]
    public static partial void LogGetListAll(ILogger logger, string entityName);

    [LoggerMessage(EventId = 2005, Level = LogLevel.Information, Message = "Inserting {EntityName}.")]
    public static partial void LogCreatingEntity(ILogger logger, string entityName);

    [LoggerMessage(EventId = 2006, Level = LogLevel.Information, Message = "Created {EntityName} with ID {Id}.")]
    public static partial void LogCreatedEntity(ILogger logger, string entityName, string id);

    [LoggerMessage(EventId = 2007, Level = LogLevel.Information, Message = "Updating {EntityName} with ID {Id}.")]
    public static partial void LogUpdatingEntity(ILogger logger, string entityName, string id);

    [LoggerMessage(EventId = 2008, Level = LogLevel.Information, Message = "Updated {EntityName} with ID {Id}.")]
    public static partial void LogUpdatedEntity(ILogger logger, string entityName, string id);

    [LoggerMessage(EventId = 2009, Level = LogLevel.Information, Message = "Deleting {EntityName} with ID {Id}.")]
    public static partial void LogDeletingEntity(ILogger logger, string entityName, string id);

    [LoggerMessage(EventId = 2010, Level = LogLevel.Information, Message = "Deleted {EntityName} with ID {Id}.")]
    public static partial void LogDeletedEntity(ILogger logger, string entityName, string id);

    [LoggerMessage(EventId = 3000, Level = LogLevel.Error, Message = "Persistence error during {Operation} on {EntityName}.")]
    public static partial void LogPersistenceError(ILogger logger, Exception exception, string entityName, string operation);

    [LoggerMessage(EventId = 3001, Level = LogLevel.Warning, Message = "Validation failed for {EntityName}.")]
    public static partial void LogValidationFailed(ILogger logger, string entityName);
}
