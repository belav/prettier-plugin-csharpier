using System.Collections.Generic;
using CSharpier.DocTypes;
using CSharpier.SyntaxPrinter.SyntaxNodePrinters;
using CSharpier.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpier.SyntaxPrinter
{
    public static class ArgumentListLike
    {
        public static Doc Print(
            SyntaxToken openParenToken,
            SeparatedSyntaxList<ArgumentSyntax> arguments,
            SyntaxToken closeParenToken
        ) {
            var docs = new List<Doc> { Token.Print(openParenToken) };

            if (arguments.Any())
            {
                docs.Add(
                    Doc.Indent(
                        Doc.SoftLine,
                        SeparatedSyntaxList.Print(arguments, Argument.Print, Doc.Line)
                    ),
                    Doc.SoftLine
                );
            }

            docs.Add(Token.Print(closeParenToken));

            return Doc.Concat(docs);
        }
    }
}
