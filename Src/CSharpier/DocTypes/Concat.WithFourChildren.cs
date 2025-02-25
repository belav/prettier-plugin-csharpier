namespace CSharpier.DocTypes;

internal abstract partial class Concat
{
    internal sealed class WithFourChildren(Doc child0, Doc child1, Doc child2, Doc child3) : Concat
    {
        private Doc _child0 = child0;
        private Doc _child1 = child1;
        private Doc _child2 = child2;
        private Doc _child3 = child3;

        public override int Count => 4;

        public override Doc this[int index]
        {
            get =>
                index switch
                {
                    0 => _child0,
                    1 => _child1,
                    2 => _child2,
                    3 => _child3,
                    _ => throw new IndexOutOfRangeException(nameof(index)),
                };
            set
            {
                switch (index)
                {
                    case 0:
                        _child0 = value;
                        break;
                    case 1:
                        _child1 = value;
                        break;
                    case 2:
                        _child2 = value;
                        break;
                    case 3:
                        _child3 = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException(nameof(index));
                }
            }
        }

        public override void RemoveAt(int index)
        {
            switch (index)
            {
                case 0:
                    _child0 = _child1;
                    _child1 = _child2;
                    _child2 = _child3;
                    _child3 = Doc.Null;
                    break;
                case 1:
                    _child1 = _child2;
                    _child2 = _child3;
                    _child3 = Doc.Null;
                    break;
                case 2:
                    _child2 = _child3;
                    _child3 = Doc.Null;
                    break;
                case 3:
                    _child3 = Doc.Null;
                    break;
                default:
                    throw new IndexOutOfRangeException(nameof(index));
            }
        }
    }
}
