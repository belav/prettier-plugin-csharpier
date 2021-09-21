using System;
using CSharpier.DocTypes;
using CSharpier.SyntaxPrinter;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpier.SyntaxPrinter.SyntaxNodePrinters
{
    public static class FixedStatement
    {
        public static Doc Print(FixedStatementSyntax node)
        {
            return Doc.Concat(
                ExtraNewLines.Print(node),
                Doc.Group(
                    Token.Print(node.FixedKeyword),
                    " ",
                    Token.Print(node.OpenParenToken),
                    Doc.Group(Doc.Indent(Doc.SoftLine, Node.Print(node.Declaration)), Doc.SoftLine),
                    Token.Print(node.CloseParenToken),
                    Doc.IfBreak(Doc.Null, Doc.SoftLine)
                ),
                node.Statement is BlockSyntax blockSyntax
                    ? Block.Print(blockSyntax)
                    : Doc.IndentIf(
                          node.Statement is not FixedStatementSyntax,
                          Doc.Concat(Doc.HardLine, Node.Print(node.Statement))
                      )
            );
        }
    }
}
