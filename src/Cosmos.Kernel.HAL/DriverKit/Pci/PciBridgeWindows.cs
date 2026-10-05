// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.DriverKit.Pci;

/// <summary>
/// The windows a bridge's type 1 header decodes, with the kit's placement
/// cursor in each: where the base address registers of a function arriving
/// behind a hot-plug slot are placed. Read when the slot's function 0 is
/// described; firmware's assignment of everything present at boot is never
/// changed. Thread context; allocation-free after construction. Built and
/// used on the kit worker or a driver thread only.
/// </summary>
internal sealed class PciBridgeWindows
{
    /// <summary>I/O Base register offset (8 bits).</summary>
    private const ushort IoBaseOffset = 0x1C;
    /// <summary>I/O Limit register offset (8 bits).</summary>
    private const ushort IoLimitOffset = 0x1D;
    /// <summary>Memory Base register offset (16 bits).</summary>
    private const ushort MemoryBaseOffset = 0x20;
    /// <summary>Memory Limit register offset (16 bits).</summary>
    private const ushort MemoryLimitOffset = 0x22;
    /// <summary>Prefetchable Memory Base register offset (16 bits).</summary>
    private const ushort PrefetchableBaseOffset = 0x24;
    /// <summary>Prefetchable Memory Limit register offset (16 bits).</summary>
    private const ushort PrefetchableLimitOffset = 0x26;
    /// <summary>Prefetchable Base Upper 32 Bits register offset.</summary>
    private const ushort PrefetchableBaseUpperOffset = 0x28;
    /// <summary>Prefetchable Limit Upper 32 Bits register offset.</summary>
    private const ushort PrefetchableLimitUpperOffset = 0x2C;
    /// <summary>I/O Base Upper 16 Bits register offset.</summary>
    private const ushort IoBaseUpperOffset = 0x30;
    /// <summary>I/O Limit Upper 16 Bits register offset.</summary>
    private const ushort IoLimitUpperOffset = 0x32;

    /// <summary>Memory Base and Limit bits 15:4: address bits 31:20.</summary>
    private const ushort MemoryAddressMask = 0xFFF0;
    /// <summary>Shift of the memory address bits of a Base or Limit register into place.</summary>
    private const int MemoryAddressShift = 16;
    /// <summary>The low bits a memory limit decodes up to: the window ends on a 1 MiB boundary.</summary>
    private const ulong MemoryLimitLowBits = 0xFFFFF;
    /// <summary>Prefetchable Base and Limit bit 0: the window is 64-bit, its upper half in the Upper 32 Bits registers.</summary>
    private const ushort PrefetchableIs64Bit = 0x1;
    /// <summary>Shift placing the upper half of a 64-bit prefetchable address.</summary>
    private const int UpperHalfShift = 32;
    /// <summary>I/O Base and Limit bits 7:4: address bits 15:12.</summary>
    private const byte IoAddressMask = 0xF0;
    /// <summary>I/O Base and Limit bits 3:0: the addressing capability.</summary>
    private const byte IoCapabilityMask = 0x0F;
    /// <summary>I/O capability of a 32-bit window, its upper halves in the Upper 16 Bits registers.</summary>
    private const byte Io32Bit = 0x1;
    /// <summary>Shift of the I/O address bits of a Base or Limit register into place.</summary>
    private const int IoAddressShift = 8;
    /// <summary>Shift placing the upper half of a 32-bit I/O address.</summary>
    private const int IoUpperShift = 16;
    /// <summary>The low bits an I/O limit decodes up to: the window ends on a 4 KiB boundary.</summary>
    private const ulong IoLimitLowBits = 0xFFF;

    private ulong _memoryNext;
    private ulong _prefetchableNext;
    private ulong _ioNext;

    private PciBridgeWindows(ulong memoryBase, ulong memoryLimit, ulong prefetchableBase, ulong prefetchableLimit, ulong ioBase, ulong ioLimit)
    {
        MemoryBase = memoryBase;
        MemoryLimit = memoryLimit;
        PrefetchableBase = prefetchableBase;
        PrefetchableLimit = prefetchableLimit;
        IoBase = ioBase;
        IoLimit = ioLimit;
        _memoryNext = memoryBase;
        _prefetchableNext = prefetchableBase;
        _ioNext = ioBase;
    }

