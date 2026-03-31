using FastSharp.Modules;
using FastSharp.Tests.Context;

namespace FastSharp.Tests.Modules
{
    public class IdSelectorModule : Module<TestDbContext>
    {
        public IdSelectorModule()
        {
            AddCRUD<TestModel, int>("/id-selector", t => t.Id);
        }
    }
}
