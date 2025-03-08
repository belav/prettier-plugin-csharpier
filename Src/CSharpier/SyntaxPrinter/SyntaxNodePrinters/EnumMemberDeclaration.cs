namespace CSharpier.SyntaxPrinter.SyntaxNodePrinters;

internal static class EnumMemberDeclaration
{
    public static Doc Print(EnumMemberDeclarationSyntax node, PrintingContext context)
    {
        var docs = new ValueListBuilder<Doc>([null, null, null, null]);
        docs.Append(AttributeLists.Print(node, node.AttributeLists, context));
        docs.Append(Modifiers.Print(node.Modifiers, context));
        docs.Append(Token.Print(node.Identifier, context));

        if (node.EqualsValue != null)
        {
            docs.Append(EqualsValueClause.Print(node.EqualsValue, context));
        }
        return Doc.Concat(ref docs);
    }
}
