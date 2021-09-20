using System;
using CSharpier.DocTypes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpier.SyntaxPrinter.SyntaxNodePrinters
{
    public static class ConditionalExpression
    {
        public static Doc Print(ConditionalExpressionSyntax node)
        {
            Doc[] contents =
            {
                Doc.Line,
                Token.PrintWithSuffix(node.QuestionToken, " "),
                Doc.Align(2, Node.Print(node.WhenTrue)),
                Doc.Line,
                Token.PrintWithSuffix(node.ColonToken, " "),
                Doc.Align(2, Node.Print(node.WhenFalse))
            };

            return Doc.Group(
                node.Parent is ReturnStatementSyntax
                && node.Condition is BinaryExpressionSyntax or IsPatternExpressionSyntax
                    ? Doc.Indent(
                          Doc.Group(Doc.IfBreak(Doc.SoftLine, Doc.Null), Node.Print(node.Condition))
                      )
                    : Node.Print(node.Condition),
                node.Parent
                    is ConditionalExpressionSyntax
                    or ArgumentSyntax
                    or ReturnStatementSyntax
                    ? Doc.Align(2, contents)
                    : Doc.Indent(contents)
            );
        }
    }
}
