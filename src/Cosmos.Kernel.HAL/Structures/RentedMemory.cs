// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Buffers;

namespace Cosmos.Kernel.HAL.Structures;

/// <summary>
/// </summary>
/// <typeparam name="T"></typeparam>
public struct RentedMemory<T> : IDisposable
{
    private T[] _data;

    internal RentedMemory(T[] data, int index, int size)
    {
        _data = data;
        Memory = new Memory<T>(_data, index, size);
    }

    internal RentedMemory(int size)
    {
        _data = ArrayPool<T>.Shared.Rent(size);
        Memory = new Memory<T>(_data, 0, size);
    }

    /// <summary>
    /// </summary>
    public Memory<T> Memory { get; internal set; }

    /// <summary>
    /// </summary>
    public void Dispose()
    {
        ArrayPool<T>.Shared.Return(_data);
        _data = null;
    }
}
