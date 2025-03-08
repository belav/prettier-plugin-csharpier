namespace CSharpier.SyntaxPrinter.SyntaxNodePrinters;

internal static class Parameter
{
    public static Doc Print(ParameterSyntax node, PrintingContext context)
    {
        var hasAttribute = node.AttributeLists.Any();
        var groupId = hasAttribute ? Guid.NewGuid().ToString() : string.Empty;
        var docs = new ValueListBuilder<Doc>([null, null, null, null, null, null, null]);

        if (hasAttribute)
        {
            docs.Append(AttributeLists.Print(node, node.AttributeLists, context));
            docs.Append(Doc.IndentIfBreak(Doc.Line, groupId));
        }

        if (node.Modifiers.Any())
        {
            docs.Append(Modifiers.Print(node.Modifiers, context));
        }

        if (node.Type != null)
        {
            docs.Append(Node.Print(node.Type, context), " ");
        }

        docs.Append(Token.Print(node.Identifier, context));
        if (node.Default != null)
        {
            docs.Append(EqualsValueClause.Print(node.Default, context));
        }

        return hasAttribute
            ? Doc.GroupWithId(groupId, docs.AsSpan().ToArray())
            : Doc.Concat(ref docs);
    }
}
