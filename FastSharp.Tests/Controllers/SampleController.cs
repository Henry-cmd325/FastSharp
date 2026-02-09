using FastSharp.Controllers;

namespace FastSharp.Tests.Controllers
{
    public sealed class SampleController : Module<TestDbContext>
    {
        public SampleController()
        {
            AddCRUD<TestModel, int>(_ => { }, "/sample");
        }
    }
}
