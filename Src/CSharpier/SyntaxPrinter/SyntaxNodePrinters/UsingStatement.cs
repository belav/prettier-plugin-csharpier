using System;
using System.Collections.Generic;
using CSharpier.DocTypes;
using CSharpier.SyntaxPrinter;
using CSharpier.Utilities;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpier.SyntaxPrinter.SyntaxNodePrinters
{
    public static class UsingStatement
    {
        public static Doc Print(UsingStatementSyntax node)
        {
            var docs = new List<Doc>
            {
                ExtraNewLines.Print(node),
                Doc.Group(
                    Token.Print(node.AwaitKeyword),
                    node.AwaitKeyword.Kind() != SyntaxKind.None ? " " : Doc.Null,
                    Token.Print(node.UsingKeyword),
                    " ",
                    Token.Print(node.OpenParenToken),
                    Doc.Group(
                        Doc.Indent(
                            Doc.SoftLine,
                            node.Declaration != null
                                ? VariableDeclaration.Print(node.Declaration)
                                : Doc.Null,
                            node.Expression != null ? Node.Print(node.Expression) : Doc.Null
                        ),
                        Doc.SoftLine
                    ),
                    Token.Print(node.CloseParenToken),
                    Doc.IfBreak(Doc.Null, Doc.SoftLine)
                )
            };
            if (node.Statement is UsingStatementSyntax)
            {
                docs.Add(Doc.HardLine, Node.Print(node.Statement));
            }
            else if (node.Statement is BlockSyntax blockSyntax)
            {
                docs.Add(Block.Print(blockSyntax));
            }
            else
            {
                docs.Add(Doc.Indent(Doc.HardLine, Node.Print(node.Statement)));
            }

            return Doc.Concat(docs);
        }
    }
}
