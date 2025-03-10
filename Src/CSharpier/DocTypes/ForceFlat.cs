namespace CSharpier.DocTypes;

internal sealed class ForceFlat : Doc, IHasContents
{
    internal ForceFlat()
        : base(DocKind.ForceFlat) { }

    public Doc Contents { get; set; } = Null;
}
