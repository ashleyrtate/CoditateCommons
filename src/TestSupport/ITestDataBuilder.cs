namespace Coditate.TestSupport
{
    /// <summary>
    /// Defines the contract for classes that build data entities for unit tests.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// Each builder instance is expected to manage the life cycle of a single 
    /// data entity and all entities that it depends on.
    /// </remarks>
    public interface ITestDataBuilder<T>
    {
        /// <summary>
        /// Builds an instance of a data entity.
        /// </summary>
        /// <returns>A reference to this builder</returns>
        /// <remarks>
        /// Implementations of this method should only instantiate (not save) 
        /// the primary entity managed by this builder. Entities on which 
        /// the primary entity depends <b>should</b> be saved here.
        /// </remarks>
        ITestDataBuilder<T> Build();

        /// <summary>
        /// Gets the entity instance managed by this builder.
        /// </summary>
        /// <returns>the entity instance or null if <see cref="Build"/> has not yet been 
        /// invoked</returns>
        T Get();

        /// <summary>
        /// Saves the data entity managed by this builder.
        /// </summary>
        /// <returns>The entity instanced managed by this builder.</returns>
        T Save();
    }
}