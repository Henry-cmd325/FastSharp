namespace FastSharp.Modules
{
    public interface IEndpoint
    {
        public void Map(RouteGroupBuilder app);
    }
}
