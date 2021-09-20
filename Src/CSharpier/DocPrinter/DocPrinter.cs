using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpier.DocTypes;
using CSharpier.Utilities;

namespace CSharpier.DocPrinter
{
    public class DocPrinter
    {
        protected readonly Stack<PrintCommand> RemainingCommands = new();
        protected readonly Dictionary<string, PrintMode> GroupModeMap = new();
        protected int CurrentWidth;
        protected readonly StringBuilder Output = new();
        protected bool ShouldRemeasure;
        protected bool NewLineNextStringValue;
        protected bool SkipNextNewLine;
        protected readonly string EndOfLine;
        protected readonly PrinterOptions PrinterOptions;
        protected readonly Indenter Indenter;

        protected DocPrinter(Doc doc, PrinterOptions printerOptions, string endOfLine)
        {
            EndOfLine = endOfLine;
            PrinterOptions = printerOptions;
            Indenter = new Indenter(printerOptions);
            RemainingCommands.Push(new PrintCommand(Indenter.GenerateRoot(), PrintMode.Break, doc));
        }

        public static string Print(Doc document, PrinterOptions printerOptions, string endOfLine)
        {
            PropagateBreaks.RunOn(document);

            return new DocPrinter(document, printerOptions, endOfLine).Print();
        }

        public string Print()
        {
            while (RemainingCommands.Count > 0)
            {
                ProcessNextCommand();
            }

            if (Output.Length == 0 || Output[^1] != '\n')
            {
                Output.Append(EndOfLine);
            }

            var result = Output.ToString();
            if (PrinterOptions.TrimInitialLines)
            {
                result = result.TrimStart('\n', '\r');
            }

            return result;
        }

        private void ProcessNextCommand()
        {
            var (indent, mode, doc) = RemainingCommands.Pop();
            if (doc == Doc.Null)
            {
                return;
            }

            switch (doc)
            {
                case StringDoc stringDoc:
                    ProcessString(stringDoc, indent);
                    break;
                case Concat concat:
                {
                    for (var x = concat.Contents.Count - 1; x >= 0; x--)
                    {
                        Push(concat.Contents[x], mode, indent);
                    }
                    break;
                }
                case IndentDoc indentDoc:
                    Push(indentDoc.Contents, mode, Indenter.IncreaseIndent(indent));
                    break;
                case Trim:
                    CurrentWidth -= Output.TrimTrailingWhitespace();
                    NewLineNextStringValue = false;
                    break;
                case Group group:
                    ProcessGroup(@group, mode, indent);
                    break;
                case IfBreak ifBreak:
                {
                    var groupMode = mode;
                    if (ifBreak.GroupId != null)
                    {
                        if (!GroupModeMap.TryGetValue(ifBreak.GroupId, out groupMode))
                        {
                            throw new Exception(
                                "You cannot use an ifBreak before the group it targets."
                            );
                        }
                    }

                    var contents =
                        groupMode == PrintMode.Break ? ifBreak.BreakContents : ifBreak.FlatContents;
                    Push(contents, mode, indent);
                    break;
                }
                case LineDoc line:
                    ProcessLine(line, mode, indent);
                    break;
                case BreakParent:
                    break;
                case LeadingComment leadingComment:
                {
                    Output.TrimTrailingWhitespace();
                    if ((Output.Length != 0 && Output[^1] != '\n') || NewLineNextStringValue)
                    {
                        Output.Append(EndOfLine);
                    }

                    Output.Append(indent.Value).Append(leadingComment.Comment);
                    CurrentWidth = indent.Length;
                    NewLineNextStringValue = false;
                    SkipNextNewLine = false;
                    break;
                }
                case TrailingComment trailingComment:
                    Output.TrimTrailingWhitespace();
                    Output.Append(' ').Append(trailingComment.Comment);
                    CurrentWidth = indent.Length;
                    NewLineNextStringValue = true;
                    SkipNextNewLine = true;
                    break;
                case ForceFlat forceFlat:
                    Push(forceFlat.Contents, PrintMode.Flat, indent);
                    break;
                case Align align:
                    Push(align.Contents, mode, Indenter.AddAlign(indent, align.Width));
                    break;
                default:
                    throw new Exception("didn't handle " + doc);
            }
        }

