namespace FastSharp.Controllers
{
    public interface IFastEndpoint
    {
        public void Map(IEndpointRouteBuilder app);
    }
}
