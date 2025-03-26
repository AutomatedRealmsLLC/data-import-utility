﻿using DataImportUtility.Helpers;
using DataImportUtility.SourceGenerator.Helpers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DataImportUtility.SourceGenerator
{
    public partial class MappingRuleSourceGenerator
    {
        private const string _enumTypeNamePlaceholder = "|EnumTypeName|";
        private const string _baseClassNamePlaceholder = "|BaseClassName|";

        /// <summary>
        /// The template for the enum.
        /// </summary>
        /// <remarks>
        /// {0} is the list of enum values.
        /// </remarks>
        private static readonly string _enumTypeSource = $@"// <auto-generated />
#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace {_defaultNamespace};

/// <summary>
/// The type of transformation rule.
/// </summary>
/// <remarks>
/// To create a new rule, create a new class that inherits from <see cref=""{_baseClassNamePlaceholder}"" />.
/// </remarks>
public enum {_enumTypeNamePlaceholder}
{{{{
{{0}}
}}}}
";

        /// <summary>
        /// The template for the extension methods for the enum.
        /// </summary>
        /// <remarks>
        /// {0} is the contents of a switch expression to return the display text.
        /// {1} is the contents of a switch expression to return the description.
        /// {2} is the contents of a switch expression to return the concrete class.
        /// {3} is the contents of a switch expression to return the enum value for a class type.
        /// 
        /// There must be a comma after the last variable since it will automatically
        /// include the default case.
        /// </remarks>
        private static readonly string _enumTypeExtensions = $@"// <auto-generated />
#nullable enable

namespace {_defaultNamespace};

/// <summary>
/// Extension methods for the <see cref=""{_enumTypeNamePlaceholder}""/> enumeration.
/// </summary>
public static partial class {_enumTypeNamePlaceholder}Extensions
{{{{
    /// <summary>
    /// Gets the text to display for the {_enumTypeNamePlaceholder}.
    /// </summary>
    /// <param name=""enumMem"">The {_enumTypeNamePlaceholder} to get the display text for.</param>
    /// <param name=""shortVersion"">Whether to get the short version of the display text.</param>
    /// <returns>The user display text for the {_enumTypeNamePlaceholder}.</returns>
    public static string GetDisplayText(this {_enumTypeNamePlaceholder} enumMem, bool shortVersion = false)
    {{{{
        return enumMem switch
        {{{{
{{0}}
            _ => $""{{{{enumMem}}}} does not have a description defined.""
        }}}};
    }}}}

    /// <summary>
    /// Gets the user-friendly description for the {_enumTypeNamePlaceholder}.
    /// </summary>
    /// <param name=""enumMem"">The {_enumTypeNamePlaceholder} to get the description for.</param>
    /// <returns>The user-friendly description for the {_enumTypeNamePlaceholder}.</returns>
    public static string GetDescription(this {_enumTypeNamePlaceholder} enumMem)
    {{{{
        return enumMem switch
        {{{{
{{1}}
            _ => $""{{{{enumMem}}}} does not have a description defined.""
        }}}};
    }}}}

    /// <summary>
    /// Gets the concrete class the {_enumTypeNamePlaceholder} enum is representing.
    /// </summary>
    /// <param name=""enumMem"">The {_enumTypeNamePlaceholder} to get the concrete class for.</param>
    /// <returns>The concrete class for the {_enumTypeNamePlaceholder}.</returns>
    /// <exception cref=""ArgumentException"">Thrown when the provided {_enumTypeNamePlaceholder} does not have a valid concrete class.</exception>
    public static Type GetClassType(this {_enumTypeNamePlaceholder} enumMem)
    {{{{
        return enumMem switch
        {{{{
{{2}}
            _ => throw new ArgumentException($""The {{{{enumMem}}}} class is not a valid {_enumTypeNamePlaceholder}."")
        }}}};
    }}}}

    /// <summary>
    /// Gets the enum value for the given class type.
    /// </summary>
    /// <param name=""enumMem"">The {_enumTypeNamePlaceholder} to get the enum value for.</param>
    /// <returns>The enum value for the {_baseClassNamePlaceholder}.</returns>
    /// <exception cref=""ArgumentException"">Thrown when the provided {_enumTypeNamePlaceholder} does not have a valid concrete class.</exception>
    public static {_enumTypeNamePlaceholder} GetEnumValue(this {_baseClassNamePlaceholder} enumMem)
    {{{{
        return enumMem switch
        {{{{
{{3}}
            _ => throw new ArgumentException($""The {{{{enumMem}}}} class is not a valid {_enumTypeNamePlaceholder}."")
        }}}};
    }}}}


    /// <summary>
    /// Gets a new instance of the concrete class for the provided {_enumTypeNamePlaceholder}.
    /// </summary>
    /// <param name=""enumMem"">The {_enumTypeNamePlaceholder} to get an instantiation of the associated concreate class for.</param>
    /// <returns>An instantiation of the associated concreate class for the provided {_enumTypeNamePlaceholder}.</returns>
    /// <exception cref=""ArgumentException"">Thrown when the provided {_enumTypeNamePlaceholder} does not have a valid concrete class.</exception>
    public static {_baseClassNamePlaceholder} CreateNewInstance(this {_enumTypeNamePlaceholder} enumMem)
    {{{{
        return Activator.CreateInstance(enumMem.GetClassType()) as {_baseClassNamePlaceholder} 
            ?? throw new ArgumentException($""The {{{{enumMem}}}} class is not a valid {_enumTypeNamePlaceholder}."");
    }}}}

}}}}
";

        private static void GenerateEnumsAndExtensions(SourceProductionContext context, IEnumerable<(ClassDeclarationSyntax ClassSyntax, EnumInfo EnumInfo)> enumInfoDetails)
        {
            if (!enumInfoDetails.Any()) { return; }

            foreach (var enumInfoGroup in enumInfoDetails.GroupBy(x => x.EnumInfo.BaseClassName).Where(x => x.Key?.EndsWith("Base") ?? false))
            {
                foreach (var (classSyntax, enumInfo) in enumInfoGroup)
                {
                    PopulateEnumInfo(classSyntax, enumInfo);
                }

                // Replace duplicate enum names with an increasing numeric suffix
                foreach (var curEnumInfoGroup in enumInfoGroup.Select(x => x.EnumInfo).GroupBy(x => x.EnumMemberName))
                {
                    if (curEnumInfoGroup.Count() == 1) { continue; }
                    var curIndex = 1;
                    foreach (var curEnumInfo in curEnumInfoGroup.Skip(1))
                    {
                        curEnumInfo.EnumMemberName += curIndex++;
                    }
                }

                var enumInfos = enumInfoGroup.Select(x => x.EnumInfo).OrderBy(x => x.EnumMemberOrder).ThenBy(x => x.EnumMemberName).ToList();
                GenerateEnumType(context, enumInfos);
                GenerateExtensionsClass(context, enumInfos);
            }
        }

        private static void PopulateEnumInfo(ClassDeclarationSyntax ruleClass, EnumInfo ruleInfo)
        {
            // Get the enum value from the EnumValue property
            var props = ruleClass.Members.OfType<PropertyDeclarationSyntax>();

            // Get the definition for the Name property
            var enumMemberNameProperty = props.FirstOrDefault(x => x.Identifier.Text == "EnumMemberName");
            if (enumMemberNameProperty is not null)
            {
                var name = enumMemberNameProperty.GetPropertyValue();
                ruleInfo.EnumMemberName = string.IsNullOrWhiteSpace(name) ? string.Empty : name;
            }
            else
            {
                // This will cause the enum member to take the class name variant
                ruleInfo.EnumMemberName = string.Empty;
            }

            // Get the definition for the Name property
            var displayNameProperty = props.FirstOrDefault(x => x.Identifier.Text == "DisplayName");
            if (displayNameProperty is not null)
            {
                var name = displayNameProperty.GetPropertyValue();
                ruleInfo.DisplayName = string.IsNullOrWhiteSpace(name) ? ruleInfo.EnumMemberName : name;
            }

            // Get the definition for the ShortName property
            var shortDisplayNameProperty = props.FirstOrDefault(x => x.Identifier.Text == "ShortName");
            if (shortDisplayNameProperty is not null)
            {
                var name = shortDisplayNameProperty.GetPropertyValue();
                ruleInfo.ShortDisplayName = string.IsNullOrWhiteSpace(name) ? ruleInfo.DisplayName : name;
            }

            // Get the definition for the Description property
            var descriptionProperty = props.FirstOrDefault(x => x.Identifier.Text == "Description");
            if (descriptionProperty is not null)
            {
                var description = descriptionProperty.GetPropertyValue();
                ruleInfo.Description = string.IsNullOrWhiteSpace(description) ? string.Empty : description;
            }
        }

        private static void GenerateEnumType(SourceProductionContext context, List<EnumInfo> enumMemberInfos)
        {
            if (!enumMemberInfos.Any()) { return; }
            var enumTypeName = enumMemberInfos.First().EnumTypeName;
            var baseClassName = enumMemberInfos.First().BaseClassName;

            var enumTypeSource = string.Format(
                _enumTypeSource.Replace(_baseClassNamePlaceholder, baseClassName).Replace(_enumTypeNamePlaceholder, enumTypeName),
                string.Join(",\r\n", enumMemberInfos.Select(x => x.GetEnumOutput()))
            );

            context.AddSource($"{enumTypeName}.g.cs", enumTypeSource);
        }

        private static void GenerateExtensionsClass(SourceProductionContext context, List<EnumInfo> mappingRules)
        {
            if (!mappingRules.Any()) { return; }
            var enumTypeName = mappingRules.First().EnumTypeName;
            var baseClassName = mappingRules.First().BaseClassName;

            var enumTypeSource = string.Format(
                _enumTypeExtensions.Replace(_baseClassNamePlaceholder, baseClassName).Replace(_enumTypeNamePlaceholder, enumTypeName),
                string.Join("\r\n", mappingRules.Select(x => string.Format(x.GetEnumDisplayTextOutputTemplate(), enumTypeName))),
                string.Join("\r\n", mappingRules.Select(x => string.Format(x.GetEnumDescriptionOutputTemplate(), enumTypeName))),
                string.Join("\r\n", mappingRules.Select(x => string.Format(x.GetEnumToClassMappingTemplate(), enumTypeName))),
                string.Join("\r\n", mappingRules.Select(x => string.Format(x.GetClassToEnumMappingTemplate(), enumTypeName)))
            );

            context.AddSource($"{enumTypeName}Extensions.g.cs", enumTypeSource);
        }
    }
}
