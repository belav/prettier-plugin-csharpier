namespace CSharpier.DocTypes;

internal sealed class IndentDoc : Doc, IHasContents
{
    internal IndentDoc()
        : base(DocKind.Indent) { }

    public Doc Contents { get; set; } = Null;
}
