using System.Collections.Generic;
using CSharpier.DocTypes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpier.SyntaxPrinter.SyntaxNodePrinters
{
    public static class Argument
    {
        public static Doc Print(ArgumentSyntax node)
        {
            var docs = new List<Doc>();
            if (node.NameColon != null)
            {
                docs.Add(NameColon.Print(node.NameColon));
            }

            if (node.RefKindKeyword.RawKind != 0)
            {
                docs.Add(Token.PrintWithSuffix(node.RefKindKeyword, " "));
            }

            docs.Add(Node.Print(node.Expression));
            return Doc.Concat(docs);
        }
    }
}
