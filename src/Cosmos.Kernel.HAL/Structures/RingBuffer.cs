// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Collections;

namespace Cosmos.Kernel.HAL.Structures;

/// <summary>
/// A fixed capacity circular buffer of <typeparamref name="T"/> values.
/// </summary>
/// <typeparam name="T">The type of the buffered values.</typeparam>
public class RingBuffer<T> : IEnumerable<T>
{
    private readonly T[] _buffer;
    private int _head;
    private int _tail;

    /// <summary>
    /// Creates a ring buffer able to hold up to <paramref name="capacity"/> values.
    /// </summary>
    /// <param name="capacity">The maximum number of values the buffer can hold.</param>
    public RingBuffer(ulong capacity)
    {
        _buffer = new T[capacity];
    }

    /// <summary>
    /// Appends an item to the buffer, overwriting the oldest item when the buffer is full.
    /// </summary>
    /// <param name="item">The item to append.</param>
    public void Push(T item)
    {
        _buffer[_tail] = item;
        _tail = (_tail + 1) % _buffer.Length;
        if (Count < _buffer.Length)
        {
            Count++;
        }
    }

    /// <summary>
    /// Removes the oldest item from the buffer.
    /// </summary>
    /// <param name="item">The removed item, or the default value when the buffer is empty.</param>
    /// <returns><see langword="true"/> if an item; otherwise <see langword="false"/>.</returns>
    public bool Pop(out T? item)
    {
        if (Count == 0)
        {
            item = default;
            return false;
        }

        item = _buffer[_head];
        _head = (_head + 1) % _buffer.Length;
        Count--;
        return true;
    }

    /// <summary>
    /// Read the oldest item from the buffer.
    /// </summary>
    /// <param name="item">The item, or the default value when the buffer is empty.</param>
    /// <returns><see langword="true"/> if an item; otherwise <see langword="false"/>.</returns>
    public bool Peek(out T? item)
    {
        if (Count == 0)
        {
            item = default;
            return false;
        }

        item = _buffer[_head];
        return true;
    }

    /// <summary>
    /// Discards every buffered item.
    /// </summary>
    public void Clear()
    {
        Count = 0;
        _head = 0;
        _tail = 0;
    }

    /// <summary>
    /// Gets the number of items currently buffered.
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// Drains the buffer, yielding each buffered item and removing it as it is returned.
    /// </summary>
    /// <returns>An enumerator over the buffered items, leaving the buffer empty.</returns>
    public IEnumerator<T> GetEnumerator()
    {
        while(Pop(out T? item))
        {
            yield return item!;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

}
