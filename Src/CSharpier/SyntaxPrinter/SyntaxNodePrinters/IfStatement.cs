namespace CSharpier.SyntaxPrinter.SyntaxNodePrinters;

internal static class IfStatement
{
    public static Doc Print(IfStatementSyntax node, PrintingContext context)
    {
        var docs = new ValueListBuilder<Doc>([null, null, null, null, null, null, null]);
        if (node.Parent is not ElseClauseSyntax)
        {
            docs.Append(ExtraNewLines.Print(node));
        }

        docs.Append(
            Token.Print(node.IfKeyword, context),
            " ",
            Doc.Group(
                Token.Print(node.OpenParenToken, context),
                Doc.Indent(
                    Doc.IfBreak(Doc.SoftLine, Doc.Null),
                    Node.Print(node.Condition, context)
                ),
                Doc.IfBreak(Doc.SoftLine, Doc.Null)
            ),
            Token.Print(node.CloseParenToken, context),
            OptionalBraces.Print(node.Statement, context)
        );

        if (node.Else != null)
        {
            docs.Append(Doc.HardLine, Node.Print(node.Else, context));
        }

        return Doc.Concat(ref docs);
    }
}
