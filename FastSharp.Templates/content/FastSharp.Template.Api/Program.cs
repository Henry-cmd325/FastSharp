using FastSharpApi.Context;
using FastSharp.Modules;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
#if (Database == "InMemory")
builder.Services.AddDbContext<ApiDbContext>(opt =>
    opt.UseInMemoryDatabase("fastsharp-demo"));
#elif (Database == "SqlServer")
builder.Services.AddDbContext<ApiDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
#elif (Database == "Postgres")
builder.Services.AddDbContext<ApiDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
#elif (Database == "MySql")
builder.Services.AddDbContext<ApiDbContext>(opt =>
    opt.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), 
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));
#endif

builder.Services.AddFastSharpEndpoints();
builder.Services.AddOpenApi();

var app = builder.Build();

// Automatically create database and tables if they do not exist
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.MapFastSharpEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

#if (EnableSwagger)
    app.UseSwaggerUI(opt =>
    {
        opt.SwaggerEndpoint("/openapi/v1.json", "FastSharp API V1");
    });
#endif
}

app.UseHttpsRedirection();

app.Run();
