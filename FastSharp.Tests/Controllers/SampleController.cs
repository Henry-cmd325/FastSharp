using FastSharp.Controllers;

namespace FastSharp.Tests.Controllers
{
    public sealed class SampleController : FastController<TestDbContext, TestModel, int>
    {
    }
}
