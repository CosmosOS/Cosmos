// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.Structures;

/// <summary>
///     Represents a chunk of a buffer
/// </summary>
/// <typeparam name="T">The type of elements stored in the buffer chunk.</typeparam>
public struct BufferChunk<T>
{
    private T[] _buffer;
    private int _size;

    /// <summary>
    ///     get the data
    /// </summary>
    /// <returns></returns>
    public readonly Memory<T> Data()
    {
        return _buffer.AsMemory(0, _size);
    }

    /// <summary>
    ///     resize the data
    /// </summary>
    /// <param name="size"></param>
    public void Resize(int size)
    {
        if (_buffer.Length < size)
        {
            T[] newBuffer = ArrayPool<T>.Shared.Rent(size);
            _buffer.AsSpan(0, _size).CopyTo(newBuffer);
            ArrayPool<T>.Shared.Return(_buffer);
            _buffer = newBuffer;
        }

        _size = size;
    }

    internal BufferChunk(int size)
    {
        _buffer = ArrayPool<T>.Shared.Rent(size);
        _size = size;
    }

    internal readonly void Free()
    {
        ArrayPool<T>.Shared.Return(_buffer);
    }

    /// <inheritdoc />
    public override readonly bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is BufferChunk<T> buff)
        {
            if (buff._size == _size)
            {
                return _buffer.Equals(buff);
            }
        }

        return false;
    }

    /// <inheritdoc />
    public override readonly int GetHashCode()
    {
        return _buffer.GetHashCode();
    }
}

/// <summary>
///     Provides a builder for managing and constructing sequences of <see cref="BufferChunk{T}" /> objects.
/// </summary>
/// <typeparam name="T">The type of elements stored within the buffer chunks managed by the builder.</typeparam>
public struct BufferBuilder<T> : IDisposable
{
    private RentedList<BufferChunk<T>> _chunks;

    /// <summary>
    ///     Provides a builder to manage and construct sequences of buffer chunks represented by <see cref="BufferChunk{T}" />.
    ///     This structure facilitates efficient memory management and construction of buffer-based data.
    /// </summary>
    /// <typeparam name="T">The type of elements contained within the buffer chunks managed by the builder.</typeparam>
    public BufferBuilder()
    {
        _chunks = new RentedList<BufferChunk<T>>();
    }

    /// <summary>
    ///     Get a new chunk as the end
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    public BufferChunk<T> NewChunk(int size)
    {
        BufferChunk<T> chunk = new(size);
        _chunks.Add(chunk);
        return chunk;
    }

    /// <summary>
    ///     Creates and inserts a new buffer chunk into the <see cref="BufferBuilder{T}" /> at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index at which the new chunk should be inserted.</param>
    /// <param name="size">The size of the new buffer chunk.</param>
    /// <returns>A new <see cref="BufferChunk{T}" /> instance with the specified size.</returns>
    public BufferChunk<T> NewChunk(int index, int size)
    {
        BufferChunk<T> chunk = new(size);
        _chunks.Insert(index, chunk);
        return chunk;
    }

    /// <summary>
    ///     Removes the buffer chunk at the specified index from the underlying collection.
    /// </summary>
    /// <param name="index">The zero-based index of the buffer chunk to remove.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown if <paramref name="index" /> is less than 0 or greater than or equal to the number of chunks.
    /// </exception>
    public void RemoveIndex(int index)
    {
        _chunks.RemoveAt(index);
    }

    /// <summary>
    ///     Combines all buffer chunks managed by the <see cref="BufferBuilder{T}" /> into a single
    ///     contiguous block of memory and returns it as a <see cref="RentedMemory{T}" /> instance.
    /// </summary>
    /// <returns>A <see cref="RentedMemory{T}" /> containing the combined data from all buffer chunks.</returns>
    public readonly RentedMemory<T> Build()
    {
        int totalSize = 0;
        for (int i = 0; i < _chunks.Count; i++)
        {
            totalSize += _chunks[i].Data().Length;
        }

        RentedMemory<T> data = new(totalSize);
        int offset = 0;
        for (int i = 0; i < _chunks.Count; i++)
        {
            int size = _chunks[i].Data().Length;
            _chunks[i].Data().CopyTo(data.Memory.Slice(offset, size));
            offset += size;
        }

        return data;
    }

    /// <summary>
    ///     Frees all buffer chunks and disposes the <see cref="BufferBuilder{T}" />.
    /// </summary>
    public readonly void Dispose()
    {
        for (int i = 0; i < _chunks.Count; i++)
        {
            _chunks[i].Free();
        }

        _chunks.Dispose();
    }
}
