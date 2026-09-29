// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.DriverKit.Engine;

namespace Cosmos.Kernel.HAL.DriverKit;

/// <summary>
/// A bulk device region mapped through <see cref="DeviceBinding.MapRegion"/>:
/// a framebuffer, a command queue, a descriptor area. Where a
/// <see cref="RegisterWindow"/> checks and orders every access, a region is
/// memory: <see cref="Span"/> fills it, <see cref="As{T}"/> copies structs
/// into it, and the driver orders what needs ordering with
/// <see cref="DmaBuffer.WriteBarrier"/> and <see cref="DmaBuffer.ReadBarrier"/>.
/// <see cref="Slice"/> carves a sub-range that goes dead with its parent.
/// The accessors neither allocate nor block.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public sealed unsafe class DeviceRegion : IKitResource
{
    private readonly ulong _address;

    /// <summary>The region this one was sliced from; null for a region the kit mapped itself.</summary>
    private readonly DeviceRegion? _parent;

    /// <summary>Set by the binding's teardown; every access throws from then on.</summary>
    private volatile bool _invalidated;

    /// <summary>Length of the region in bytes.</summary>
    public ulong Length { get; }

    /// <summary>The caching the region was mapped with.</summary>
    public RegionCaching Caching { get; }

    internal DeviceRegion(ulong address, ulong length, RegionCaching caching)
    {
        _address = address;
        Length = length;
        Caching = caching;
    }

    private DeviceRegion(DeviceRegion parent, ulong address, ulong length)
    {
        _parent = parent;
        _address = address;
        Length = length;
        Caching = parent.Caching;
    }

    /// <summary>
    /// The region as bytes. A span is at most <see cref="int.MaxValue"/>
    /// bytes long; a longer region is reached through <see cref="Pointer"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">The binding that mapped the region was torn down.</exception>
    public Span<byte> Span
    {
        get
        {
            ThrowIfInvalidated();
            return new Span<byte>((void*)_address, (int)Math.Min(Length, int.MaxValue));
        }
    }

    /// <summary>The region's first byte. For accesses a span cannot express.</summary>
    /// <exception cref="InvalidOperationException">The binding that mapped the region was torn down.</exception>
    public byte* Pointer
    {
        get
        {
            ThrowIfInvalidated();
            return (byte*)_address;
        }
    }

    /// <summary>
    /// The region as a span of <typeparamref name="T"/> values, as many as fit.
    /// </summary>
    /// <typeparam name="T">The element type, an unmanaged struct such as a descriptor.</typeparam>
    /// <exception cref="InvalidOperationException">The binding that mapped the region was torn down.</exception>
    public Span<T> As<T>() where T : unmanaged
    {
        ThrowIfInvalidated();
        ulong count = Length / (ulong)sizeof(T);
        return new Span<T>((void*)_address, (int)Math.Min(count, int.MaxValue));
    }

    /// <summary>
    /// A region over <paramref name="length"/> bytes of this one starting at
    /// <paramref name="offset"/>, with the same caching, that shares this
    /// region's invalidation: once this region (or the one it was sliced
    /// from) is torn down, the slice throws too. Thread context. The slice
    /// is not a kit resource of its own: it is not in the binding's ledger
    /// and not counted by its held resources.
    /// </summary>
    /// <param name="offset">Offset of the slice's first byte from this region's first byte.</param>
    /// <param name="length">Length of the slice in bytes.</param>
    /// <exception cref="ArgumentOutOfRangeException">The range does not fit in this region.</exception>
    /// <exception cref="InvalidOperationException">The binding that mapped the region was torn down.</exception>
    public DeviceRegion Slice(ulong offset, ulong length)
    {
        ThrowIfInvalidated();

        // Written as a remainder rather than offset + length > Length, which
        // an offset near ulong.MaxValue would wrap past.
        if (offset > Length)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), offset, "The slice starts past the end of the region.");
        }

        if (Length - offset < length)
        {
            throw new ArgumentOutOfRangeException(nameof(length), length, "The slice runs past the end of the region.");
        }

        return new DeviceRegion(this, _address + offset, length);
    }

    /// <summary>
    /// Makes every later access throw, on this region and on every slice of
    /// it. Called by the binding's teardown, and by the firmware display's
    /// retirement, so a driver or a ring still holding the region gets an
    /// exception instead of writing to a device that is no longer its own.
    /// </summary>
    internal void Invalidate() => _invalidated = true;

    void IKitResource.Release() => Invalidate();

    private void ThrowIfInvalidated()
    {
        DeviceRegion? region = this;
        while (region is not null)
        {
            if (region._invalidated)
            {
                throw new InvalidOperationException("The binding that mapped this region was torn down.");
            }

            region = region._parent;
        }
    }
}
