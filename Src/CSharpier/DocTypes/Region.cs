namespace CSharpier.DocTypes;

internal sealed class Region(string text) : Doc(DocKind.Region)
{
    public string Text { get; } = text;
    public bool IsEnd { get; init; }
}
