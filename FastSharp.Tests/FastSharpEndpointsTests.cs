using FastSharp.Modules;
using FastSharp.Models;
using FastSharp.Tests.Context;
using FastSharp.Tests.Dtos;
using FastSharp.Tests.Endpoints;
using FastSharp.Tests.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using FastSharp.Modules.Registry;
using FastSharp.Tests.Validators;

namespace FastSharp.Tests;
public class FastSharpEndpointsTests
{
    private static async Task<WebApplication> CreateAppAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        var databaseRoot = new InMemoryDatabaseRoot();
        builder.Services.AddSingleton(databaseRoot);
        builder.Services.AddDbContext<TestDbContext>(options =>
            options.UseInMemoryDatabase("FastSharpTests", databaseRoot));
        builder.Services.AddFastSharpEndpoints(typeof(SampleModule).Assembly);
        builder.Services.AddScoped<ValidationRequestValidator>();

        var app = builder.Build();
        app.MapFastSharpEndpoints();
        await app.StartAsync();
        return app;
    }

    [Fact]
    public void AddFastSharpEndpoints_RegistersModulesAndEndpoints()
    {
        var services = new ServiceCollection();
        services.AddFastSharpEndpoints(typeof(SampleModule).Assembly);
        var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        Assert.NotNull(scope.ServiceProvider.GetService(typeof(SampleModule)));
        Assert.NotNull(scope.ServiceProvider.GetService(typeof(PingEndpoint)));
    }

    [Fact]
    public void AddFastSharpEndpoints_RegistersModuleWithoutContext()
    {
        var services = new ServiceCollection();
        services.AddFastSharpEndpoints(typeof(SampleModule).Assembly);
        var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        Assert.NotNull(scope.ServiceProvider.GetService(typeof(NoContextModule)));
        Assert.NotNull(scope.ServiceProvider.GetService(typeof(NoContextPingEndpoint)));
    }

    [Fact]
    public void FastSharpGenerator_RegistersAssemblyRegistry()
    {
        var found = FastSharpAssemblyRegistryStore.TryGetRegistry(typeof(SampleModule).Assembly, out var registry);

        Assert.True(found);
        Assert.NotNull(registry);
        Assert.Contains(typeof(PingEndpoint), registry!.GetEndpointTypes(typeof(PingEndpoint).Namespace!));
    }

    [Fact]
    public void AddFastSharpEndpoints_ThrowsWhenAssemblyHasNoGeneratedRegistry()
    {
        var services = new ServiceCollection();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddFastSharpEndpoints(typeof(string).Assembly));

        Assert.Contains("source-generated endpoint metadata", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MapFastSharpEndpoints_ThrowsWhenAssemblyHasNoGeneratedRegistry()
    {
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            app.MapFastSharpEndpoints(typeof(string).Assembly));

        Assert.Contains("source-generated endpoint metadata", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task MapFastSharpEndpoints_UsesAssembliesRegisteredInAddFastSharpEndpoints()
    {
        await using var app = await CreateAppAsync();
        var client = app.GetTestClient();

        var response = await client.GetAsync("/api/nocontext/ping");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task MapFastSharpEndpoints_MapsCrudEndpoints()
    {
        await using var app = await CreateAppAsync();
        var client = app.GetTestClient();

        var createResponse = await client.PostAsJsonAsync("/api/sample", new TestModel { Id = 1, Name = "Widget" });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var getResponse = await client.GetAsync("/api/sample/1");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var model = await getResponse.Content.ReadFromJsonAsync<TestModel>();
        Assert.NotNull(model);
        Assert.Equal("Widget", model!.Name);

        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        context.Models.Add(new TestModel { Id = 2, Name = "Gadget" });
        await context.SaveChangesAsync();

        var listResponse = await client.GetAsync("/api/sample");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var list = await listResponse.Content.ReadFromJsonAsync<List<TestModel>>();
        Assert.NotNull(list);
        Assert.Contains(list!, item => item.Id == 2);
    }

    [Fact]
    public async Task MapFastSharpEndpoints_GetById_ReturnsNotFoundWhenMissing()
    {
        await using var app = await CreateAppAsync();
        var client = app.GetTestClient();

        var response = await client.GetAsync("/api/sample/999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task MapFastSharpEndpoints_Put_ReturnsNotFoundWhenMissing()
    {
        await using var app = await CreateAppAsync();
        var client = app.GetTestClient();

        var response = await client.PutAsJsonAsync("/api/sample/999", new TestModel { Id = 999, Name = "Missing" });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task MapFastSharpEndpoints_Delete_ReturnsNotFoundWhenMissing()
    {
        await using var app = await CreateAppAsync();
        var client = app.GetTestClient();

        var response = await client.DeleteAsync("/api/sample/999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task MapFastSharpEndpoints_RespectsDisabledEndpointsAndIncludesCustom()
    {
        await using var app = await CreateAppAsync();
        var client = app.GetTestClient();

        var disabledResponse = await client.GetAsync("/api/custom");
        Assert.Equal(HttpStatusCode.MethodNotAllowed, disabledResponse.StatusCode);

        var pingResponse = await client.GetAsync("/api/custom/ping");
        Assert.Equal(HttpStatusCode.OK, pingResponse.StatusCode);
        var pingText = await pingResponse.Content.ReadAsStringAsync();
        Assert.Equal("pong", pingText);
    }

    [Fact]
    public async Task MapFastSharpEndpoints_ConfigureGroup_PrefixesRoutes()
    {
        await using var app = await CreateAppAsync();
        var client = app.GetTestClient();

        var createResponse = await client.PostAsJsonAsync("/api/grouped/items", new TestModel { Id = 10, Name = "Grouped" });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var getResponse = await client.GetAsync("/api/grouped/items/10");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var ungroupedResponse = await client.GetAsync("/api/items");
        Assert.Equal(HttpStatusCode.NotFound, ungroupedResponse.StatusCode);
    }

    [Fact]
    public async Task MapFastSharpEndpoints_ModuleWithoutContext_MapsCustomEndpointOnly()
    {
        await using var app = await CreateAppAsync();
        var client = app.GetTestClient();

        var pingResponse = await client.GetAsync("/api/nocontext/ping");
        Assert.Equal(HttpStatusCode.OK, pingResponse.StatusCode);
        Assert.Equal("pong-no-context", await pingResponse.Content.ReadAsStringAsync());

        var getResponse = await client.GetAsync("/api/nocontext");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);

        var postResponse = await client.PostAsJsonAsync("/api/nocontext", new { Id = 1 });
        Assert.Equal(HttpStatusCode.NotFound, postResponse.StatusCode);
    }

    [Fact]
    public void ModuleWithoutContext_DoesNotExposeGenericCrudSupport()
    {
        var nonGenericModuleMethods = typeof(FastSharp.Modules.Core.Module)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(method => method.Name == "AddCRUD");

        Assert.Empty(nonGenericModuleMethods);
    }

    [Fact]
    public async Task MapFastSharpEndpoints_Paged_ReturnsPagedResult()
    {
        await using var app = await CreateAppAsync();
        var client = app.GetTestClient();

        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            context.Models.AddRange(
                new TestModel { Id = 1, Name = "One" },
                new TestModel { Id = 2, Name = "Two" },
                new TestModel { Id = 3, Name = "Three" });
            await context.SaveChangesAsync();
        }

        var response = await client.GetAsync("/api/sample?page=1&pageSize=2");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<TestModel>>();
        Assert.NotNull(result);
        Assert.Equal(1, result!.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(3, result.TotalItems);
        Assert.Equal(2, result.Items.Count());
        Assert.Equal(new[] { 1, 2 }, result.Items.Select(item => item.Id).ToArray());
    }

    [Fact]
    public async Task MapFastSharpEndpoints_ConfigureAllSingleDto_UsesDtoContract()
    {
        await using var app = await CreateAppAsync();
        var client = app.GetTestClient();

        var createResponse = await client.PostAsJsonAsync("/api/dto-single", new TestModelIdDto { Id = 50 });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createBody = await createResponse.Content.ReadAsStringAsync();
        Assert.DoesNotContain("name", createBody, StringComparison.OrdinalIgnoreCase);

        var getResponse = await client.GetAsync("/api/dto-single/50");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var getBody = await getResponse.Content.ReadAsStringAsync();
        Assert.DoesNotContain("name", getBody, StringComparison.OrdinalIgnoreCase);

        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        var model = await context.Models.FindAsync(50);

        Assert.NotNull(model);
        Assert.Null(model!.Name);
    }

    [Fact]
    public async Task MapFastSharpEndpoints_ConfigureAllRequestResponseDtos_UsesRequestAndResponseDtos()
    {
        await using var app = await CreateAppAsync();
        var client = app.GetTestClient();

        var createResponse = await client.PostAsJsonAsync("/api/dto-dual", new TestModelRequestDto { Id = 60, Name = "DTO Name" });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createBody = await createResponse.Content.ReadAsStringAsync();
        Assert.DoesNotContain("name", createBody, StringComparison.OrdinalIgnoreCase);

        var getResponse = await client.GetAsync("/api/dto-dual/60");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var getBody = await getResponse.Content.ReadAsStringAsync();
        Assert.DoesNotContain("name", getBody, StringComparison.OrdinalIgnoreCase);

        var listResponse = await client.GetAsync("/api/dto-dual");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var listBody = await listResponse.Content.ReadAsStringAsync();
        Assert.DoesNotContain("name", listBody, StringComparison.OrdinalIgnoreCase);

        var pagedResponse = await client.GetAsync("/api/dto-dual?page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, pagedResponse.StatusCode);
        var pagedResult = await pagedResponse.Content.ReadFromJsonAsync<PagedResult<TestModelResponseDto>>();
        Assert.NotNull(pagedResult);
        Assert.Contains(pagedResult!.Items, item => item.Id == 60);

        var pagedBody = await pagedResponse.Content.ReadAsStringAsync();
        Assert.DoesNotContain("name", pagedBody, StringComparison.OrdinalIgnoreCase);

        var updateResponse = await client.PutAsJsonAsync("/api/dto-dual/60", new TestModelRequestDto { Id = 60, Name = "DTO Updated" });
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        var model = await context.Models.FindAsync(60);

        Assert.NotNull(model);
        Assert.Equal("DTO Updated", model!.Name);
    }

    [Fact]
    public async Task MapFastSharpEndpoints_IdSelectorModule()
    {
        await using var app = await CreateAppAsync();
        var client = app.GetTestClient();

        var createResponse = await client.PostAsJsonAsync("/api/id-selector", new TestModelWitoutInterface { Id = 60, Name = "DTO Name" });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var getResponse = await client.GetAsync("/api/id-selector/60");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var getBody = await getResponse.Content.ReadAsStringAsync();

        var listResponse = await client.GetAsync("/api/id-selector");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var listBody = await listResponse.Content.ReadAsStringAsync();

        var pagedResponse = await client.GetAsync("/api/id-selector?page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, pagedResponse.StatusCode);

        var pagedResult = await pagedResponse.Content.ReadFromJsonAsync<PagedResult<TestModelWitoutInterface>>();
        Assert.NotNull(pagedResult);
        Assert.Contains(pagedResult!.Items, item => item.Id == 60);

        var updateResponse = await client.PutAsJsonAsync("/api/id-selector/60", new TestModelWitoutInterface { Id = 60, Name = "DTO Updated" });
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        var model = await context.ModelsWithoutInterface.FindAsync(60);

        Assert.NotNull(model);
        Assert.Equal("DTO Updated", model!.Name);
    }

    [Fact]
    public async Task MapFastSharpEndpoints_WithValidation_ReturnsValidationProblemForInvalidRequest()
    {
        await using var app = await CreateAppAsync();
        var client = app.GetTestClient();

        var invalidResponse = await client.PostAsJsonAsync("/api/validation", new ValidationRequest { Name = string.Empty });
        Assert.Equal(HttpStatusCode.BadRequest, invalidResponse.StatusCode);

        using var problem = await JsonDocument.ParseAsync(await invalidResponse.Content.ReadAsStreamAsync());
        Assert.True(problem.RootElement.TryGetProperty("errors", out var errors));
        Assert.True(errors.TryGetProperty(nameof(ValidationRequest.Name), out _));

        var validResponse = await client.PostAsJsonAsync("/api/validation", new ValidationRequest { Name = "valid" });
        Assert.Equal(HttpStatusCode.OK, validResponse.StatusCode);

        var body = await validResponse.Content.ReadFromJsonAsync<ValidationRequest>();
        Assert.NotNull(body);
        Assert.Equal("valid", body!.Name);
    }
}
