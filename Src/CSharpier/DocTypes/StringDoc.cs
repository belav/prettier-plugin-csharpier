namespace CSharpier.DocTypes;

internal sealed class StringDoc(string value, bool isDirective = false) : Doc(DocKind.String)
{
    public string Value { get; } = value;
    public bool IsDirective { get; } = isDirective;
}
