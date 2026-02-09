using FastSharp.Controllers;
using FastSharp.Controllers.Configuration;
using FastSharp.Tests.Endpoints;

namespace FastSharp.Tests.Controllers
{
    public sealed class CustomController : Module<TestDbContext>
    {
        public CustomController()
        {
            AddCRUD<TestModel, int>(options => options.DisableEndpoint(GenericEndpoint.GetList), "/custom");
            IncludeNamespace<PingEndpoint>();
        }
    }
}
