namespace CSharpier.SyntaxPrinter.SyntaxNodePrinters;

internal static class Argument
{
    public static Doc Print(ArgumentSyntax node, PrintingContext context)
    {
        return Doc.Concat(PrintModifiers(node, context), Node.Print(node.Expression, context));
    }

    public static Doc PrintModifiers(ArgumentSyntax node, PrintingContext context)
    {
        var docs = new ValueListBuilder<Doc>([null, null]);
        if (node.NameColon != null)
        {
            docs.Append(BaseExpressionColon.Print(node.NameColon, context));
        }

        if (node.RefKindKeyword.RawSyntaxKind() != SyntaxKind.None)
        {
            docs.Append(Token.PrintWithSuffix(node.RefKindKeyword, " ", context));
        }

        return Doc.Concat(ref docs);
    }
}
