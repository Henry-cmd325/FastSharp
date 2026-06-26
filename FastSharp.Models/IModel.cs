namespace FastSharp.Models
{
    /// <summary>
    /// Represents an entity with a strongly-typed identifier.
    /// Implement this interface on entities used with the parameterless <c>AddCRUD</c> overload.
    /// </summary>
    /// <typeparam name="T">The type of the entity's primary key.</typeparam>
    public interface IModel<T>
    {
        /// <summary>Gets or sets the unique identifier of the entity.</summary>
        T Id { get; set; }
    }
}