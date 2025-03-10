namespace CSharpier.DocTypes;

internal sealed class LeadingComment : Doc
{
    internal LeadingComment()
        : base(DocKind.LeadingComment) { }

    public CommentType Type { get; init; }
    public string Comment { get; init; } = string.Empty;
}
