using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using DataImportUtility.Helpers;
using DataImportUtility.SourceGenerator.Helpers;

namespace DataImportUtility.SourceGenerator
{
    /// <summary>
    /// Generates the MappingRuleType enum based on classes that inherit from MappingRuleBase.
    /// </summary>
    [Generator]
    public partial class MappingRuleSourceGenerator : IIncrementalGenerator
    {
        private const string _defaultNamespace = "DataImportUtility.Abstractions";
        private const string _mappingRuleBaseClassName = "MappingRuleBase";
        private const string _valueTransformationClassName = "ValueTransformationBase";
        private const string _comparisonOperationClassName = "ComparisonOperationBase";

        /// <inheritdoc />
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
#if DEBUGGENERATOR
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif
            var classDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (s, _) => IsClassDeclaration(s),
                    transform: static (ctx, _) => GetClassDeclaration(ctx))
                .Where(static m => m is not null);

            var compilationAndClasses = context.CompilationProvider.Combine(classDeclarations.Collect());

            context.RegisterSourceOutput(compilationAndClasses, static (spc, source) => Execute(source.Left, source.Right, spc));
        }

        private static bool IsClassDeclaration(SyntaxNode node)
        {
            return node is ClassDeclarationSyntax;
        }

        private static ClassDeclarationSyntax? GetClassDeclaration(GeneratorSyntaxContext context)
        {
            var classDeclaration = (ClassDeclarationSyntax)context.Node;
            var baseClass = classDeclaration.BaseList?.Types.FirstOrDefault()?.Type?.GetFirstToken().ValueText;

            if (baseClass == _mappingRuleBaseClassName || baseClass == _valueTransformationClassName || baseClass == _comparisonOperationClassName)
            {
                return classDeclaration;
            }

            return null;
        }

        private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax?> classes, SourceProductionContext context)
        {
            var enumInfos = new List<(ClassDeclarationSyntax ClassSyntax, EnumInfo EnumInfo)>();

            foreach (var classSyntax in classes)
            {
                if (classSyntax is null) continue;

                var baseClass = classSyntax.BaseList?.Types.FirstOrDefault()?.Type?.GetFirstToken().ValueText;

                if (baseClass == _mappingRuleBaseClassName || baseClass == _valueTransformationClassName || baseClass == _comparisonOperationClassName)
                {
                    enumInfos.Add((
                        classSyntax,
                        new EnumInfo()
                        {
                            Namespace = classSyntax.GetNamespace(),
                            BaseClassName = baseClass,
                            ClassComments = classSyntax.GetLeadingTrivia().ToFullString().Trim(),
                            ConcreteClassName = classSyntax.Identifier.Text,
                            DisplayName = classSyntax.Identifier.Text,
                            Description = classSyntax.Identifier.Text
                        }));
                }
            }

            GenerateEnumsAndExtensions(context, enumInfos);
        }
    }
}
