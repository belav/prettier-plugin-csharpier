using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

// ReSharper disable InconsistentNaming

namespace CSharpier.Utilities;

// From https://github.com/dotnet/runtime/blob/f5c73447ca9dcb3407d0143829bbf708c04170c1/src/libraries/System.Private.CoreLib/src/System/Collections/Generic/ValueListBuilder.cs#L10
internal ref partial struct ValueListBuilder<T>
{
    private Span<T> _span;
    private T[]? _arrayFromPool;
    private int _pos;

    public ValueListBuilder(Span<T?> scratchBuffer)
    {
        _span = scratchBuffer!;
    }

    public ValueListBuilder(int capacity)
    {
        Grow(capacity);
    }

    public int Length
    {
        get => _pos;
        set
        {
            Debug.Assert(value >= 0);
            Debug.Assert(value <= _span.Length);
            _pos = value;
        }
    }

    public ref T this[int index]
    {
        get
        {
            Debug.Assert(index < _pos);
            return ref _span[index];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(T item)
    {
        int pos = _pos;

        // Workaround for https://github.com/dotnet/runtime/issues/72004
        Span<T> span = _span;
        if ((uint)pos < (uint)span.Length)
        {
            span[pos] = item;
            _pos = pos + 1;
        }
        else
        {
            AddWithResize(item);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(params ReadOnlySpan<T> source)
    {
        int pos = _pos;
        Span<T> span = _span;
        if (source.Length == 1 && (uint)pos < (uint)span.Length)
        {
            span[pos] = source[0];
            _pos = pos + 1;
        }
        else
        {
            AppendMultiChar(source);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AppendMultiChar(scoped ReadOnlySpan<T> source)
    {
        if ((uint)(_pos + source.Length) > (uint)_span.Length)
        {
            Grow(_span.Length - _pos + source.Length);
        }

        source.CopyTo(_span.Slice(_pos));
        _pos += source.Length;
    }

    public void Insert(int index, T item)
    {
        int pos = _pos;
        if ((uint)pos >= (uint)_span.Length)
        {
            Grow(1);
        }

        var sp = _span;

        _span.Slice(index, _pos - index).CopyTo(_span.Slice(index + 1));
        _span[index] = item;
        _pos += 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AppendSpan(int length)
    {
        Debug.Assert(length >= 0);

        int pos = _pos;
        Span<T> span = _span;
        if ((ulong)(uint)pos + (ulong)(uint)length <= (ulong)(uint)span.Length) // same guard condition as in Span<T>.Slice on 64-bit
        {
            _pos = pos + length;
            return span.Slice(pos, length);
        }
        else
        {
            return AppendSpanWithGrow(length);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private Span<T> AppendSpanWithGrow(int length)
    {
        int pos = _pos;
        Grow(_span.Length - pos + length);
        _pos += length;
        return _span.Slice(pos, length);
    }

    // Hide uncommon path
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AddWithResize(T item)
    {
        Debug.Assert(_pos == _span.Length);
        int pos = _pos;
        Grow(1);
        _span[pos] = item;
        _pos = pos + 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Pop()
    {
        _pos--;
        return _span[_pos];
    }

    public ReadOnlySpan<T> AsSpan()
    {
        return _span.Slice(0, _pos);
    }

    public List<T> ToList()
    {
        var list = new List<T>(_pos);
#if NETSTANDARD2_0
        foreach (var item in _span[.._pos])
        {
            list.Add(item);
        }
#else
        list.AddRange(_span[.._pos]);
#endif

        return list;
    }

    public bool TryCopyTo(Span<T> destination, out int itemsWritten)
    {
        if (_span.Slice(0, _pos).TryCopyTo(destination))
        {
            itemsWritten = _pos;
            return true;
        }

        itemsWritten = 0;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        T[]? toReturn = _arrayFromPool;
        if (toReturn != null)
        {
            _arrayFromPool = null;
            ArrayPool<T>.Shared.Return(toReturn);
        }
    }

    // Note that consuming implementations depend on the list only growing if it's absolutely
    // required.  If the list is already large enough to hold the additional items be added,
    // it must not grow. The list is used in a number of places where the reference is checked
    // and it's expected to match the initial reference provided to the constructor if that
    // span was sufficiently large.
    private void Grow(int additionalCapacityRequired = 1)
    {
        const int ArrayMaxLength = 0x7FFFFFC7; // same as Array.MaxLength

        // Double the size of the span.  If it's currently empty, default to size 4,
        // although it'll be increased in Rent to the pool's minimum bucket size.
        int nextCapacity = Math.Max(
            _span.Length != 0 ? _span.Length * 2 : 4,
            _span.Length + additionalCapacityRequired
        );

        // If the computed doubled capacity exceeds the possible length of an array, then we
        // want to downgrade to either the maximum array length if that's large enough to hold
        // an additional item, or the current length + 1 if it's larger than the max length, in
        // which case it'll result in an OOM when calling Rent below.  In the exceedingly rare
        // case where _span.Length is already int.MaxValue (in which case it couldn't be a managed
        // array), just use that same value again and let it OOM in Rent as well.
        if ((uint)nextCapacity > ArrayMaxLength)
        {
            nextCapacity = Math.Max(Math.Max(_span.Length + 1, ArrayMaxLength), _span.Length);
        }

        T[] array = ArrayPool<T>.Shared.Rent(nextCapacity);
        _span.CopyTo(array);

        T[]? toReturn = _arrayFromPool;
        _span = _arrayFromPool = array;
        if (toReturn != null)
        {
            ArrayPool<T>.Shared.Return(toReturn);
        }
    }
}
