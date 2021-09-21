using System.Collections.Generic;
using System.Linq;
using CSharpier.DocTypes;
using CSharpier.SyntaxPrinter;
using CSharpier.Utilities;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpier.SyntaxPrinter.SyntaxNodePrinters
{
    public static class Block
    {
        public static Doc Print(BlockSyntax node)
        {
            if (
                node.Statements.Count == 0
                && node.Parent is MethodDeclarationSyntax
                && !Token.HasComments(node.CloseBraceToken)
            ) {
                return Doc.Concat(
                    " ",
                    Token.Print(node.OpenBraceToken),
                    " ",
                    Token.Print(node.CloseBraceToken)
                );
            }

            Doc statementSeparator =
                node.Parent is AccessorDeclarationSyntax && node.Statements.Count <= 1
                    ? Doc.Line
                    : Doc.HardLine;

            Doc innerDoc = Doc.Null;

            if (node.Statements.Count > 0)
            {
                innerDoc = Doc.Indent(
                    statementSeparator,
                    Doc.Join(statementSeparator, node.Statements.Select(Node.Print))
                );

                DocUtilities.RemoveInitialDoubleHardLine(innerDoc);
            }

            var result = Doc.Group(
                node.Parent is ParenthesizedLambdaExpressionSyntax or BlockSyntax
                    ? Doc.Null
                    : Doc.Line,
                Token.Print(node.OpenBraceToken),
                node.Statements.Count == 0 ? " " : Doc.Concat(innerDoc, statementSeparator),
                Token.Print(node.CloseBraceToken)
            );

            return node.Parent is BlockSyntax
                ? Doc.Concat(ExtraNewLines.Print(node), result)
                : result;
        }
    }
}
