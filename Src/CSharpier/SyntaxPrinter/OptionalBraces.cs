using System.Collections.Generic;
using System.Linq;
using CSharpier.DocTypes;
using CSharpier.SyntaxPrinter.SyntaxNodePrinters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpier.SyntaxPrinter
{
    public static class OptionalBraces
    {
        public static Doc Print(StatementSyntax node)
        {
            return node is BlockSyntax blockSyntax
                ? Block.Print(blockSyntax)
                : Doc.Indent(Doc.HardLine, Node.Print(node));
        }
    }
}
