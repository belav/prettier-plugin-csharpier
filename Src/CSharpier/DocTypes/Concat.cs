namespace CSharpier.DocTypes;

internal abstract partial class Concat : Doc
{
    public abstract int Count { get; }

    public abstract Doc this[int index] { get; set; }

    public abstract void RemoveAt(int index);

    public bool Any(Func<Doc, bool> predicate)
    {
        for (var i = 0; i < Count; i++)
        {
            if (predicate(this[i]))
            {
                return true;
            }
        }

        return false;
    }

    public static Concat Create(IList<Doc> collection) => new WithManyChildren(collection);

    public static Doc Create(ReadOnlySpan<Doc> collection)
    {
        return collection.Length switch
        {
            0 => Doc.Null,
            1 => collection[0],
            2 => new WithTwoChildren(collection[0], collection[1]),
            3 => new WithThreeChildren(collection[0], collection[1], collection[2]),
            4 => new WithFourChildren(collection[0], collection[1], collection[2], collection[3]),
            _ => new WithManyChildren(collection.ToArray()),
        };
    }
}
