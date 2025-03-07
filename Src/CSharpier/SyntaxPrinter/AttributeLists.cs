namespace CSharpier.SyntaxPrinter;

internal static class AttributeLists
{
    public static Doc Print(
        SyntaxNode node,
        SyntaxList<AttributeListSyntax> attributeLists,
        PrintingContext context
    )
    {
        if (attributeLists.Count == 0)
        {
            return Doc.Null;
        }

        var docs = new ValueListBuilder<Doc>([null, null]);
        Doc separator = node
            is TypeParameterSyntax
                or ParameterSyntax
                or ParenthesizedLambdaExpressionSyntax
            ? Doc.Line
            : Doc.HardLine;

        docs.Append(
            Doc.Join(separator, attributeLists.Select(o => AttributeList.Print(o, context)))
        );

        if (node is not (ParameterSyntax or TypeParameterSyntax))
        {
            docs.Append(separator);
        }

        return Doc.Concat(ref docs);
    }
}
