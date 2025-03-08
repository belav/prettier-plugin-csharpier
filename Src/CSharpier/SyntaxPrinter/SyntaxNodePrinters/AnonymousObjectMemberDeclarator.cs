namespace CSharpier.SyntaxPrinter.SyntaxNodePrinters;

internal static class AnonymousObjectMemberDeclarator
{
    public static Doc Print(AnonymousObjectMemberDeclaratorSyntax node, PrintingContext context)
    {
        var docs = new ValueListBuilder<Doc>([null, null, null, null]);
        if (
            node.Parent is AnonymousObjectCreationExpressionSyntax parent
            && node != parent.Initializers.First()
        )
        {
            docs.Append(ExtraNewLines.Print(node));
        }

        if (node.NameEquals != null)
        {
            docs.Append(Token.PrintWithSuffix(node.NameEquals.Name.Identifier, " ", context));
            docs.Append(Token.PrintWithSuffix(node.NameEquals.EqualsToken, " ", context));
        }
        docs.Append(Node.Print(node.Expression, context));
        return Doc.Concat(ref docs);
    }
}
