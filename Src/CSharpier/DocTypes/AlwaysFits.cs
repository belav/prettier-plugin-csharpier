namespace CSharpier.DocTypes;

internal sealed class AlwaysFits(Doc printedTrivia) : Doc(DocKind.AlwaysFits)
{
    public readonly Doc Contents = printedTrivia;
}
