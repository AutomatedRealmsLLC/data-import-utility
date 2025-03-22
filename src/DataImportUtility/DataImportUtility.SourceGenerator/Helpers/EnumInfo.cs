using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DataImportUtility.Helpers
{
    /// <summary>
    /// Information about a transformation rule.
    /// </summary>
    internal class EnumInfo
    {
        private const string _enumOutputTemplate = "    {0}\r\n    {1}";
        private const string _attributeTextTemplate = "[EnumMember(Value = \"{0}\"), Display(Name = \"{1}\", ShortName = \"{2}\", Description = \"{3}\")]";
        private const string _switchExpressionTemplate = "            {0} => {1},";
        private static readonly char[] _commentsLineSplitDelimiters = new char[] { '\r', '\n' };

        /// <summary>
        /// The namespace of the enum member's class.
        /// </summary>
        /// <remarks>
        /// This should be populated during the source generation process.
        /// </remarks>
        [AllowNull]
        public string Namespace { get; internal set; }
        /// <summary>
        /// The base class name of the enum member's class.
        /// </summary>
        /// <remarks>
        /// This should be populated during the source generation process.
        /// </remarks>
        public string? BaseClassName { get; internal set; }
        /// <summary>
        /// The name of the enum type.
        /// </summary>
        public string? EnumTypeName => string.IsNullOrWhiteSpace(BaseClassName) ? null : $"{BaseClassName!.Replace("Base", null)}Type";
        /// <summary>
        /// The name of the enum member's class.
        /// </summary>
        /// <remarks>
        /// This should be populated during the source generation process.
        /// </remarks>
        [AllowNull]
        public string ConcreteClassName { get; set; }
        /// <summary>
        /// The comments for the class.
        /// </summary>
        /// <remarks>
        /// This should be populated during the source generation process by getting
        /// the comments from the class.
        /// </remarks>
        [AllowNull]
        public string ClassComments { get; set; }
        /// <summary>
        /// The order the enum member will be in for the generated enum type.
        /// </summary>
        /// <remarks>
        /// This will be in the EnumMemberOrder property of the class.
        /// </remarks>
        public int EnumMemberOrder { get; set; }
        /// <summary>
        /// The display name for the enum member.
        /// </summary>
        /// <remarks>
        /// This will be in the EnumMemberName property of the enum member's class.
        /// </remarks>
        [AllowNull]
        public string EnumMemberName { get; set; }
        /// <summary>
        /// The display name for the enum member.
        /// </summary>
        /// <remarks>
        /// This will be in the DisplayName property of the enum member's class.
        /// </remarks>
        [AllowNull]
        public string DisplayName { get; set; }
        /// <summary>
        /// The display name for the enum member.
        /// </summary>
        /// <remarks>
        /// This will be in the ShortName property of the enum member's class.
        /// </remarks>
        [AllowNull]
        public string ShortDisplayName { get; set; }
        /// <summary>
        /// The description of the enum member.
        /// </summary>
        /// <remarks>
        /// This will be in the Description property of the enum member's class.
        /// </remarks>
        [AllowNull]
        public string Description { get; set; }

        /// <summary>
        /// Gets the output for the generated enum member.
        /// </summary>
        /// <returns>
        /// The output for the generated enum member.
        /// </returns>
        public string GetEnumOutput()
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(ClassComments))
            {
                sb.Append("    ");
                sb.AppendLine(
                string.Join(
                        "\r\n    ",
                        ClassComments
                            .Split(_commentsLineSplitDelimiters, StringSplitOptions.RemoveEmptyEntries)
                            .Select(line => line.Trim())
                    )
                );
            }
            sb.AppendFormat(_enumOutputTemplate, GetAttributeText(), GetEnumValue());
            return sb.ToString();
        }

        /// <summary>
        /// Gets the output for the generated enum member display text as a switch expression.
        /// </summary>
        /// <returns>
        /// {0} in the resulting template string should be replaced with the enum class name.
        /// </returns>
        public string GetEnumDisplayTextOutputTemplate()
        {
            return string.Format(
                _switchExpressionTemplate,
                $"{{0}}.{GetEnumValue()}",
                $"shortVersion ? \"{ShortDisplayName}\" : \"{DisplayName}\""
            );
        }

        /// <summary>
        /// Gets the output for the generated enum member description as a switch expression.
        /// </summary>
        /// <returns>
        /// {0} in the resulting template string should be replaced with the enum class name.
        /// </returns>
        public string GetEnumDescriptionOutputTemplate()
        {
            return string.Format(
                _switchExpressionTemplate,
                $"{{0}}.{GetEnumValue()}",
                $"\"{Description}\""
            );
        }

        /// <summary>
        /// Gets the output for the generated enum member to class mapping as a switch expression.
        /// </summary>
        /// <returns>
        /// {0} in the resulting template string should be replaced with the enum class name.
        /// </returns>
        public string GetEnumToClassMappingTemplate()
        {
            return string.Format(
                _switchExpressionTemplate,
                $"{{0}}.{GetEnumValue()}",
                $"typeof({Namespace}.{ConcreteClassName})"
            );
        }

        /// <summary>
        /// Gets the output for the generated class to enum member mapping as a switch expression.
        /// </summary>
        /// <returns>
        /// {0} in the resulting template string should be replaced with the enum class name.
        /// </returns>
        public string GetClassToEnumMappingTemplate()
        {
            return string.Format(
                _switchExpressionTemplate,
                $"{Namespace}.{ConcreteClassName} _",
                $"{{0}}.{GetEnumValue()}"
            );
        }

        private string GetAttributeText()
        {
            return string.Format(_attributeTextTemplate, DisplayName, DisplayName, ShortDisplayName, Description);
        }

        private string GetEnumValue()
        {
            return string.IsNullOrWhiteSpace(EnumMemberName)
                ? ConcreteClassName.Replace("Rule", null).Replace("Transformation", null).Replace("Operation", null)
                : Regex.Replace(EnumMemberName, @"[^a-zA-Z0-9]", "_");
        }
    }
}
