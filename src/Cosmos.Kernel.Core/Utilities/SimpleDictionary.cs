namespace Cosmos.Kernel.Core.Utilities;

internal class SimpleDictionary<TKey, TValue> where TKey : notnull
{
    private const int InitialCapacity = 16;
    private Entry[] _buckets;
    private int _count;

    public SimpleDictionary() : this(InitialCapacity) { }
    public SimpleDictionary(int capacity)
    {
        _buckets = new Entry[capacity];
    }


    public int Count => _count;

    public TValue this[TKey key]
    {
        get
        {
            if (TryGetValue(key, out TValue value))
            {
                return value;
            }

            throw new KeyNotFoundException();
        }
        set
        {
            Add(key, value);
        }
    }

    public void Add(TKey key, TValue value)
    {
        int hash = HashCode(key);
        int bucketIndex = hash % _buckets.Length;

        var newEntry = new Entry(key, value);

        if (_buckets[bucketIndex] == null)
        {
            _buckets[bucketIndex] = newEntry;
        }
        else
        {
            Entry? current = _buckets[bucketIndex];
            while (current is not null)
            {
                if (current.Key.Equals(key))
                {
                    throw new ArgumentException("An item with the same key already exists.");
                }

                if (current.Next == null)
                {
                    current.Next = newEntry;
                    break;
                }

                current = current.Next;
            }
        }

        _count++;

        // Resize if needed
        if (_count > _buckets.Length * 0.75)
        {
            Resize();
        }
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        int hash = HashCode(key);
        int bucketIndex = hash % _buckets.Length;

        var current = _buckets[bucketIndex];

        while (current != null)
        {
            if (current.Key.Equals(key))
            {
                value = current.Value;
                return true;
            }

            current = current.Next;
        }

        value = default!;
        return false;
    }

    public bool ContainsKey(TKey key)
    {
        return TryGetValue(key, out _);
    }

    private void Resize()
    {
        var oldBuckets = _buckets;
        _buckets = new Entry[_buckets.Length * 2];
        _count = 0;

        foreach (var entry in oldBuckets)
        {
            var current = entry;
            while (current != null)
            {
                Add(current.Key, current.Value);
                current = current.Next;
            }
        }
    }

    private static int HashCode(TKey key)
    {
        if (key is string strKey)
        {
            unchecked
            {
                int hash = 17;
                foreach (char c in strKey)
                {
                    hash = hash * 31 + c;
                }
                hash &= 0x7FFFFFFF;
                return hash;
            }
        }
        else
        {
            return key.GetHashCode() & 0x7FFFFFFF;
        }
    }

    public bool Remove(TKey key)
    {
        int hash = HashCode(key);
        int bucketIndex = hash % _buckets.Length;

        Entry? current = _buckets[bucketIndex];
        Entry? previous = null;

        while (current != null)
        {
            if (current.Key.Equals(key))
            {
                if (previous == null)
                {
                    _buckets[bucketIndex] = current.Next!;
                }
                else
                {
                    previous.Next = current.Next;
                }

                _count--;
                return true;
            }

            previous = current;
            current = current.Next;
        }

        return false;
    }
    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

    private sealed class Entry(TKey key, TValue value)
    {
        public TKey Key { get; } = key;
        public TValue Value { get; } = value;
        public Entry? Next { get; set; }
    }
}
