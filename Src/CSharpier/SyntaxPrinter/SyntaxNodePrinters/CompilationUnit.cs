namespace CSharpier.SyntaxPrinter.SyntaxNodePrinters;

internal static class CompilationUnit
{
    public static Doc Print(CompilationUnitSyntax node, PrintingContext context)
    {
        var docs = new List<Doc>();

        NamespaceLikePrinter.Print(node, docs, context);

        // this is what ends up adding multiple HardLineSkipBreakIfFirstInGroup, but it is needed for some cases
        // and trying to change the logic in there to not print multiple lines was taking me down a rabbit hole
        var finalTrivia = Token.PrintLeadingTriviaWithNewLines(
            node.EndOfFileToken.LeadingTrivia,
            context
        );
        if (finalTrivia != Doc.Null)
        {
            // really ugly code to prevent a comment at the end of a file from continually inserting new blank lines
            if (
                finalTrivia is Concat { Count: > 1 } list
                && docs.Count > 0
                && docs[^1] is Concat { Count: > 1 } previousList
                && previousList[^1] is HardLine
                && previousList[^2] is HardLine
            )
            {
                while (list[0] is HardLine { SkipBreakIfFirstInGroup: true })
                {
                    list.RemoveAt(0);
                }

                docs.Add(finalTrivia);
            }
            else
            {
                // even though we include the initialNewLines above, a literalLine from directives trims the hardline, so add an extra one here
                docs.Add(Doc.HardLineIfNoPreviousLine, finalTrivia);
            }
        }
        docs.Add(Doc.HardLineIfNoPreviousLine);

        return Doc.Concat(docs);
    }
}
