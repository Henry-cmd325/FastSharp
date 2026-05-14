using FastSharp.Modules.Core;
using FastSharp.Tests.Context;
using FastSharp.Tests.Dtos;

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
}
