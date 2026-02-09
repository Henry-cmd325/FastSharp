using FastSharp.Controllers;
using FastSharp.Tests.Controllers;
using FastSharp.Tests.Endpoints;
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
            builder.Services.AddFastSharpEndpoints(typeof(SampleController).Assembly);

            var app = builder.Build();
            app.MapFastSharpEndpoints(typeof(SampleController).Assembly);
            await app.StartAsync();
            return app;
        }

        [Fact]
        public void AddFastSharpEndpoints_RegistersControllersAndEndpoints()
        {
            var services = new ServiceCollection();
            services.AddFastSharpEndpoints(typeof(SampleController).Assembly);
            var provider = services.BuildServiceProvider();

            using var scope = provider.CreateScope();
            Assert.NotNull(scope.ServiceProvider.GetService(typeof(SampleController)));
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
    }
}
