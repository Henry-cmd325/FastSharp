using FastSharp.Modules;
using FastSharp.Tests.Context;

namespace FastSharp.Tests.Modules
{
    public sealed class SampleModule : Module<TestDbContext>
    {
        public SampleModule()
        {
            AddCRUD<TestModel, int>("/sample");
        }
    }
}
