using System;
using System.Collections.Generic;
using CSharpier.DocTypes;
using CSharpier.SyntaxPrinter;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpier.SyntaxPrinter.SyntaxNodePrinters
{
    public static class AnonymousMethodExpression
    {
        public static Doc Print(AnonymousMethodExpressionSyntax node)
        {
            var docs = new List<Doc>
            {
                Modifiers.Print(node.Modifiers),
                Token.Print(node.DelegateKeyword)
            };

            if (node.ParameterList != null)
            {
                docs.Add(ParameterList.Print(node.ParameterList));
            }

            docs.Add(Block.Print(node.Block));

            return Doc.Concat(docs);
        }
    }
}
