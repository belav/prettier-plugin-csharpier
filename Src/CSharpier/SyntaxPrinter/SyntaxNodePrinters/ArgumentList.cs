using CSharpier.DocTypes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpier.SyntaxPrinter.SyntaxNodePrinters
{
    public static class ArgumentList
    {
        public static Doc Print(ArgumentListSyntax node)
        {
            return Doc.Group(
                Doc.IndentIf(
                    // indent if this is the first argumentList in a method chain
                    node.Parent
                        is InvocationExpressionSyntax
                        {
                            Expression: IdentifierNameSyntax,
                            Parent: { Parent: InvocationExpressionSyntax }
                        },
                    ArgumentListLike.Print(
                        node.OpenParenToken,
                        node.Arguments,
                        node.CloseParenToken
                    )
                )
            );
        }
    }
}
