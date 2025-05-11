using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Core.Models; // For FieldMapping
using System;
// Assuming MappingRuleTypeExtensions.CreateNewInstance() will be moved to Core.Helpers or Abstractions

namespace AutomatedRealms.DataImportUtility.Core.Helpers;

internal static class ReflectionHelpers
{
    public static FieldMapping AsFieldMapping(this PropertyInfo prop, bool forceRequired = false)
        => new()
        {
            FieldName = prop.Name,
            FieldType = prop.PropertyType,
            Required = forceRequired || Attribute.IsDefined(prop, typeof(RequiredAttribute)),
            // This assumes CreateNewInstance() is an extension method for MappingRuleType.
            // It might need to be updated if its location or signature changes during refactoring.
            MappingRule = MappingRuleType.CopyRule.CreateNewInstance(), 
            ValidationAttributes = prop.GetValidationAttributes()
        };

    public static ImmutableArray<ValidationAttribute> GetValidationAttributes(this PropertyInfo prop)
        => prop.GetCustomAttributes<ValidationAttribute>().ToImmutableArray();
        
    /// <summary>
    /// Changes the type of an object to the specified type.
    /// </summary>
    /// <param name="value">The value to change the type of.</param>
    /// <param name="conversionType">The type to convert to.</param>
    /// <returns>The converted value.</returns>
    public static object? ChangeType(object? value, Type conversionType)
    {
        if (value is null)
            return null;
            
        if (conversionType.IsNullableType())
            conversionType = Nullable.GetUnderlyingType(conversionType) ?? conversionType;
            
        return Convert.ChangeType(value, conversionType);
    }
    
    /// <summary>
    /// Determines whether the specified type is a nullable type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is a nullable type, otherwise false.</returns>
    public static bool IsNullableType(this Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }
}
