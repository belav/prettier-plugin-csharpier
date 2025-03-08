namespace CSharpier.SyntaxPrinter.SyntaxNodePrinters;

internal static class BaseFieldDeclaration
{
    public static Doc Print(BaseFieldDeclarationSyntax node, PrintingContext context)
    {
        var docs = new ValueListBuilder<Doc>([null, null, null, null, null]);
        docs.Append(AttributeLists.Print(node, node.AttributeLists, context));
        docs.Append(Modifiers.PrintSorted(node.Modifiers, context));
        if (node is EventFieldDeclarationSyntax eventFieldDeclarationSyntax)
        {
            docs.Append(
                Token.PrintWithSuffix(eventFieldDeclarationSyntax.EventKeyword, " ", context)
            );
        }

        docs.Append(
            VariableDeclaration.Print(node.Declaration, context),
            Token.Print(node.SemicolonToken, context)
        );
        return Doc.Concat(ref docs);
    }
}
