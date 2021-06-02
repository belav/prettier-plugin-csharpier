using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpier.DocTypes;
using CSharpier.Utilities;

namespace CSharpier.DocPrinter
{
    public static class DocFitter
    {
        public static bool Fits(
            PrintCommand nextCommand,
            Stack<PrintCommand> remainingCommands,
            int remainingWidth,
            PrinterOptions printerOptions,
            Dictionary<string, PrintMode> groupModeMap
        ) {
            var returnFalseIfMoreStringsFound = false;
            var newCommands = new Stack<PrintCommand>();
            newCommands.Push(nextCommand);

            void Push(Doc doc, PrintMode printMode, Indent indent)
            {
                newCommands.Push(new PrintCommand(indent, printMode, doc));
            }

            var output = new StringBuilder();

            for (var x = 0; x < remainingCommands.Count || newCommands.Count > 0;)
            {
                if (remainingWidth < 0)
                {
                    return false;
                }

                PrintCommand command;
                if (newCommands.Count > 0)
                {
                    command = newCommands.Pop();
                }
                else
                {
                    command = remainingCommands.ElementAt(x);
                    x++;
                }

                var (currentIndent, currentMode, currentDoc) = command;

                if (currentDoc is StringDoc stringDoc)
                {
                    if (stringDoc.Value == null)
                    {
                        continue;
                    }

                    if (returnFalseIfMoreStringsFound)
                    {
                        return false;
                    }

                    output.Append(stringDoc.Value);
                    remainingWidth -= stringDoc.Value.GetPrintedWidth();
                }
                else if (currentDoc != Doc.Null)
                {
                    switch (currentDoc)
                    {
                        case LeadingComment:
                        case TrailingComment:
                            if (output.Length > 0)
                            {
                                returnFalseIfMoreStringsFound = true;
                            }
                            break;
                        case Concat concat:
                            for (var i = concat.Contents.Count - 1; i >= 0; i--)
                            {
                                Push(concat.Contents[i], currentMode, currentIndent);
                            }
                            break;
                        case IndentDoc indent:
                            Push(
                                indent.Contents,
                                currentMode,
                                IndentBuilder.Make(currentIndent, printerOptions)
                            );
                            break;
                        case Trim:
                            remainingWidth += output.TrimTrailingWhitespace();
                            break;
                        case Group group:
                            var groupMode = group.Break ? PrintMode.Break : currentMode;

                            Push(group.Contents, groupMode, currentIndent);

                            if (group.GroupId != null)
                            {
                                groupModeMap![group.GroupId] = groupMode;
                            }
                            break;
                        case IfBreak ifBreak:
                            var ifBreakMode = ifBreak.GroupId != null
                            && groupModeMap!.ContainsKey(ifBreak.GroupId)
                                ? groupModeMap[ifBreak.GroupId]
                                : currentMode;

                            var contents = ifBreakMode == PrintMode.Break
                                ? ifBreak.BreakContents
                                : ifBreak.FlatContents;

                            Push(contents, currentMode, currentIndent);
                            break;
                        case LineDoc line:
                            switch (currentMode)
                            {
                                case PrintMode.Flat:
                                case PrintMode.Forceflat:
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
                                case PrintMode.Break:
                                    return true;
                            }
                            break;
                        case ForceFlat flat:
                            Push(flat.Contents, currentMode, currentIndent);
                            break;
                        case BreakParent:
                            break;
                        default:
                            throw new Exception("Can't handle " + currentDoc.GetType());
                    }
                }
            }

            return remainingWidth > 0;
        }
    }
}
