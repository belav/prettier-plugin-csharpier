using System.Text;

namespace CSharpier.DocPrinter;

using System.Runtime.InteropServices.ComTypes;

internal class DocPrinter
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
    protected Stack<Indent> RegionIndents = new();

    protected DocPrinter(Doc doc, PrinterOptions printerOptions, string endOfLine)
    {
        this.EndOfLine = endOfLine;
        this.PrinterOptions = printerOptions;
        this.Indenter = new Indenter(printerOptions);
        this.RemainingCommands.Push(
            new PrintCommand(Indenter.GenerateRoot(), PrintMode.Break, doc)
        );
    }

    public static string Print(Doc document, PrinterOptions printerOptions, string endOfLine)
    {
        PropagateBreaks.RunOn(document);

        return new DocPrinter(document, printerOptions, endOfLine).Print();
    }

    public string Print()
    {
        while (this.RemainingCommands.Count > 0)
        {
            this.ProcessNextCommand();
        }

        this.EnsureOutputEndsWithSingleNewLine();

        var result = this.Output.ToString();
        if (this.PrinterOptions.TrimInitialLines)
        {
            result = result.TrimStart('\n', '\r');
        }

        return result;
    }

    private void EnsureOutputEndsWithSingleNewLine()
    {
        var trimmed = 0;
        for (; trimmed < this.Output.Length; trimmed++)
        {
            if (this.Output[^(trimmed + 1)] != '\r' && this.Output[^(trimmed + 1)] != '\n')
            {
                break;
            }
        }

        this.Output.Length -= trimmed;

        this.Output.Append(this.EndOfLine);
    }

    private void ProcessNextCommand()
    {
        var (indent, mode, doc) = this.RemainingCommands.Pop();
        if (doc == Doc.Null)
        {
            return;
        }

        if (doc is StringDoc stringDoc)
        {
            this.ProcessString(stringDoc, indent);
        }
        else if (doc is Concat concat)
        {
            for (var x = concat.Contents.Count - 1; x >= 0; x--)
            {
                this.Push(concat.Contents[x], mode, indent);
            }
        }
        else if (doc is IndentDoc indentDoc)
        {
            this.Push(indentDoc.Contents, mode, this.Indenter.IncreaseIndent(indent));
        }
        else if (doc is Trim)
        {
            this.CurrentWidth -= this.Output.TrimTrailingWhitespace();
            this.NewLineNextStringValue = false;
        }
        else if (doc is Group group)
        {
            this.ProcessGroup(group, mode, indent);
        }
        else if (doc is IfBreak ifBreak)
        {
            var groupMode = mode;
            if (ifBreak.GroupId != null)
            {
                if (!this.GroupModeMap.TryGetValue(ifBreak.GroupId, out groupMode))
                {
                    throw new Exception("You cannot use an ifBreak before the group it targets.");
                }
            }

            var contents =
                groupMode == PrintMode.Break ? ifBreak.BreakContents : ifBreak.FlatContents;
            this.Push(contents, mode, indent);
        }
        else if (doc is LineDoc line)
        {
            this.ProcessLine(line, mode, indent);
        }
        else if (doc is BreakParent) { }
        else if (doc is LeadingComment leadingComment)
        {
            this.Output.TrimTrailingWhitespace();
            if ((this.Output.Length != 0 && this.Output[^1] != '\n') || this.NewLineNextStringValue)
            {
                this.Output.Append(this.EndOfLine);
            }

            this.AppendComment(leadingComment, indent);

            this.CurrentWidth = indent.Length;
            this.NewLineNextStringValue = false;
            this.SkipNextNewLine = false;
        }
        else if (doc is TrailingComment trailingComment)
        {
            this.Output.TrimTrailingWhitespace();
            this.Output.Append(' ').Append(trailingComment.Comment);
            this.CurrentWidth = indent.Length;
            if (mode != PrintMode.ForceFlat)
            {
                this.NewLineNextStringValue = true;
                this.SkipNextNewLine = true;
            }
        }
        else if (doc is ForceFlat forceFlat)
        {
            this.Push(forceFlat.Contents, PrintMode.ForceFlat, indent);
        }
        else if (doc is Align align)
        {
            this.Push(align.Contents, mode, this.Indenter.AddAlign(indent, align.Width));
        }
        else if (doc is Region region)
        {
            if (region.IsEnd)
            {
                var regionIndent = this.RegionIndents.Pop();
                this.Output.Append(regionIndent.Value);
            }
            else
            {
                this.Output.Append(indent.Value);
                this.RegionIndents.Push(indent);
            }

            this.Output.Append(region.Text);
        }
        else if (doc is AlwaysFits temp)
        {
            this.Push(temp.Contents, mode, indent);
        }
        else
        {
            throw new Exception("didn't handle " + doc);
        }
    }

    private void AppendComment(LeadingComment leadingComment, Indent indent)
    {
        int CalculateIndentLength(string line) =>
            line.CalculateCurrentLeadingIndentation(this.PrinterOptions.TabWidth);

        var stringReader = new StringReader(leadingComment.Comment);
        var line = stringReader.ReadLine();
        var firstLineIndentLength = 0;
        if (leadingComment.Type == CommentType.MultiLine && line != null)
        {
            // in order to maintain the formatting inside of a multiline comment
            // we calculate how much indentation the first line has so we can trim the same
            // amount off each line
            firstLineIndentLength = CalculateIndentLength(line);
        }

        while (line != null)
        {
            if (leadingComment.Type == CommentType.SingleLine)
            {
                this.Output.Append(indent.Value).Append(line.Trim());
            }
            else if (!string.IsNullOrWhiteSpace(line))
            {
                var thingsToRemove = firstLineIndentLength;
                var index = 0;
                while (thingsToRemove > 0 && index < line.Length)
                {
                    if (line[index] == '\t')
                    {
                        thingsToRemove -= 4;
                    }
                    else
                    {
                        thingsToRemove -= 1;
                    }

                    index++;
                }

                var possibleThingsToRemove = CalculateIndentLength(line);
                if (possibleThingsToRemove < index)
                {
                    index = possibleThingsToRemove;
                }

                this.Output.Append(indent.Value).Append(line[index..]);
            }

            line = stringReader.ReadLine();
            if (line == null)
            {
                return;
            }

            this.Output.Append(this.EndOfLine);
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
        if (this.NewLineNextStringValue && this.SkipNextNewLine && stringDoc.Value == " ")
        {
            return;
        }

        if (this.NewLineNextStringValue)
        {
            this.Output.TrimTrailingWhitespace();
            this.Output.Append(this.EndOfLine).Append(indent.Value);
            this.CurrentWidth = indent.Length;
            this.NewLineNextStringValue = false;
        }

        this.Output.Append(stringDoc.Value);
        this.CurrentWidth += stringDoc.Value.GetPrintedWidth();
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
                this.Output.Append(' ');
                this.CurrentWidth += 1;
                return;
            }

            // This line was forced into the output even if we were in flattened mode, so we need to tell the next
            // group that no matter what, it needs to remeasure because the previous measurement didn't accurately
            // capture the entire expression (this is necessary for nested groups)
            this.ShouldRemeasure = true;
        }

        if (line.Squash && this.Output.Length > 0 && this.Output.EndsWithNewLineAndWhitespace())
        {
            return;
        }

        if (line.IsLiteral)
        {
            if (this.Output.Length > 0)
            {
                this.Output.Append(this.EndOfLine);
                this.CurrentWidth = 0;
            }
        }
        else
        {
            if (!this.SkipNextNewLine || !this.NewLineNextStringValue)
            {
                if (line is not HardLineNoTrim)
                {
                    this.Output.TrimTrailingWhitespace();
                }

                this.Output.Append(this.EndOfLine).Append(indent.Value);
                this.CurrentWidth = indent.Length;
            }

            if (this.SkipNextNewLine)
            {
                this.SkipNextNewLine = false;
            }
        }
    }

    private void ProcessGroup(Group group, PrintMode mode, Indent indent)
    {
        if (mode is PrintMode.Flat or PrintMode.ForceFlat && !this.ShouldRemeasure)
        {
            this.Push(group.Contents, group.Break ? PrintMode.Break : mode, indent);
        }
        else
        {
            this.ShouldRemeasure = false;
            var possibleCommand = new PrintCommand(indent, PrintMode.Flat, group.Contents);

            if (!group.Break && this.Fits(possibleCommand))
            {
                this.RemainingCommands.Push(possibleCommand);
            }
            else if (group is ConditionalGroup conditionalGroup)
            {
                if (group.Break)
                {
                    this.Push(conditionalGroup.Options.Last(), PrintMode.Break, indent);
                }
                else
                {
                    var foundSomethingThatFits = false;
                    foreach (var option in conditionalGroup.Options.Skip(1))
                    {
                        possibleCommand = new PrintCommand(indent, mode, option);
                        if (!this.Fits(possibleCommand))
                        {
                            continue;
                        }

                        this.RemainingCommands.Push(possibleCommand);
                        foundSomethingThatFits = true;
                        break;
                    }

                    if (!foundSomethingThatFits)
                    {
                        this.RemainingCommands.Push(possibleCommand);
                    }
                }
            }
            else
            {
                this.Push(group.Contents, PrintMode.Break, indent);
            }
        }

        if (group.GroupId != null)
        {
            this.GroupModeMap[group.GroupId] = this.RemainingCommands.Peek().Mode;
        }
    }

    private bool Fits(PrintCommand possibleCommand)
    {
        return DocFitter.Fits(
            possibleCommand,
            this.RemainingCommands,
            this.PrinterOptions.Width - this.CurrentWidth,
            this.GroupModeMap,
            this.Indenter
        );
    }

    private void Push(Doc doc, PrintMode printMode, Indent indent)
    {
        this.RemainingCommands.Push(new PrintCommand(indent, printMode, doc));
    }
}

internal record PrintCommand(Indent Indent, PrintMode Mode, Doc Doc);

internal enum PrintMode
{
    Flat,
    Break,
    ForceFlat
}