    /// <summary>The first address of the memory window. Any context; allocation-free.</summary>
    internal ulong MemoryBase { get; }

    /// <summary>The last address of the memory window. Any context; allocation-free.</summary>
    internal ulong MemoryLimit { get; }

    /// <summary>
    /// True when the memory window decodes: its base is at most its limit
    /// and not 0. A base of 0 is what a bridge firmware never programmed
    /// reads (both registers at their reset value of 0 decode as
    /// 0x0-0xFFFFF), never a window on these platforms, where address 0 is
    /// RAM. Any context; allocation-free.
    /// </summary>
    internal bool HasMemory => MemoryBase != 0 && MemoryBase <= MemoryLimit;

    /// <summary>The first address of the prefetchable memory window. Any context; allocation-free.</summary>
    internal ulong PrefetchableBase { get; }

    /// <summary>The last address of the prefetchable memory window. Any context; allocation-free.</summary>
    internal ulong PrefetchableLimit { get; }

    /// <summary>True when the prefetchable window decodes: its base is at most its limit and not 0, as for <see cref="HasMemory"/>. Any context; allocation-free.</summary>
    internal bool HasPrefetchable => PrefetchableBase != 0 && PrefetchableBase <= PrefetchableLimit;

    /// <summary>The first port of the I/O window. Any context; allocation-free.</summary>
    internal ulong IoBase { get; }

    /// <summary>The last port of the I/O window. Any context; allocation-free.</summary>
    internal ulong IoLimit { get; }

    /// <summary>
    /// True when the I/O window decodes: its base is at most its limit and
    /// not 0. An unprogrammed bridge reads ports 0x0-0xFFF, where the
    /// legacy DMA controller sits on x64, so a base of 0 is taken as no
    /// window. Any context; allocation-free.
    /// </summary>
    internal bool HasIo => IoBase != 0 && IoBase <= IoLimit;

    /// <summary>
    /// Decodes the memory, prefetchable and I/O windows of a type 1 header,
    /// with every cursor at its window's base. Thread context.
    /// </summary>
    /// <param name="bridge">The bridge function.</param>
    /// <returns>The windows; a window firmware disabled has its base above its limit, one it never programmed its base at 0.</returns>
    internal static PciBridgeWindows Read(PciAccess bridge)
    {
        ushort memoryBaseRegister = bridge.ReadConfig16(MemoryBaseOffset);
        ushort memoryLimitRegister = bridge.ReadConfig16(MemoryLimitOffset);
        ulong memoryBase = (ulong)(memoryBaseRegister & MemoryAddressMask) << MemoryAddressShift;
        ulong memoryLimit = ((ulong)(memoryLimitRegister & MemoryAddressMask) << MemoryAddressShift) | MemoryLimitLowBits;

        ushort prefetchableBaseRegister = bridge.ReadConfig16(PrefetchableBaseOffset);
        ushort prefetchableLimitRegister = bridge.ReadConfig16(PrefetchableLimitOffset);
        ulong prefetchableBase = (ulong)(prefetchableBaseRegister & MemoryAddressMask) << MemoryAddressShift;
        ulong prefetchableLimit = ((ulong)(prefetchableLimitRegister & MemoryAddressMask) << MemoryAddressShift) | MemoryLimitLowBits;
        if ((prefetchableBaseRegister & PrefetchableIs64Bit) != 0)
        {
            prefetchableBase |= (ulong)bridge.ReadConfig32(PrefetchableBaseUpperOffset) << UpperHalfShift;
        }

        if ((prefetchableLimitRegister & PrefetchableIs64Bit) != 0)
        {
            prefetchableLimit |= (ulong)bridge.ReadConfig32(PrefetchableLimitUpperOffset) << UpperHalfShift;
        }

        byte ioBaseRegister = bridge.ReadConfig8(IoBaseOffset);
        byte ioLimitRegister = bridge.ReadConfig8(IoLimitOffset);
        ulong ioBase = (ulong)(ioBaseRegister & IoAddressMask) << IoAddressShift;
        ulong ioLimit = ((ulong)(ioLimitRegister & IoAddressMask) << IoAddressShift) | IoLimitLowBits;
        if ((ioBaseRegister & IoCapabilityMask) == Io32Bit)
        {
            ioBase |= (ulong)bridge.ReadConfig16(IoBaseUpperOffset) << IoUpperShift;
        }

        if ((ioLimitRegister & IoCapabilityMask) == Io32Bit)
        {
            ioLimit |= (ulong)bridge.ReadConfig16(IoLimitUpperOffset) << IoUpperShift;
        }

        return new PciBridgeWindows(memoryBase, memoryLimit, prefetchableBase, prefetchableLimit, ioBase, ioLimit);
    }

