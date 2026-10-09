namespace DevKernel.Diagnostics.Charting;

// Fixed-capacity ring of samples: once full, adding overwrites the oldest one.
// Indexing goes from the oldest sample (0) to the newest (Count - 1), which is the
// order a chart draws them in.
internal sealed class SampleRing<T>
{
    private readonly T[] _items;
    private int _next;

    public int Capacity => _items.Length;

    public int Count { get; private set; }

    public T this[int index] => _items[Count < Capacity ? index : (_next + index) % Capacity];

    public T Newest => _items[(_next + Capacity - 1) % Capacity];

    public SampleRing(int capacity)
    {
        _items = new T[capacity];
    }

    public void Add(T item)
    {
        _items[_next] = item;

        _next++;
        if (_next >= Capacity)
        {
            _next = 0;
        }

        if (Count < Capacity)
        {
            Count++;
        }
    }

    public void Clear()
    {
        for (int i = 0; i < Capacity; i++)
        {
            _items[i] = default!;
        }

        _next = 0;
        Count = 0;
    }
}
