using FastSharp.Modules;
using FastSharp.Models;
using FastSharp.Tests.Context;
using FastSharp.Tests.Endpoints;
using FastSharp.Tests.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace FastSharp.Tests
{
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

            var app = builder.Build();
            app.MapFastSharpEndpoints(typeof(SampleModule).Assembly);
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

            var response = await client.GetAsync("/api/sample/paged?page=1&pageSize=2");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<PagedResult<TestModel>>();
            Assert.NotNull(result);
            Assert.Equal(1, result!.Page);
            Assert.Equal(2, result.PageSize);
            Assert.Equal(3, result.TotalItems);
            Assert.Equal(2, result.Items.Count());
            Assert.Equal(new[] { 1, 2 }, result.Items.Select(item => item.Id).ToArray());
        }
    }
}