    /// <summary>
    /// Records a register already assigned (by firmware, or by a placement
    /// of this pass once the register took the address): the cursor of the
    /// window holding it moves past its end, so a placement never lands on
    /// it. A register outside every window moves nothing. Thread context;
    /// allocation-free.
    /// </summary>
    /// <param name="bar">An assigned register.</param>
    internal void NoteAssigned(PciBar bar)
    {
        if (!bar.IsAssigned || bar.Length == 0)
        {
            return;
        }

        ulong end = bar.Base + bar.Length;
        if (bar.IsIo)
        {
            if (HasIo && Contains(IoBase, IoLimit, bar.Base) && end > _ioNext)
            {
                _ioNext = end;
            }

            return;
        }

        if (HasPrefetchable && Contains(PrefetchableBase, PrefetchableLimit, bar.Base))
        {
            if (end > _prefetchableNext)
            {
                _prefetchableNext = end;
            }

            return;
        }

        if (HasMemory && Contains(MemoryBase, MemoryLimit, bar.Base) && end > _memoryNext)
        {
            _memoryNext = end;
        }
    }

    /// <summary>
    /// Chooses an address for an unassigned register: in the prefetchable
    /// window for a prefetchable register when the bridge has one and the
    /// register fits there, else in the memory window for any memory
    /// register (the window type is a hint, not a constraint); in the I/O
    /// window for an I/O register when the bridge has one. The address is
    /// the window's cursor aligned up to the register's size. No cursor
    /// moves: once the register took the address, the caller records it
    /// with <see cref="NoteAssigned"/>, so a register that refused it wastes
    /// no space. Thread context; allocation-free.
    /// </summary>
    /// <param name="bar">An unassigned register with its decoded length, a power of two.</param>
    /// <param name="address">The chosen address; 0 when nothing fits.</param>
    /// <returns>
    /// False when the bridge has no window of the kind, when the register
    /// does not fit above the cursor of any window it may take, or when a
    /// 32-bit memory register would end above 4 GiB.
    /// </returns>
    internal bool TryPlace(PciBar bar, out ulong address)
    {
        address = 0;
        if (bar.Length == 0)
        {
            return false;
        }

        if (bar.IsIo)
        {
            return HasIo && TryFit(_ioNext, IoLimit, bar, out address);
        }

        if (bar.IsPrefetchable && HasPrefetchable && TryFit(_prefetchableNext, PrefetchableLimit, bar, out address))
        {
            return true;
        }

        return HasMemory && TryFit(_memoryNext, MemoryLimit, bar, out address);
    }

    /// <summary>The address of <paramref name="bar"/> at <paramref name="cursor"/> aligned up to its size, when it fits below <paramref name="limit"/>.</summary>
    private static bool TryFit(ulong cursor, ulong limit, PciBar bar, out ulong address)
    {
        address = 0;
        ulong alignMask = bar.Length - 1;
        if (cursor > ulong.MaxValue - alignMask)
        {
            return false;
        }

        ulong aligned = (cursor + alignMask) & ~alignMask;
        if (aligned > limit || alignMask > limit - aligned)
        {
            return false;
        }

        if (!bar.Is64Bit && aligned > uint.MaxValue - bar.Length + 1)
        {
            return false;
        }

        address = aligned;
        return true;
    }

    /// <summary>True when <paramref name="address"/> lies within <paramref name="windowBase"/> and <paramref name="windowLimit"/>.</summary>
    private static bool Contains(ulong windowBase, ulong windowLimit, ulong address) => address >= windowBase && address <= windowLimit;
}
