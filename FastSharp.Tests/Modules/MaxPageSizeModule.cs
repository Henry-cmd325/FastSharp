using FastSharp.Modules.Core;
using FastSharp.Tests.Context;

namespace FastSharp.Tests.Modules
{
    public sealed class MaxPageSizeModule : Module<TestDbContext>
    {
        public MaxPageSizeModule()
        {
            AddCRUD<TestModel, int>("/max-page", crud => crud.GetList(maxPageSize: 3));
        }
    }
}
