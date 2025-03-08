namespace CSharpier.SyntaxPrinter.SyntaxNodePrinters;

internal static class SwitchSection
{
    public static Doc Print(SwitchSectionSyntax node, PrintingContext context)
    {
        var docs = new ValueListBuilder<Doc>([null, null]);
        docs.Append(Doc.Join(Doc.HardLine, node.Labels.Select(o => Node.Print(o, context))));
        if (node.Statements is [BlockSyntax blockSyntax])
        {
            docs.Append(Block.Print(blockSyntax, context));
        }
        else
        {
            docs.Append(
                Doc.Indent(
                    node.Statements.First() is BlockSyntax ? Doc.Null : Doc.HardLine,
                    Doc.Join(
                        Doc.HardLine,
                        node.Statements.Select(o => Node.Print(o, context)).ToArray()
                    )
                )
            );
        }
        return Doc.Concat(ref docs);
    }
}
