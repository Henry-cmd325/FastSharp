using FastSharp.Controllers;
using FastSharp.Controllers.Configuration;
using FastSharp.Tests.Endpoints;

namespace FastSharp.Tests.Controllers
{
    public sealed class CustomController : FastController<TestDbContext, TestModel, int>
    {
        public CustomController()
        {
            ConfigureCRUD(options => options.DisableEndpoint(GenericEndpoint.GetList));
            IncludeNamespace<PingEndpoint>();
        }
    }
}
