using System;

namespace Cosmos.Kernel.Core.Memory;

/// <summary>
/// The safe view over a run of pages <see cref="PageAllocator"/> handed out:
/// how the layers above Core, which compile without unsafe code, read and
/// write the memory behind a <see cref="PageType.Unmanaged"/> allocation.
/// </summary>
/// <remarks>
/// An address and a length, not an array: the pages may sit anywhere in the
/// heap and out of any object graph, so they are reached by span over the raw
/// base rather than indexed. This type owns nothing — freeing the pages is
/// <see cref="PageAllocator.Free(MemoryBlock)"/>, which takes the block back.
/// </remarks>
internal class MemoryBlock
{
    /// <summary>
    /// Memory block base address.
    /// </summary>
    public readonly ulong Base;

    /// <summary>
    /// Memory block size.
    /// </summary>
    public readonly uint Size;

    /// <summary>
    /// The block as bytes, at most <see cref="int.MaxValue"/> of them. A longer
    /// run is reached a slice at a time by the caller. Allocation-free.
    /// </summary>
    public unsafe Span<byte> Span => new((void*)Base, (int)Math.Min(Size, int.MaxValue));

    /// <summary>
    /// Create new instance of the <see cref="MemoryBlock"/> class.
    /// </summary>
    /// <param name="aBase">A base.</param>
    /// <param name="aByteSize">A size.</param>
    public MemoryBlock(ulong aBase, uint aByteSize)
    {
        Base = aBase;
        Size = aByteSize;
    }

    /// <summary>
    /// The block as values of <typeparamref name="T"/>, as many as fit, at
    /// most <see cref="int.MaxValue"/>. Allocation-free.
    /// </summary>
    /// <typeparam name="T">The element type, an unmanaged struct such as a descriptor.</typeparam>
    public unsafe Span<T> AsSpan<T>() where T : unmanaged
    {
        return new Span<T>((void*)Base, (int)Math.Min(Size / (uint)sizeof(T), int.MaxValue));
    }
}