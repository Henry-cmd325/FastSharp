using FastSharpApi.Context;
using FastSharp.Modules.Core;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastSharpApi.Modules.Products.Endpoints;

public class UpdateProductsStock : IEndpoint
{
    public record UpdateProductStock(int Id, string Name, int Quantity);

    public class UpdateProductStockValidator : AbstractValidator<UpdateProductStock>
    {
        public UpdateProductStockValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id must be greater than 0");
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name cannot be empty");
            RuleFor(x => x.Quantity).NotEqual(0).WithMessage("Quantity cannot be 0");
        }
    }

    public void Map(RouteGroupBuilder app)
    {
        app.MapPost("/update-stock/{id}", async ([FromServices] ApiDbContext dbContext, [FromRoute] int id, [FromBody] UpdateProductStock product) =>
        {
            if (id != product.Id) return Results.BadRequest("ID in route does not match ID in body");

            var existingProduct = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (existingProduct == null) return Results.NotFound("Product not found");

            existingProduct.AddQuantity(product.Quantity);
            await dbContext.SaveChangesAsync();

            return Results.NoContent();
        })
        .WithValidation<UpdateProductStock>()
        .WithTags("Custom");
    }
}
