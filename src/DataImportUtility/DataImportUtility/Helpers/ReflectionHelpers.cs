using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

using DataImportUtility.Abstractions;
using DataImportUtility.Models;

namespace DataImportUtility.Helpers;

internal static class ReflectionHelpers
{
    public static FieldMapping AsFieldMapping(this PropertyInfo prop, bool forceRequired = false)
        => new()
        {
            FieldName = prop.Name,
            FieldType = prop.PropertyType,
            Required = forceRequired || Attribute.IsDefined(prop, typeof(RequiredAttribute)),
            MappingRule = MappingRuleType.CopyRule.CreateNewInstance(),
            ValidationAttributes = prop.GetValidationAttributes()
        };

    public static ImmutableArray<ValidationAttribute> GetValidationAttributes(this PropertyInfo prop)
        => prop.GetCustomAttributes<ValidationAttribute>().ToImmutableArray();
}
