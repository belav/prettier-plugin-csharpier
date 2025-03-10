namespace CSharpier.DocTypes;

internal class Group : Doc, IHasContents
{
    internal Group()
        : base(DocKind.Group) { }

    public Doc Contents { get; set; } = Null;
    public bool Break { get; set; }
    public string? GroupId { get; set; }
}
