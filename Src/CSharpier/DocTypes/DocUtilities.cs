namespace CSharpier.DocTypes;

internal static class DocUtilities
{
    public static bool ContainsBreak(Doc doc)
    {
        if (CheckForBreak(doc))
        {
            return true;
        }

        // we don't deal with ConditionalGroups or the BreakContents of IfBreak
        // they aren't needed for where this is currently used and they will be more expensive
        return doc switch
        {
            IHasContents hasContents => ContainsBreak(hasContents.Contents),
            Concat concat => concat.Any(ContainsBreak),
            IfBreak ifBreak => ContainsBreak(ifBreak.FlatContents),
            _ => false,
        };
    }

    private static bool CheckForBreak(Doc doc)
    {
        return doc switch
        {
            Group group => group.Break,
            HardLine => true,
            LiteralLine => true,
            BreakParent => true,
            _ => false,
        };
    }

    public static void RemoveInitialDoubleHardLine(List<Doc> docs)
    {
        var removeNextHardLine = false;
        RemoveInitialDoubleHardLine(docs, ref removeNextHardLine);
    }

    private static void RemoveInitialDoubleHardLine(IList<Doc> docs, ref bool removeNextHardLine)
    {
        var x = 0;
        while (x < docs.Count)
        {
            var doc = docs[x];

            if (doc is HardLine)
            {
                if (removeNextHardLine)
                {
                    docs[x] = Doc.Null;
                    return;
                }

                removeNextHardLine = true;
            }
            else
            {
                RemoveInitialDoubleHardLine(doc, ref removeNextHardLine);
                return;
            }

            x++;
        }
    }

    private static void RemoveInitialDoubleHardLineConcat(Concat docs, ref bool removeNextHardLine)
    {
        var x = 0;
        while (x < docs.Count)
        {
            var doc = docs[x];

            if (doc is HardLine)
            {
                if (removeNextHardLine)
                {
                    docs[x] = Doc.Null;
                    return;
                }

                removeNextHardLine = true;
            }
            else
            {
                RemoveInitialDoubleHardLine(doc, ref removeNextHardLine);
                return;
            }

            x++;
        }
    }

    public static Doc RemoveInitialDoubleHardLine(Doc doc)
    {
        var removeNextHardLine = false;
        RemoveInitialDoubleHardLine(doc, ref removeNextHardLine);

        return doc;
    }

    private static void RemoveInitialDoubleHardLine(Doc doc, ref bool removeNextHardLine)
    {
        switch (doc)
        {
            case StringDoc:
                return;
            case IndentDoc indentDoc:
                switch (indentDoc.Contents)
                {
                    case HardLine:
                        if (removeNextHardLine)
                        {
                            indentDoc.Contents = Doc.Null;
                            return;
                        }

                        removeNextHardLine = true;
                        return;
                    default:
                        RemoveInitialDoubleHardLine(indentDoc.Contents, ref removeNextHardLine);
                        return;
                }
            case Group group:
                switch (group.Contents)
                {
                    case HardLine:
                        if (removeNextHardLine)
                        {
                            group.Contents = Doc.Null;
                            return;
                        }

                        removeNextHardLine = true;
                        return;
                    default:
                        RemoveInitialDoubleHardLine(group.Contents, ref removeNextHardLine);
                        return;
                }
            case Concat concat:
                RemoveInitialDoubleHardLineConcat(concat, ref removeNextHardLine);
                return;
        }
    }
}
