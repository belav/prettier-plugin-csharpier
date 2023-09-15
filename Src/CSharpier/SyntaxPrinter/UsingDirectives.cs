namespace CSharpier.SyntaxPrinter;

internal static class UsingDirectives
{
    private static readonly DefaultOrder Comparer = new();

    /* TODO added edgecases for the rest of the mono stuff, just don't sort if there is extern or define or undef?
TODO review PRs, the full one is too big

     */

    public static Doc PrintWithSorting(
        SyntaxList<UsingDirectiveSyntax> usings,
        FormattingContext context,
        bool printExtraLines
    )
    {
        if (usings.Count == 0)
        {
            return Doc.Null;
        }

        // TODO sorting we can maybe use this to ignore disabled text, but that doesn't quite work
        // context.ReorderedModifiers = usings.Any(o => o.GetLeadingTrivia().Any(p => p.IsDirective));

        var docs = new List<Doc>();
        var usingList = usings.ToList();

        var initialComments = new List<SyntaxTrivia>();
        var triviaWithinIf = new List<SyntaxTrivia>();
        var foundIfDirective = false;
        var keepUsingsUntilEndIf = false;
        foreach (var leadingTrivia in usings[0].GetLeadingTrivia())
        {
            if (
                leadingTrivia.RawSyntaxKind() == SyntaxKind.DisabledTextTrivia
                && leadingTrivia.ToFullString().TrimStart().StartsWith("extern alias")
            )
            {
                initialComments = usings[0].GetLeadingTrivia().ToList();
                triviaWithinIf.Clear();
                keepUsingsUntilEndIf = true;
                break;
            }
            if (
                leadingTrivia.RawSyntaxKind() == SyntaxKind.DefineDirectiveTrivia
                || leadingTrivia.RawSyntaxKind() == SyntaxKind.UndefDirectiveTrivia
            )
            {
                initialComments = usings.First().GetLeadingTrivia().ToList();
                triviaWithinIf.Clear();
                break;
            }
            if (leadingTrivia.RawSyntaxKind() == SyntaxKind.IfDirectiveTrivia)
            {
                foundIfDirective = true;
            }

            if (foundIfDirective)
            {
                triviaWithinIf.Add(leadingTrivia);
            }
            else
            {
                initialComments.Add(leadingTrivia);
            }
        }

        docs.Add(Token.PrintLeadingTrivia(new SyntaxTriviaList(initialComments), context));
        if (keepUsingsUntilEndIf)
        {
            while (usingList.Any())
            {
                var firstUsing = usingList.First();

                usingList.RemoveAt(0);
                if (firstUsing != usings[0])
                {
                    docs.Add(Token.PrintLeadingTrivia(firstUsing.GetLeadingTrivia(), context));
                }
                docs.Add(UsingDirective.Print(firstUsing, context));

                if (
                    firstUsing
                        .GetLeadingTrivia()
                        .Any(o => o.RawSyntaxKind() == SyntaxKind.EndIfDirectiveTrivia)
                )
                {
                    break;
                }
            }
        }

        var isFirst = true;
        foreach (
            var groupOfUsingData in GroupUsings(
                usingList,
                new SyntaxTriviaList(triviaWithinIf),
                context
            )
        )
        {
            foreach (var usingData in groupOfUsingData)
            {
                if (!isFirst)
                {
                    docs.Add(Doc.HardLine);
                }

                if (usingData.LeadingTrivia != Doc.Null)
                {
                    docs.Add(usingData.LeadingTrivia);
                }
                if (usingData.Using is not null)
                {
                    docs.Add(UsingDirective.Print(usingData.Using, context, printExtraLines));
                }

                isFirst = false;
            }
        }

        return Doc.Concat(docs);
    }

