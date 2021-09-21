using System;
using System.Collections.Generic;
using System.Linq;
using CSharpier.DocTypes;
using CSharpier.SyntaxPrinter;
using CSharpier.Utilities;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpier.SyntaxPrinter.SyntaxNodePrinters
{
    public static class ForStatement
    {
        public static Doc Print(ForStatementSyntax node)
        {
            var docs = new List<Doc>
            {
                ExtraNewLines.Print(node),
                Doc.Group(
                    Token.Print(node.ForKeyword),
                    " ",
                    Token.Print(node.OpenParenToken),
                    Doc.Group(
                        Doc.Indent(
                            Doc.SoftLine,
                            Doc.Group(
                                node.Declaration != null
                                    ? VariableDeclaration.Print(node.Declaration)
                                    : Doc.Null,
                                SeparatedSyntaxList.Print(node.Initializers, Node.Print, " "),
                                Token.Print(node.FirstSemicolonToken)
                            ),
                            node.Condition != null
                                ? Doc.Concat(Doc.Line, Node.Print(node.Condition))
                                : Doc.SoftLine,
                            Token.Print(node.SecondSemicolonToken),
                            node.Incrementors.Any() ? Doc.Line : Doc.SoftLine,
                            Doc.Group(
                                Doc.Indent(
                                    SeparatedSyntaxList.Print(
                                        node.Incrementors,
                                        Node.Print,
                                        Doc.Line
                                    )
                                )
                            )
                        ),
                        Doc.SoftLine
                    ),
                    Token.Print(node.CloseParenToken)
                ),
                OptionalBraces.Print(node.Statement)
            };

            return Doc.Concat(docs);
        }
    }
}
