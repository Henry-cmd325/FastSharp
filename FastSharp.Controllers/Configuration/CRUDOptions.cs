namespace FastSharp.Controllers.Configuration
{
    public class CRUDOptions
    {
        internal EndpointOptions ConfigGetList = new();
        internal EndpointOptions ConfigGetById = new();
        internal EndpointOptions ConfigPost = new();
        internal EndpointOptions ConfigPut = new();
        internal EndpointOptions ConfigDelete = new();
        internal EndpointOptions ConfigAll = new();

        public void DisableEndpoint(GenericEndpoint endpointName)
        {
            switch (endpointName)
            {
                case GenericEndpoint.GetList:
                    ConfigGetList.Active = false;
                    break;
                case GenericEndpoint.GetById:
                    ConfigGetById.Active = false;
                    break;
                case GenericEndpoint.Create:
                    ConfigPost.Active = false;
                    break;
                case GenericEndpoint.Update:
                    ConfigPut.Active = false;
                    break;
                case GenericEndpoint.Delete:
                    ConfigDelete.Active = false;
                    break;
                case GenericEndpoint.All:
                    ConfigAll.Active = false;
                    break;
            }
        }

        public void ConfigureEndpoint(GenericEndpoint endpointName, Action<RouteHandlerBuilder> configure)
        {
            switch (endpointName)
            {
                case GenericEndpoint.GetList:
                    ConfigGetList.Builder = b => configure(b);
                    break;
                case GenericEndpoint.GetById:
                    ConfigGetById.Builder = b => configure(b);
                    break;
                case GenericEndpoint.Create:
                    ConfigPost.Builder = b => configure(b);
                    break;
                case GenericEndpoint.Update:
                    ConfigPut.Builder = b => configure(b);
                    break;
                case GenericEndpoint.Delete:
                    ConfigDelete.Builder = b => configure(b);
                    break;
                case GenericEndpoint.All:
                    ConfigAll.Builder = b => configure(b);
                    break;
            }
        }
    }
}
