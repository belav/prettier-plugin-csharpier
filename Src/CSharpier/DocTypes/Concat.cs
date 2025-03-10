namespace CSharpier.DocTypes;

internal sealed class Concat(IList<Doc> contents) : Doc(DocKind.Concat)
{
    public IList<Doc> Contents { get; set; } = contents;
}
