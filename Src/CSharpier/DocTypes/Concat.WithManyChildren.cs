namespace CSharpier.DocTypes;

internal abstract partial class Concat
{
    internal sealed class WithManyChildren(IList<Doc> content) : Concat
    {
        public override int Count => content.Count;

        public override Doc this[int index]
        {
            get => content[index];
            set => content[index] = value;
        }

        public override void RemoveAt(int index) => content.RemoveAt(index);
    }
}
