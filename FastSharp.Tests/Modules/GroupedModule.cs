using FastSharp.Modules.Core;
using FastSharp.Tests.Context;
using Microsoft.AspNetCore.Http;

namespace FastSharp.Tests.Modules
{
    public sealed class GroupedModule : Module<TestDbContext>
    {
        public GroupedModule()
        {
            ConfigureModule("/api/grouped", opt => opt.WithTags("grouped"));
            AddCRUD<TestModel, int>("/items");
        }
    }
}
