namespace CSharpier.SyntaxPrinter.SyntaxNodePrinters;

internal static class VariableDeclarator
{
    public static Doc Print(VariableDeclaratorSyntax node, PrintingContext context)
    {
        var docs = new ValueListBuilder<Doc>([null, null, null]);
        docs.Append(Token.Print(node.Identifier, context));

        if (node.ArgumentList != null)
        {
            docs.Append(BracketedArgumentList.Print(node.ArgumentList, context));
        }
        if (node.Initializer != null)
        {
            docs.Append(EqualsValueClause.Print(node.Initializer, context));
        }
        return Doc.Concat(ref docs);
    }
}
