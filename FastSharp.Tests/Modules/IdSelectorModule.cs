using FastSharp.Modules.Core;
using FastSharp.Tests.Context;

namespace FastSharp.Tests.Modules
{
    public class IdSelectorModule : Module<TestDbContext>
    {
        public IdSelectorModule()
        {
            AddCRUD<TestModelWitoutInterface, int>("/id-selector", t => t.Id);
        }
    }
}
