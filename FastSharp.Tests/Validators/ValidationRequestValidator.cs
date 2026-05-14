using FastSharp.Tests.Dtos;
using FluentValidation;

namespace FastSharp.Tests.Validators;

public sealed class ValidationRequestValidator : AbstractValidator<ValidationRequest>
{
    public ValidationRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
    }
}
