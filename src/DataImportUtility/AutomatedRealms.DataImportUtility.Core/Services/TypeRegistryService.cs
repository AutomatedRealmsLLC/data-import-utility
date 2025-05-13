// Original file path: d:\git\AutomatedRealms\data-import-utility\src\DataImportUtility\AutomatedRealms.DataImportUtility.Core\Services\TypeRegistryService.cs
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Services;

namespace AutomatedRealms.DataImportUtility.Core.Services;

/// <summary>
/// Provides a central registry for mapping TypeId strings to System.Type objects.
/// This service is thread-safe.
/// </summary>
public class TypeRegistryService : ITypeRegistryService
{
    private readonly ConcurrentDictionary<string, Type> _typeMap = new();

    /// <inheritdoc />
    public void RegisterType(string typeId, Type type)
    {
        if (string.IsNullOrWhiteSpace(typeId))
        {
            throw new ArgumentException("TypeId cannot be null or whitespace.", nameof(typeId));
        }
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        if (!_typeMap.TryAdd(typeId, type))
        {
            throw new InvalidOperationException($"A type with TypeId '{(typeId)}' is already registered.");
        }
    }

    /// <inheritdoc />
    public void RegisterType<T>(string typeId)
    {
        RegisterType(typeId, typeof(T));
    }

    /// <inheritdoc />
    public Type? ResolveType(string typeId)
    {
        if (string.IsNullOrWhiteSpace(typeId))
        {
            return null;
        }
        _typeMap.TryGetValue(typeId, out var type);
        return type;
    }

    /// <inheritdoc />
    public bool TryResolveType(string typeId, out Type? type)
    {
        type = null;
        if (string.IsNullOrWhiteSpace(typeId))
        {
            return false;
        }
        return _typeMap.TryGetValue(typeId, out type);
    }

    /// <inheritdoc />
    public IEnumerable<KeyValuePair<string, Type>> GetAllRegisteredTypes()
    {
        return _typeMap.ToArray(); // Return a copy to prevent modification of the internal collection
    }

    /// <inheritdoc />
    public IEnumerable<string> GetAllTypeIds()
    {
        return _typeMap.Keys.ToArray(); // Return a copy
    }

    /// <inheritdoc />
    public ComparisonOperationBase? ResolveComparisonOperation(string typeId)
    {
        if (TryResolveType(typeId, out Type? type) && type != null)
        {
            if (typeof(ComparisonOperationBase).IsAssignableFrom(type))
            {
                try
                {
                    // Attempt to create an instance of the resolved type.
                    // This assumes the type has a parameterless constructor.
                    // If constructors with parameters are needed, a more sophisticated activation (e.g., using IServiceProvider) would be required.
                    var instance = Activator.CreateInstance(type) as ComparisonOperationBase;
                    if (instance == null)
                    {
                        // Log or handle the case where Activator.CreateInstance returns null (e.g., abstract type without concrete registration for that specific TypeId)
                        Console.WriteLine($"Warning: Activator.CreateInstance returned null for type '{type.FullName}' with TypeId '{typeId}'. The type might be abstract or have no parameterless constructor.");
                    }
                    return instance;
                }
                catch (Exception ex)
                {
                    // Log or handle exceptions during activation (e.g., missing constructor, constructor throws exception)
                    Console.WriteLine($"Error: Could not create an instance of type '{type.FullName}' for TypeId '{typeId}'. Exception: {ex.Message}");
                    return null;
                }
            }
            else
            {
                // Log or handle the case where the resolved type is not a ComparisonOperationBase
                Console.WriteLine($"Warning: Type '{type.FullName}' (TypeId: '{typeId}') is not assignable to ComparisonOperationBase.");
                return null;
            }
        }
        return null; // TypeId not found or type is null
    }
}