    private static IEnumerable<List<UsingData>> GroupUsings(
        List<UsingDirectiveSyntax> usings,
        SyntaxTriviaList triviaOnFirstUsing,
        FormattingContext context
    )
    {
        var globalUsings = new List<UsingData>();
        var systemUsings = new List<UsingData>();
        var aliasNameUsings = new List<UsingData>();
        var regularUsings = new List<UsingData>();
        var staticUsings = new List<UsingData>();
        var aliasUsings = new List<UsingData>();
        var directiveGroup = new List<UsingData>();
        var ifCount = 0;
        var isFirst = true;

        foreach (var usingDirective in usings)
        {
            var openIf = ifCount > 0;
            foreach (var directive in usingDirective.GetLeadingTrivia().Where(o => o.IsDirective))
            {
                if (directive.RawSyntaxKind() is SyntaxKind.IfDirectiveTrivia)
                {
                    ifCount++;
                }
                else if (directive.RawSyntaxKind() is SyntaxKind.EndIfDirectiveTrivia)
                {
                    ifCount--;
                }
            }

            if (ifCount > 0)
            {
                directiveGroup.Add(
                    new UsingData
                    {
                        Using = usingDirective,
                        LeadingTrivia = PrintStuff(usingDirective)
                    }
                );
            }
            else
            {
                if (openIf)
                {
                    directiveGroup.Add(
                        new UsingData { LeadingTrivia = PrintStuff(usingDirective) }
                    );
                }

                var usingData = new UsingData
                {
                    Using = usingDirective,
                    LeadingTrivia = !openIf ? PrintStuff(usingDirective) : Doc.Null
                };

                if (usingDirective.GlobalKeyword.RawSyntaxKind() != SyntaxKind.None)
                {
                    globalUsings.Add(usingData);
                }
                else if (usingDirective.StaticKeyword.RawSyntaxKind() != SyntaxKind.None)
                {
                    staticUsings.Add(usingData);
                }
                else if (usingDirective.Alias is not null)
                {
                    aliasUsings.Add(usingData);
                }
                else if (usingDirective.Name is AliasQualifiedNameSyntax)
                {
                    aliasNameUsings.Add(usingData);
                }
                else if (usingDirective.Name is not null && IsSystemName(usingDirective.Name))
                {
                    systemUsings.Add(usingData);
                }
                else
                {
                    regularUsings.Add(usingData);
                }
            }
        }

        yield return globalUsings.OrderBy(o => o.Using!, Comparer).ToList();
        yield return systemUsings.OrderBy(o => o.Using!, Comparer).ToList();
        yield return aliasNameUsings.OrderBy(o => o.Using!, Comparer).ToList();
        yield return regularUsings.OrderBy(o => o.Using!, Comparer).ToList();
        yield return directiveGroup;
        yield return staticUsings.OrderBy(o => o.Using!, Comparer).ToList();
        yield return aliasUsings.OrderBy(o => o.Using!, Comparer).ToList();
        yield break;

        Doc PrintStuff(UsingDirectiveSyntax value)
        {
            var result = isFirst
                ? Token.PrintLeadingTrivia(triviaOnFirstUsing, context)
                : Token.PrintLeadingTrivia(value.GetLeadingTrivia(), context);

            isFirst = false;
            return result;
        }
    }

    private class UsingData
    {
        public Doc LeadingTrivia { get; init; } = Doc.Null;
        public UsingDirectiveSyntax? Using { get; init; }
    }

    private static bool IsSystemName(NameSyntax value)
    {
        while (true)
        {
            if (value is not QualifiedNameSyntax qualifiedNameSyntax)
            {
                return value is IdentifierNameSyntax { Identifier.Text: "System" };
            }
            value = qualifiedNameSyntax.Left;
        }
    }

    private class DefaultOrder : IComparer<UsingDirectiveSyntax>
    {
        public int Compare(UsingDirectiveSyntax? x, UsingDirectiveSyntax? y)
        {
            if (x?.Name is null)
            {
                return -1;
            }

            if (y?.Name is null)
            {
                return 1;
            }

            if (x.Alias is not null && y.Alias is not null)
            {
                return x.Alias.ToFullString().CompareTo(y.Alias.ToFullString());
            }

            return x.Name.ToFullString().CompareTo(y.Name.ToFullString());
        }
    }
}
