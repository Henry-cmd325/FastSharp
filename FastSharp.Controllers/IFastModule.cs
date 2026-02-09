namespace FastSharp.Controllers
{
    public interface IFastModule
    {
        public void Map(IEndpointRouteBuilder app);
    }
}
