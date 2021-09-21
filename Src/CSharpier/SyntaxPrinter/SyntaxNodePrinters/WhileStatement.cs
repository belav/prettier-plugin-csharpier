using System;
using CSharpier.DocTypes;
using CSharpier.SyntaxPrinter;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpier.SyntaxPrinter.SyntaxNodePrinters
{
    public static class WhileStatement
    {
        public static Doc Print(WhileStatementSyntax node)
        {
            var result = Doc.Concat(
                ExtraNewLines.Print(node),
                Doc.Group(
                    Token.Print(node.WhileKeyword),
                    " ",
                    Token.Print(node.OpenParenToken),
                    Doc.Group(Doc.Indent(Doc.SoftLine, Node.Print(node.Condition)), Doc.SoftLine),
                    Token.Print(node.CloseParenToken)
                ),
                OptionalBraces.Print(node.Statement)
            );

            return result;
        }
    }
}
