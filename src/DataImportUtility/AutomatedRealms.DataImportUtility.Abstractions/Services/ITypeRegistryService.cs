namespace AutomatedRealms.DataImportUtility.Abstractions.Services;

/// <summary>
/// Defines a service for registering and resolving types based on a string TypeId.
/// </summary>
public interface ITypeRegistryService
{
    /// <summary>
    /// Registers a type with a given TypeId.
    /// </summary>
    /// <param name="typeId">The unique string identifier for the type.</param>
    /// <param name="type">The type to register.</param>
    /// <exception cref="ArgumentException">Thrown if typeId is null or whitespace, or if type is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if a type with the same TypeId is already registered.</exception>
    void RegisterType(string typeId, Type type);

    /// <summary>
    /// Registers a type using a generic parameter.
    /// </summary>
    /// <typeparam name="T">The type to register.</typeparam>
    /// <param name="typeId">The unique string identifier for the type.</param>
    /// <exception cref="ArgumentException">Thrown if typeId is null or whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown if a type with the same TypeId is already registered.</exception>
    void RegisterType<T>(string typeId);

    /// <summary>
    /// Resolves a TypeId to a System.Type.
    /// </summary>
    /// <param name="typeId">The TypeId to resolve.</param>
    /// <returns>The System.Type if found; otherwise, null.</returns>
    Type? ResolveType(string typeId);

    /// <summary>
    /// Tries to resolve a TypeId to a System.Type.
    /// </summary>
    /// <param name="typeId">The TypeId to resolve.</param>
    /// <param name="type">When this method returns, contains the type associated with the specified TypeId, if the TypeId is found; otherwise, the default value for the type of the type parameter. This parameter is passed uninitialized.</param>
    /// <returns>true if the TypeRegistryService contains an element with the specified TypeId; otherwise, false.</returns>
    bool TryResolveType(string typeId, out Type? type);

    /// <summary>
    /// Gets all registered TypeIds and their corresponding Types.
    /// </summary>
    /// <returns>An enumerable of KeyValuePair containing TypeId and Type.</returns>
    IEnumerable<KeyValuePair<string, Type>> GetAllRegisteredTypes();

    /// <summary>
    /// Gets all registered TypeIds.
    /// </summary>
    /// <returns>An enumerable of registered TypeId strings.</returns>
    IEnumerable<string> GetAllTypeIds();

    /// <summary>
    /// Resolves a TypeId to an instance of ComparisonOperationBase.
    /// </summary>
    /// <param name="typeId">The TypeId of the comparison operation to resolve.</param>
    /// <returns>An instance of ComparisonOperationBase if found and activatable; otherwise, null.</returns>
    ComparisonOperationBase? ResolveComparisonOperation(string typeId);
}
