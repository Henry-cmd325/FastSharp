using FastSharp.Modules.Core;
using FastSharp.Tests.Context;
using FastSharp.Tests.Dtos;
using Microsoft.AspNetCore.Builder;

namespace FastSharp.Tests.Modules
{
    public sealed class DtoSingleModule : Module<TestDbContext>
    {
        public DtoSingleModule()
        {
            AddCRUD<TestModel, int>("/dto-single", options => options.ConfigureAll<TestModelIdDto>());
        }
    }

    public sealed class DtoRequestResponseModule : Module<TestDbContext>
    {
        public DtoRequestResponseModule()
        {
            AddCRUD<TestModel, int>("/dto-dual", options => options.ConfigureAll<TestModelRequestDto, TestModelResponseDto>());
        }
    }

    public sealed class DtoValidationModule : Module<TestDbContext>
    {
        public DtoValidationModule()
        {
            AddCRUD<TestModel, int>("/dto-validation", options => options.ConfigureAll<TestModelValidatedDto>(builder => builder.WithValidation<TestModelValidatedDto>()));
        }
    }
}
