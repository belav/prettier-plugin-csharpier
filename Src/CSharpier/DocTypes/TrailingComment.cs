namespace CSharpier.DocTypes;

internal sealed class TrailingComment : Doc
{
    internal TrailingComment()
        : base(DocKind.TrailingComment) { }

    public CommentType Type { get; set; }
    public string Comment { get; set; } = string.Empty;
}
