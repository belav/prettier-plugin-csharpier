namespace CSharpier.SyntaxPrinter.SyntaxNodePrinters;

internal static class AnonymousMethodExpression
{
    public static Doc Print(AnonymousMethodExpressionSyntax node, PrintingContext context)
    {
        var docs = new ValueListBuilder<Doc>([null, null, null, null]);
        docs.Append(Modifiers.Print(node.Modifiers, context));
        docs.Append(Token.Print(node.DelegateKeyword, context));

        if (node.ParameterList != null)
        {
            docs.Append(ParameterList.Print(node.ParameterList, context));
        }

        docs.Append(Block.Print(node.Block, context));

        return Doc.Concat(docs.AsSpan().ToArray());
    }
}
