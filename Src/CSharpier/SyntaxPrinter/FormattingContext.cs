namespace CSharpier.SyntaxPrinter;

// TODO rename this to PrintingContext
internal class FormattingContext
{
    // TODO these go into Options
    // context.Options.LineEnding
    public required string LineEnding { get; init; }
    public required int IndentSize { get; init; }
    public required bool UseTabs { get; init; }

    // TODO the rest of these go into State
    // context.State.PrintingDepth
    public int PrintingDepth { get; set; }
    public bool NextTriviaNeedsLine { get; set; }
    public bool ShouldSkipNextLeadingTrivia { get; set; }

    // we need to keep track if we reordered modifiers because when modifiers are moved inside
    // of an #if, then we can't compare the before and after disabled text in the source file
    public bool ReorderedModifiers { get; set; }

    // we also need to keep track if we move around usings with disabledText
    public bool ReorderedUsingsWithDisabledText { get; set; }
}
