namespace CSharpier.DocPrinter;

internal static class PropagateBreaks
{
    private class MarkerDoc : Doc { }

    private static readonly MarkerDoc TraverseDocOnExitStackMarker = new();

    public static void RunOn(Doc document)
    {
        var alreadyVisitedSet = new HashSet<Group>();
        var groupStack = new Stack<Group>();
        var forceFlat = 0;
        var canSkipBreak = false;

        void BreakParentGroup()
        {
            if (groupStack.Count > 0)
            {
                var parentGroup = groupStack.Peek();
                if (parentGroup is not ConditionalGroup)
                {
                    parentGroup.Break = true;
                }
            }
        }

        bool OnEnter(Doc doc)
        {
            if (doc is ForceFlat)
            {
                forceFlat++;
            }
            if (doc is IBreakParent && (forceFlat == 0 || (forceFlat > 0 && doc is LiteralLine)))
            {
                if (doc is HardLine { SkipBreakIfFirstInGroup: true } && canSkipBreak)
                {
                    if (groupStack.Count > 1)
                    {
                        var nextGroup = groupStack.Pop();
                        groupStack.Peek().Break = true;
                        groupStack.Push(nextGroup);
                    }
                }
                else
                {
                    BreakParentGroup();
                }
            }
            else if (doc is Group group)
            {
                canSkipBreak = true;
                groupStack.Push(group);
                if (!alreadyVisitedSet.Add(group))
                {
                    return false;
                }
            }
            else if (doc is StringDoc { IsDirective: false })
            {
                canSkipBreak = false;
            }

            return true;
        }

        void OnExit(Doc doc)
        {
            if (doc is ForceFlat)
            {
                forceFlat--;
            }
            else if (doc is Group)
            {
                canSkipBreak = false;
                var group = groupStack.Pop();
                if (group.Break)
                {
                    BreakParentGroup();
                }
            }
        }

        var docsStack = new Stack<Doc>();
        docsStack.Push(document);
        while (docsStack.Count > 0)
        {
            var doc = docsStack.Pop();

            if (doc == TraverseDocOnExitStackMarker)
            {
                OnExit(docsStack.Pop());
                continue;
            }

            docsStack.Push(doc);
            docsStack.Push(TraverseDocOnExitStackMarker);

            if (!OnEnter(doc))
            {
                continue;
            }

            if (doc is Concat concat)
            {
                // push onto stack in reverse order so they are processed in the original order
                for (var x = concat.Count - 1; x >= 0; --x)
                {
                    if (forceFlat > 0 && concat[x] is LineDoc { IsLiteral: false } lineDoc)
                    {
                        concat[x] = lineDoc.Type == LineDoc.LineType.Soft ? string.Empty : " ";
                    }

                    docsStack.Push(concat[x]);
                }
            }
            else if (doc is IfBreak ifBreak)
            {
                docsStack.Push(ifBreak.FlatContents);
                docsStack.Push(ifBreak.BreakContents);
            }
            else if (doc is ConditionalGroup conditionalGroup)
            {
                foreach (var option in conditionalGroup.Options)
                {
                    docsStack.Push(option);
                }
            }
            else if (doc is IHasContents hasContents)
            {
                docsStack.Push(hasContents.Contents);
            }
        }
    }
}
