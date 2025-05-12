using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using AutomatedRealms.DataImportUtility.Abstractions.Enums; // Added
using AutomatedRealms.DataImportUtility.Abstractions.Models;
using System;

namespace AutomatedRealms.DataImportUtility.Core.Helpers;

internal static class ReflectionHelpers
{
    public static FieldMapping AsFieldMapping(this PropertyInfo prop, bool forceRequired = false)
    {
        var mappingRuleType = MappingRuleType.CopyRule; // Default to CopyRule
        var classType = mappingRuleType.GetClassType(); // Use the new extension method
        var mappingRule = classType != null ? Activator.CreateInstance(classType) as MappingRuleBase : null;

        return new FieldMapping()
        {
            FieldName = prop.Name,
            FieldType = prop.PropertyType,
            Required = forceRequired || Attribute.IsDefined(prop, typeof(RequiredAttribute)),
            MappingRule = mappingRule,
            ValidationAttributes = prop.GetValidationAttributes()
        };
    }

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
