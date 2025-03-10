using System.Text;

namespace CSharpier.DocPrinter;

internal static class DocFitter
{
    public static bool Fits(
        PrintCommand nextCommand,
        Stack<PrintCommand> remainingCommands,
        int remainingWidth,
        Dictionary<string, PrintMode> groupModeMap,
        Indenter indenter,
        Stack<PrintCommand> newCommands,
        StringBuilder output
    )
    {
        // Reset reusable collections before usage
        newCommands.Clear();
        output.Clear();

        var returnFalseIfMoreStringsFound = false;
        newCommands.Push(nextCommand);

        void Push(Doc doc, PrintMode printMode, Indent indent)
        {
            newCommands.Push(new PrintCommand(indent, printMode, doc));
        }

        for (var x = 0; x < remainingCommands.Count || newCommands.Count > 0; )
        {
            if (remainingWidth < 0)
            {
                return false;
            }

            var (currentIndent, currentMode, currentDoc) = newCommands switch
            {
                { Count: > 0 } => newCommands.Pop(),
                _ => remainingCommands.ElementAt(x++),
            };

            switch (currentDoc.Kind)
            {
                case DocKind.Null:
                    break;
                case DocKind.String:
                    var stringDoc = (StringDoc)currentDoc;
                    // directives should not be considered when calculating if something fits
                    if (stringDoc.Value == null || stringDoc.IsDirective)
                        continue;

                    if (returnFalseIfMoreStringsFound)
                        return false;

                    output.Append(stringDoc.Value);
                    remainingWidth -= stringDoc.Value.GetPrintedWidth();
                    break;
                case DocKind.LeadingComment
                or DocKind.TrailingComment:
                    if (output.Length > 0 && currentMode is not PrintMode.ForceFlat)
                        returnFalseIfMoreStringsFound = true;

                    break;
                case DocKind.Region:
                    return false;
                case DocKind.Concat:
                    var concat = (Concat)currentDoc;
                    for (var i = concat.Contents.Count - 1; i >= 0; i--)
                        Push(concat.Contents[i], currentMode, currentIndent);
                    break;
                case DocKind.Indent:
                    var indent = (IndentDoc)currentDoc;
                    Push(indent.Contents, currentMode, indenter.IncreaseIndent(currentIndent));
                    break;
                case DocKind.Trim:
                    remainingWidth += output.TrimTrailingWhitespace();
                    break;
                case DocKind.Group:
                {
                    var group = (Group)currentDoc;
                    var groupMode = group.Break ? PrintMode.Break : currentMode;

                    // when determining if something fits, use the last option from a conditionalGroup, which should be the most expanded one
                    var groupContents =
                        groupMode == PrintMode.Break && group is ConditionalGroup conditionalGroup
                            ? conditionalGroup.Options.Last()
                            : group.Contents;
                    Push(groupContents, groupMode, currentIndent);

                    if (group.GroupId != null)
                    {
                        groupModeMap![group.GroupId] = groupMode;
                    }

                    break;
                }
                case DocKind.IfBreak:
                {
                    var ifBreak = (IfBreak)currentDoc;
                    var ifBreakMode =
                        ifBreak.GroupId != null
                        && groupModeMap.TryGetValue(ifBreak.GroupId, out var groupMode)
                            ? groupMode
                            : currentMode;

                    var contents =
                        ifBreakMode == PrintMode.Break
                            ? ifBreak.BreakContents
                            : ifBreak.FlatContents;

                    Push(contents, currentMode, currentIndent);
                    break;
                }
                case DocKind.Line:
                    var line = (LineDoc)currentDoc;
                    if (currentMode is PrintMode.Flat or PrintMode.ForceFlat)
                    {
                        if (currentDoc is HardLine { SkipBreakIfFirstInGroup: true })
                        {
                            returnFalseIfMoreStringsFound = false;
                        }
                        else if (line.Type == LineDoc.LineType.Hard)
                        {
                            return true;
                        }

                        if (line.Type != LineDoc.LineType.Soft)
                        {
                            output.Append(' ');

                            remainingWidth -= 1;
                        }

                        break;
                    }

                    return true;
                case DocKind.ForceFlat:
                    var flat = (ForceFlat)currentDoc;
                    Push(flat.Contents, PrintMode.ForceFlat, currentIndent);
                    break;
                case DocKind.BreakParent:
                    break;
                case DocKind.AlwaysFits:
                    break;
                default:
                    throw new Exception("Can't handle " + currentDoc.GetType());
            }
        }

        return remainingWidth > 0;
    }
}
