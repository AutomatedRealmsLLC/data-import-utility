using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using DataImportUtility.SourceGenerator.Helpers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DataImportUtility.SourceGenerator;

/// <summary>
/// Generates the MappingRuleType enum based on classes that inherit from MappingRuleBase,
/// as well as the ValueTransformationType enum based on classes that inherit from 
/// ValueTransformationBase.
/// </summary>
[Generator]
public partial class MappingRuleSourceGenerator : IIncrementalGenerator
{
    private const string _defaultNamespace = "DataImportUtility.Abstractions";
    private const string _mappingRuleBaseClassName = "MappingRuleBase";
    private const string _valueTransformationClassName = "ValueTransformationBase";

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

        if (baseClass == _mappingRuleBaseClassName || baseClass == _valueTransformationClassName)
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

            if (baseClass == _mappingRuleBaseClassName || baseClass == _valueTransformationClassName)
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
