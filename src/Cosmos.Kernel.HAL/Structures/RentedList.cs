// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Buffers;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.Structures;

/// <summary>
///     Represents a collection of items that uses a pooled array for memory management,
///     reducing allocations.
/// </summary>
/// <typeparam name="T">The type of elements in the collection.</typeparam>
public struct RentedList<T> : IList<T>, IDisposable, IReadOnlyList<T>
{
    private const int GrowSize = 16;
    private T[] _data;

    private int _count;
    private int _sizeIndex;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RentedList{T}" /> struct.
    /// </summary>
    public RentedList()
    {
        _data = ArrayPool<T>.Shared.Rent(10);
        _sizeIndex = 1;
        _count = 0;
    }

    /// <summary>
    ///     Returns a struct enumerator to avoid boxing during foreach.
    /// </summary>
    public readonly Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    /// <inheritdoc />
    readonly IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return new Enumerator(this);
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }


    /// <summary>
    /// </summary>
    /// <param name="item"></param>
    public void Add(T item)
    {
        if (_count < (_data.Length - 1))
        {
        }
        else
        {
            int trueSizeIndex = _data.Length / GrowSize;
            if (trueSizeIndex > _sizeIndex + 1)
            {
                _sizeIndex = trueSizeIndex;
            }

            _sizeIndex++;
            T[] newData = ArrayPool<T>.Shared.Rent(GrowSize * _sizeIndex);
            Array.Copy(_data, newData, _count);
            ArrayPool<T>.Shared.Return(_data);
            _data = newData;
        }

        _data[_count++] = item;
    }

    /// <summary>
    ///     "Removes" all items from the <see cref="RentedList{T}" />.
    ///     The underlying rented array is retained but its contents are considered invalid after calling this method.
    /// </summary>
    public void Clear()
    {
        _count = 0;
    }

    /// <summary>
    ///     Determines whether the <see cref="RentedList{T}" /> contains a specific value.
    /// </summary>
    /// <param name="item">The object to locate in the <see cref="RentedList{T}" />.</param>
    /// <returns>
    ///     <c>true</c> if <paramref name="item" /> is found in the <see cref="RentedList{T}" />; otherwise, <c>false</c>.
    /// </returns>
    public readonly bool Contains(T item)
    {
        return IndexOf(item) >= 0;
    }

    /// <summary>
    /// </summary>
    /// <param name="array"></param>
    /// <param name="arrayIndex"></param>
    public readonly void CopyTo(T[] array, int arrayIndex)
    {
        _data.AsSpan(0, _count).CopyTo(array.AsSpan(arrayIndex, _count));
    }

    /// <summary>
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Remove(T item)
    {
        int index = IndexOf(item);
        if (index >= 0)
        {
            RemoveAt(index);
        }

        return index >= 0;
    }

    /// <summary>
    ///     Gets the number of elements contained in the <see cref="RentedList{T}" />.
    /// </summary>
    /// <value>
    ///     The total number of elements currently present in the collection.
    /// </value>
    /// <remarks>
    ///     The value represents the count of items added to the collection. It does not reflect
    ///     the capacity of the underlying pooled array.
    /// </remarks>
    public readonly int Count
    {
        get => _count;
    }


    /// <inheritdoc />
    public readonly bool IsReadOnly
    {
        get => false;
    }

    /// <summary>
    ///     Searches for the specified item in the <see cref="RentedList{T}" /> and returns the zero-based index of the first
    ///     occurrence.
    /// </summary>
    /// <param name="item">The item to locate in the <see cref="RentedList{T}" />.</param>
    /// <returns>
    ///     The zero-based index of the first occurrence of the item within the <see cref="RentedList{T}" />,
    ///     if found; otherwise, -1.
    /// </returns>
    public readonly int IndexOf(T item)
    {
        if (item is null)
        {
            for (int i = 0; i < _count; i++)
            {
                if (_data[i] is null)
                {
                    return i;
                }
            }

            return -1;
        }

        for (int i = 0; i < _count; i++)
        {
            if (item.Equals(_data[i]))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    ///     Inserts an item into the <see cref="RentedList{T}" /> at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index at which the item should be inserted.</param>
    /// <param name="item">The object to insert into the list.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown when <paramref name="index" /> is less than 0 or greater than the current count of items in the list.
    /// </exception>
    public void Insert(int index, T item)
    {
        if (index < 0 || index >= _count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        _count++;
        Array.Copy(_data, index, _data, index + 1, _count - index - 1);
        _data[index] = item;
    }

    /// <summary>
    ///     Removes the element at the specified index of the <see cref="RentedList{T}" />.
    /// </summary>
    /// <param name="index">The zero-based index of the element to remove.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown when <paramref name="index" /> is less than 0 or greater than or equal to the count of elements in the list.
    /// </exception>
    public void RemoveAt(int index)
    {
        if (index > _count || index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        _count--;
        Array.Copy(_data, index + 1, _data, index, _count - index);
    }

    /// <summary>
    ///     Provides indexed access to the elements of the <see cref="RentedList{T}" />.
    /// </summary>
    /// <param name="index">The zero-based index of the item to get or set.</param>
    /// <returns>The element at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the index is outside the bounds of the list.</exception>
    public readonly T this[int index]
    {
        get
        {
            if (_count == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (index <= _count)
            {
                return _data[index];
            }

            throw new ArgumentOutOfRangeException(nameof(index));
        }

        set
        {
            if (index < _count)
            {
                _data[index] = value;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }
    }

    /// <summary>
    ///     Releases the resources used by the <see cref="RentedList{T}" /> instance and returns the pooled array to the array
    ///     pool.
    /// </summary>
    public readonly void Dispose()
    {
        _data.AsSpan(0, _count).Clear();
        ArrayPool<T>.Shared.Return(_data);
    }

    /// <summary>
    ///     Enumerator for <see cref="RentedList{T}" />.
    /// </summary>
    public struct Enumerator : IEnumerator<T>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Enumerator" /> struct.
        /// </summary>
        /// <param name="rentedList">The list to enumerate.</param>
        public Enumerator(RentedList<T> rentedList)
        {
            _list = rentedList;
        }

        private int _index = 0;
        private readonly RentedList<T> _list;

        /// <inheritdoc />
        public bool MoveNext()
        {
            if (_index < _list._count)
            {
                _index++;
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public readonly T Current => _list._data[_index - 1];

        /// <inheritdoc />
        public void Reset()
        {
            _index = 0;
        }

        readonly T IEnumerator<T>.Current => _list._data[_index - 1];

        readonly object? IEnumerator.Current => _list._data[_index - 1];

        /// <inheritdoc />
        public readonly void Dispose()
        {
        }
    }
}