        private void ProcessString(StringDoc stringDoc, Indent indent)
        {
            if (string.IsNullOrEmpty(stringDoc.Value))
            {
                return;
            }

            // this ensures we don't print extra spaces after a trailing comment
            // newLineNextStringValue & skipNextNewLine are set to true when we print a trailing comment
            // when they are set we new line the next string we find. If we new line and then print a " " we end up with an extra space
            if (NewLineNextStringValue && SkipNextNewLine && stringDoc.Value == " ")
            {
                return;
            }

            if (NewLineNextStringValue)
            {
                Output.TrimTrailingWhitespace();
                Output.Append(EndOfLine).Append(indent.Value);
                CurrentWidth = indent.Length;
                NewLineNextStringValue = false;
            }

            Output.Append(stringDoc.Value);
            CurrentWidth += stringDoc.Value.GetPrintedWidth();
        }

        private void ProcessLine(LineDoc line, PrintMode mode, Indent indent)
        {
            if (mode is PrintMode.Flat or PrintMode.ForceFlat)
            {
                if (line.Type == LineDoc.LineType.Soft)
                {
                    return;
                }

                if (line.Type == LineDoc.LineType.Normal)
                {
                    Output.Append(' ');
                    CurrentWidth += 1;
                    return;
                }

                // This line was forced into the output even if we were in flattened mode, so we need to tell the next
                // group that no matter what, it needs to remeasure because the previous measurement didn't accurately
                // capture the entire expression (this is necessary for nested groups)
                ShouldRemeasure = true;
            }

            if (line.Squash && Output.Length > 0 && Output.EndsWithNewLineAndWhitespace())
            {
                return;
            }

            if (line.IsLiteral)
            {
                if (Output.Length > 0)
                {
                    Output.Append(EndOfLine);
                    CurrentWidth = 0;
                }
            }
            else
            {
                if (!SkipNextNewLine || !NewLineNextStringValue)
                {
                    Output.TrimTrailingWhitespace();
                    Output.Append(EndOfLine).Append(indent.Value);
                    CurrentWidth = indent.Length;
                }

                if (SkipNextNewLine)
                {
                    SkipNextNewLine = false;
                }
            }
        }

        private void ProcessGroup(Group group, PrintMode mode, Indent indent)
        {
            if (mode is PrintMode.Flat or PrintMode.ForceFlat && !ShouldRemeasure)
            {
                Push(group.Contents, group.Break ? PrintMode.Break : PrintMode.Flat, indent);
            }
            else
            {
                ShouldRemeasure = false;
                var possibleCommand = new PrintCommand(indent, PrintMode.Flat, group.Contents);

                if (!group.Break && Fits(possibleCommand))
                {
                    RemainingCommands.Push(possibleCommand);
                }
                else if (group is ConditionalGroup conditionalGroup)
                {
                    if (group.Break)
                    {
                        Push(conditionalGroup.Options.Last(), PrintMode.Break, indent);
                    }
                    else
                    {
                        var foundSomethingThatFits = false;
                        foreach (var option in conditionalGroup.Options.Skip(1))
                        {
                            possibleCommand = new PrintCommand(indent, mode, option);
                            if (!Fits(possibleCommand))
                            {
                                continue;
                            }
                            RemainingCommands.Push(possibleCommand);
                            foundSomethingThatFits = true;
                            break;
                        }

                        if (!foundSomethingThatFits)
                        {
                            RemainingCommands.Push(possibleCommand);
                        }
                    }
                }
                else
                {
                    Push(group.Contents, PrintMode.Break, indent);
                }
            }

            if (group.GroupId != null)
            {
                GroupModeMap[group.GroupId] = RemainingCommands.Peek().Mode;
            }
        }

        private bool Fits(PrintCommand possibleCommand)
        {
            return DocFitter.Fits(
                possibleCommand,
                RemainingCommands,
                PrinterOptions.Width - CurrentWidth,
                GroupModeMap,
                Indenter
            );
        }

        private void Push(Doc doc, PrintMode printMode, Indent indent)
        {
            RemainingCommands.Push(new PrintCommand(indent, printMode, doc));
        }
    }

    public record PrintCommand(Indent Indent, PrintMode Mode, Doc Doc);

    public enum PrintMode
    {
        Flat,
        Break,
        ForceFlat
    }
}
