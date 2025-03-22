using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DataImportUtility.SourceGenerator.Helpers
{
    internal static class GeneralSourceGenHelpers
    {
        // determine the namespace the class/enum/struct is declared in, if any
        public static string GetNamespace(this BaseTypeDeclarationSyntax syntax)
        {
            // If we don't have a namespace at all we'll return an empty string
            // This accounts for the "default namespace" case
            string nameSpace = string.Empty;

            // Get the containing syntax node for the type declaration
            // (could be a nested type, for example)
            SyntaxNode? potentialNamespaceParent = syntax.Parent;

            // Keep moving "out" of nested classes etc until we get to a namespace
            // or until we run out of parents
            while (potentialNamespaceParent is not null &&
                    potentialNamespaceParent is not NamespaceDeclarationSyntax
                    && potentialNamespaceParent is not FileScopedNamespaceDeclarationSyntax)
            {
                potentialNamespaceParent = potentialNamespaceParent.Parent;
            }

            // Build up the final namespace by looping until we no longer have a namespace declaration
            if (potentialNamespaceParent is BaseNamespaceDeclarationSyntax namespaceParent)
            {
                // We have a namespace. Use that as the type
                nameSpace = namespaceParent.Name.ToString();

                // Keep moving "out" of the namespace declarations until we 
                // run out of nested namespace declarations
                while (true)
                {
                    if (namespaceParent.Parent is not NamespaceDeclarationSyntax parent)
                    {
                        break;
                    }

                    // Add the outer namespace as a prefix to the final namespace
                    nameSpace = $"{namespaceParent.Name}.{nameSpace}";
                    namespaceParent = parent;
                }
            }

            // return the final namespace
            return nameSpace;
        }

        public static string? GetPropertyValue(this PropertyDeclarationSyntax property)
        {
            // Handle expression-bodied members
            if (property.ExpressionBody is not null)
            {
                return property.ExpressionBody.Expression.GetExpressionValue();
            }

            // Handle getter-only accessors
            var getter = property.AccessorList?.Accessors.FirstOrDefault(a => a.Keyword.Text == "get");
            if (getter is not null)
            {
                if (getter.Body is not null)
                {
                    // Handle getter with body
                    var returnStatement = getter.Body.Statements.OfType<ReturnStatementSyntax>().FirstOrDefault();
                    if (returnStatement is not null)
                    {
                        return returnStatement.Expression?.GetExpressionValue();
                    }
                }
                else if (getter.ExpressionBody is not null)
                {
                    // Handle getter with expression body
                    return GetExpressionValue(getter.ExpressionBody.Expression);
                }
            }

            // Handle initializer values
            if (property.Initializer is not null)
            {
                return GetExpressionValue(property.Initializer.Value);
            }

            return property.Identifier.Text;
        }

        public static string? GetExpressionValue(this ExpressionSyntax expression)
        {
            switch (expression)
            {
                case LiteralExpressionSyntax literal:
                    return literal.Token.ValueText;
                case InvocationExpressionSyntax invocation when invocation.Expression is IdentifierNameSyntax identifier && identifier.Identifier.Text == "nameof":
                    var argument = invocation.ArgumentList.Arguments.FirstOrDefault();
                    return argument?.ToString().Trim('"');
                default:
                    return expression.ToString();
            }
        }
    }
}
