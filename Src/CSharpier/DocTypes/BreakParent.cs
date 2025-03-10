namespace CSharpier.DocTypes;

internal sealed class BreakParent : Doc, IBreakParent
{
    internal BreakParent()
        : base(DocKind.BreakParent) { }
}

internal interface IBreakParent { }
