using System.Text;

namespace CSharpier.SyntaxPrinter;

internal static class SeparatedSyntaxList
{
    public static Doc Print<T>(
        SeparatedSyntaxList<T> list,
        Func<T, PrintingContext, Doc> printFunc,
        Doc afterSeparator,
        PrintingContext context,
        int startingIndex = 0
    )
        where T : SyntaxNode
    {
        return Print(list, printFunc, afterSeparator, context, startingIndex, null);
    }

    public static Doc PrintWithTrailingComma<T>(
        SeparatedSyntaxList<T> list,
        Func<T, PrintingContext, Doc> printFunc,
        Doc afterSeparator,
        PrintingContext context,
        SyntaxToken? closingToken = null
    )
        where T : SyntaxNode
    {
        return Print(list, printFunc, afterSeparator, context, 0, closingToken);
    }

    // the names above aren't totally accurate
    // sometimes there are trailing commas with calls to Print (some patterns do that)
    // and if you pass null to PrintWithTrailingComma it won't add a trailing comma if there isn't one
    private static Doc Print<T>(
        in SeparatedSyntaxList<T> list,
        Func<T, PrintingContext, Doc> printFunc,
        Doc afterSeparator,
        PrintingContext context,
        int startingIndex,
        SyntaxToken? closingToken
    )
        where T : SyntaxNode
    {
        var docs =
            list.Count <= 3
                ? new ValueListBuilder<Doc>([null, null, null, null, null, null, null, null])
                : new ValueListBuilder<Doc>(list.Count * 3);
        var unFormattedCode = new StringBuilder();
        var printUnformatted = false;
        for (var x = startingIndex; x < list.Count; x++)
        {
            var member = list[x];

            if (Token.HasLeadingCommentMatching(member, CSharpierIgnore.IgnoreEndRegex))
            {
                docs.Append(unFormattedCode.ToString().Trim());
                unFormattedCode.Clear();
                printUnformatted = false;
            }
            else if (Token.HasLeadingCommentMatching(member, CSharpierIgnore.IgnoreStartRegex))
            {
                if (!printUnformatted && x > 0)
                {
                    docs.Append(Doc.HardLine);
                }
                printUnformatted = true;
            }

            if (printUnformatted)
            {
                unFormattedCode.Append(CSharpierIgnore.PrintWithoutFormatting(member, context));
                if (x < list.SeparatorCount)
                {
                    unFormattedCode.AppendLine(list.GetSeparator(x).Text);
                }

                continue;
            }

            var firstTrailingComment = list[x]
                .GetTrailingTrivia()
                .FirstOrDefault(o => o.IsComment());

            // we want a trailing comma, but we need to get it printed in place before a trailing comment
            // shove it in the context so the token printing can pick it up and put it in place
            if (
                x >= list.SeparatorCount
                && closingToken is not null
                && firstTrailingComment != default
            )
            {
                context.WithTrailingComma(
                    firstTrailingComment,
                    TrailingComma.Print(closingToken.Value, context, true)
                );
            }

            docs.Append(printFunc(list[x], context));

            // if the syntax tree doesn't have a trailing comma but we want want, then add it
            if (x >= list.SeparatorCount)
            {
                if (closingToken != null && firstTrailingComment == default)
                {
                    docs.Append(TrailingComma.Print(closingToken.Value, context));
                }

                continue;
            }

            var isTrailingSeparator = x == list.Count - 1;

            if (isTrailingSeparator)
            {
                var trailingSeparatorToken = list.GetSeparator(x);
                // when the trailing separator has trailing comments, we have to print it normally to prevent it from collapsing
                // when the closing token has a directive, we can't assume the comma should be added/removed so just print it normally
                if (
                    trailingSeparatorToken.TrailingTrivia.Any(o => o.IsComment())
                    || closingToken != null
                        && closingToken.Value.LeadingTrivia.Any(o => o.IsDirective)
                )
                {
                    docs.Append(Token.Print(trailingSeparatorToken, context));
                }
                else if (closingToken != null)
                {
                    docs.Append(TrailingComma.Print(closingToken.Value, context));
                }
                else
                {
                    docs.Append(Doc.IfBreak(Token.Print(list.GetSeparator(x), context), Doc.Null));
                }
            }
            else
            {
                docs.Append(Token.Print(list.GetSeparator(x), context));
                docs.Append(afterSeparator);
            }
        }

        if (unFormattedCode.Length > 0)
        {
            docs.Append(unFormattedCode.ToString().Trim());
        }

        var output = docs.Length switch
        {
            0 => Doc.Null,
            1 => docs[0],
            _ => Doc.Concat(docs.AsSpan().ToArray()),
        };
        docs.Dispose();

        return output;
    }
}
