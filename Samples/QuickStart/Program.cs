using Api.Context;
using Api.Modules.Products.Endpoints;
using FastSharp.Modules;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApiDbContext>(opt =>
    opt.UseInMemoryDatabase("fastsharp-demo"));

builder.Services.AddFastSharpEndpoints();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapFastSharpEndpoints();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(opt =>
    {
        opt.SwaggerEndpoint("/openapi/v1.json", "FastSharp API V1");
    });
}

app.UseHttpsRedirection();

app.Run();
