namespace FastSharp.Models
{
    public interface IModel<T>
    {
        T Id { get; set; }
    }
}