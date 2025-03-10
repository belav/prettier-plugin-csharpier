namespace CSharpier.DocTypes;

internal sealed class NullDoc : Doc
{
    public static NullDoc Instance { get; } = new();

    private NullDoc()
        : base(DocKind.Null) { }
}
