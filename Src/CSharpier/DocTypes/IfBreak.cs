namespace CSharpier.DocTypes;

internal class IfBreak : Doc
{
    internal IfBreak()
        : base(DocKind.IfBreak) { }

    public Doc FlatContents { get; set; } = Null;
    public Doc BreakContents { get; set; } = Null;
    public string? GroupId { get; set; }
}

// we don't need a doc kind for this because we only look for this class in serializer
internal sealed class IndentIfBreak : IfBreak
{
    public IndentIfBreak(Doc contents, string groupId)
    {
        this.BreakContents = Indent(contents);
        this.FlatContents = contents;
        this.GroupId = groupId;
    }
}
