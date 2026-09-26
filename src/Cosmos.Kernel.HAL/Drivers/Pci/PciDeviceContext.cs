// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Build.API.Enum;
using Cosmos.Kernel.Boot.Limine;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.Memory;
using Cosmos.Kernel.HAL.Drivers.Engine;
using Cosmos.Kernel.HAL.Interfaces;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.HAL.Pci;
using Cosmos.Kernel.HAL.Pci.Enums;

namespace Cosmos.Kernel.HAL.Drivers.Pci;

/// <summary>
/// A PCI driver's handle on the function it was offered. Before Probe runs,
/// the kit has sized every BAR (with decoding off, so nothing else saw the
/// sizing writes), saved the Command register, turned the function's INTx
/// line off, since no registered driver gets a line interrupt and a
/// function left asserting one must not fire into a vector someone else
/// owns, and turned bus mastering off, since firmware may have left it on
/// for a controller it drove. BARs, DMA memory, bus mastering and
/// interrupts are then handed out during Probe only. When the attempt is
/// declined or fails, the kit disarms the interrupts, drops the
/// publications and the work items and cancels the events, restores the
/// Command register with bus mastering off, frees the DMA memory, turns
/// MSI-X off and gives its vector back, and invalidates the regions, in
/// that order.
/// </summary>
internal sealed class PciDeviceContext : DeviceContext
{
    /// <summary>The one MSI-X entry this version programs: the handler's vector 0.</summary>
    private const int MsiXEntry = 0;

    /// <summary>Number of Base Address Registers in a Type-0 header.</summary>
    private const int BarCount = 6;

    /// <summary>Bytes between two BAR slots in configuration space.</summary>
    private const int BarSlotSize = 4;

    /// <summary>Value written to a BAR to read back which address bits it decodes.</summary>
    private const uint BarSizingPattern = 0xFFFF_FFFF;

    /// <summary>Mask selecting the port bits of an I/O BAR (the low two bits are flags).</summary>
    private const uint IoBarAddressMask = 0xFFFF_FFFC;

    /// <summary>Size of the x86 port space: an I/O BAR must end inside it.</summary>
    private const ulong PortSpaceLength = 0x1_0000;

    /// <summary>
    /// Granule of <see cref="IPlatformInitializer.EnsureMmioMapped"/>: each
    /// call maps the 2 MiB block holding the address, so a larger BAR takes
    /// one call per block.
    /// </summary>
    private const ulong MmioBlockSize = 2 * 1024 * 1024;

    /// <summary>
    /// First config offset a driver may write. Below it sits the header the
    /// kit manages: the Command register, which teardown writes back, and
    /// the BARs it sized and mapped.
    /// </summary>
    private const ushort FirstDriverConfigOffset = 0x40;

    private readonly PciDevice _device;

    /// <summary>
    /// What teardown writes back to the Command register: the value the
    /// attempt found, with an endpoint's bus mastering off.
    /// </summary>
    private readonly PciCommand _restoredCommand;

    // What BAR sizing found for each slot, cached so a driver's TryMapBar
    // never sizes again. A zero length is a slot with no BAR: unimplemented,
    // malformed, or the upper half of the 64-bit BAR before it.
    private readonly ulong[] _barBases;
    private readonly ulong[] _barLengths;
    private readonly bool[] _barIsIo;

    // The attempt's resources, which teardown walks.
    private readonly MmioRegion?[] _mmioRegions = new MmioRegion?[BarCount];
    private readonly PortRegion?[] _portRegions = new PortRegion?[BarCount];
    private readonly List<DmaBuffer> _dmaBuffers = new();

    // The interrupts TryRequestInterrupts granted: the trampoline in front
    // of the driver's handler, and exactly one of the MSI-X table it was
    // bound to or the timer polling it. All null until the request.
    private InterruptTrampoline? _interrupts;
    private MsiXContext? _msiX;
    private SoftwareTimer? _pollTimer;

    /// <summary>Set by the first TryRequestInterrupts, whatever it answered: the request is made once per attempt.</summary>
    private bool _interruptsRequested;

    /// <summary>The function on offer: its IDs and its configuration space.</summary>
    public PciFunction Function { get; }

    /// <summary>
    /// True where PCI I/O BARs are reachable. PCI I/O space is the x86 port
    /// space; ARM64 has none, and its port accessors map port numbers onto a
    /// fixed MMIO base unrelated to any host bridge's I/O window.
    /// </summary>
    private static bool HasPortSpace => PlatformHAL.Architecture == PlatformArchitecture.X64;

    /// <summary>Offset of the kernel's direct map, which turns a BAR's physical address into a CPU address.</summary>
    private static unsafe ulong HhdmOffset => Limine.HHDM.Response != null ? Limine.HHDM.Response->Offset : 0;

    private PciDeviceContext(string driverName, string path, PciDevice device, PciCommand restoredCommand,
        ulong[] barBases, ulong[] barLengths, bool[] barIsIo)
        : base(driverName, path)
    {
        _device = device;
        _restoredCommand = restoredCommand;
        _barBases = barBases;
        _barLengths = barLengths;
        _barIsIo = barIsIo;
        Function = new PciFunction(device);
    }

    /// <summary>
    /// Prepares <paramref name="device"/> for a binding attempt: turns an
    /// endpoint's bus mastering off, sizes each BAR with memory and I/O
    /// decoding off, puts decoding back as it was, then sets
    /// InterruptDisable.
    /// </summary>
    /// <param name="driverName">The candidate registration's name.</param>
    /// <param name="path">The function's path, <c>pci/0000:bb:dd.f</c>.</param>
    /// <param name="device">The function on offer, which nothing owns.</param>
    internal static PciDeviceContext Create(string driverName, string path, PciDevice device)
    {
        ulong[] barBases = new ulong[BarCount];
        ulong[] barLengths = new ulong[BarCount];
        bool[] barIsIo = new bool[BarCount];

        // Bus mastering off until the driver turns it on, and again once
        // the attempt is over: firmware may have left it on for a controller
        // it drove, and a function still holding that DMA state must not
        // write into memory the kernel now uses before Probe has reset it,
        // nor into a failed attempt's DMA buffers once they are freed. An
        // endpoint's only: a bridge's bit forwards the DMA of every function
        // behind it, some of which may be bound already.
        PciCommand restoredCommand = device.Command;
        if (device.HeaderType == PciHeaderType.Normal)
        {
            restoredCommand &= ~PciCommand.Master;
        }

        // Decoding off while a BAR holds all ones: the function must not
        // answer at whatever address that pattern names.
        device.Command = restoredCommand & ~(PciCommand.Io | PciCommand.Memory);
        SizeBars(device, barBases, barLengths, barIsIo);

        // Decoding back as found, since sizing was the only reason it went
        // off. INTx off for the rest of the attempt, and for good if it binds.
        device.Command = restoredCommand | PciCommand.InterruptDisable;

        return new PciDeviceContext(driverName, path, device, restoredCommand, barBases, barLengths, barIsIo);
    }

    /// <summary>
    /// Maps memory BAR <paramref name="index"/> and turns on memory decoding.
    /// Asking again for the same BAR returns the same region. Probe only.
    /// </summary>
    /// <param name="index">The BAR slot, 0 to 5. For a 64-bit BAR, the lower of its two slots.</param>
    /// <param name="region">The mapped BAR, as long as the BAR, when the call returns true.</param>
    /// <returns>
    /// False when the slot holds no memory BAR (an I/O BAR, an unimplemented
    /// slot, the upper half of a 64-bit BAR, or one firmware left unassigned)
    /// or when the platform cannot map it.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not 0 to 5.</exception>
    /// <exception cref="InvalidOperationException">Called outside the driver's Probe.</exception>
    public bool TryMapBar(int index, [NotNullWhen(true)] out MmioRegion? region)
    {
        ThrowIfNotProbing(nameof(TryMapBar));
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, BarCount);

        region = _mmioRegions[index];
        if (region is not null)
        {
            return true;
        }

        ulong physical = _barBases[index];
        ulong length = _barLengths[index];
        if (_barIsIo[index] || length == 0 || physical == 0)
        {
            WriteLog($"BAR {index} is not an assigned memory BAR");
            return false;
        }

        if (!TryMapBlocks(physical, length))
        {
            WriteLog($"BAR {index} at 0x{physical:X} could not be mapped");
            return false;
        }

        // Memory decoding only: I/O decoding and bus mastering stay as the
        // driver asks for them.
        _device.Command |= PciCommand.Memory;

        region = new MmioRegion(physical + HhdmOffset, length);
        _mmioRegions[index] = region;
        return true;
    }

    /// <summary>
    /// Maps I/O BAR <paramref name="index"/> and turns on I/O decoding.
    /// Asking again for the same BAR returns the same region. Probe only.
    /// </summary>
    /// <param name="index">The BAR slot, 0 to 5.</param>
    /// <param name="region">The mapped BAR, as long as the BAR, when the call returns true.</param>
    /// <returns>
    /// False when the slot holds no assigned I/O BAR, and always where the
    /// platform has no port space, as on ARM64: a driver that runs there
    /// maps a memory BAR instead.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not 0 to 5.</exception>
    /// <exception cref="InvalidOperationException">Called outside the driver's Probe.</exception>
    public bool TryMapIoBar(int index, [NotNullWhen(true)] out PortRegion? region)
    {
        ThrowIfNotProbing(nameof(TryMapIoBar));
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, BarCount);

        region = _portRegions[index];
        if (region is not null)
        {
            return true;
        }

        if (!HasPortSpace)
        {
            WriteLog("this platform has no I/O port space");
            return false;
        }

        ulong basePort = _barBases[index];
        ulong length = _barLengths[index];
        if (!_barIsIo[index] || length == 0 || basePort == 0 || basePort + length > PortSpaceLength)
        {
            WriteLog($"BAR {index} is not an assigned I/O BAR");
            return false;
        }

        _device.Command |= PciCommand.Io;

        region = new PortRegion((ushort)basePort, (ushort)length);
        _portRegions[index] = region;
        return true;
    }

    /// <summary>
    /// Allocates zeroed, physically contiguous memory the function can
    /// reach by DMA, whole pages at a time. Probe only.
    /// </summary>
    /// <param name="length">Bytes the driver needs.</param>
    /// <param name="maximumDeviceAddress">
    /// Highest address the function can generate: <c>uint.MaxValue</c> for a
    /// 32-bit device, <c>ulong.MaxValue</c> for one with no limit.
    /// </param>
    /// <param name="buffer">The allocated buffer when the call returns true.</param>
    /// <returns>
    /// False when no memory is free, or when what the allocator returned
    /// would end above <paramref name="maximumDeviceAddress"/>. There is no
    /// separate low-memory pool: on a machine whose RAM starts above the
    /// limit, as on ARM64 for a 28-bit device, the call always fails.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is zero or negative.</exception>
    /// <exception cref="InvalidOperationException">Called outside the driver's Probe.</exception>
    public unsafe bool TryAllocateDma(int length, ulong maximumDeviceAddress, [NotNullWhen(true)] out DmaBuffer? buffer)
    {
        ThrowIfNotProbing(nameof(TryAllocateDma));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);

        buffer = null;
        ulong pageCount = ((ulong)length + PageAllocator.PageSize - 1) / PageAllocator.PageSize;
        void* pages = PageAllocator.AllocPages(PageType.Unmanaged, pageCount, zero: true);
        if (pages == null)
        {
            WriteLog($"no memory for a {length}-byte DMA buffer");
            return false;
        }

        ulong deviceAddress = PageAllocator.VirtualToPhysical((ulong)pages);
        ulong lastDeviceAddress = deviceAddress + (ulong)length - 1;
        if (lastDeviceAddress > maximumDeviceAddress)
        {
            PageAllocator.Free(pages);
            WriteLog($"the {length}-byte DMA buffer at 0x{deviceAddress:X} ends above 0x{maximumDeviceAddress:X}");
            return false;
        }

        buffer = new DmaBuffer((ulong)pages, deviceAddress, length);
        _dmaBuffers.Add(buffer);
        return true;
    }

    /// <summary>
    /// Lets the function master the bus, so it can reach the DMA buffers.
    /// Bus mastering is off when Probe starts, whatever firmware left. Probe
    /// only; a driver calls it once the device is reset and its DMA
    /// addresses are programmed, not before.
    /// </summary>
    /// <exception cref="InvalidOperationException">Called outside the driver's Probe.</exception>
    public void EnableBusMastering()
    {
        ThrowIfNotProbing(nameof(EnableBusMastering));
        _device.Command |= PciCommand.Master;
    }

    /// <summary>
    /// Asks for the function's interrupts to reach <paramref name="handler"/>
    /// once Probe returns Bound, never before. Through MSI-X when the
    /// function has the capability and the platform can route it (the local
    /// APIC on x64, a GICv3 ITS on ARM64): entry 0 is programmed masked now,
    /// and on Bound the kit turns on bus mastering, which an MSI-X message
    /// needs, and unmasks the entry, so a message the device raised in the
    /// meantime is delivered then. Otherwise the handler is polled from the
    /// platform timer's interrupt, on every tick: about every 55 ms on x64,
    /// 10 ms on ARM64. INTx is never used. One call per binding attempt,
    /// Probe only.
    /// </summary>
    /// <param name="handler">Called in interrupt context, with vector 0; see <see cref="DeviceInterruptHandler"/> for what it may do.</param>
    /// <returns>
    /// False when the function can have neither: no MSI-X the platform can
    /// route, and no ticking timer to poll from (ARM64 without the
    /// scheduler, x64 with ACPI off, or a kernel without the timer).
    /// </returns>
    /// <exception cref="InvalidOperationException">Called outside the driver's Probe, or a second time in the same Probe.</exception>
    public bool TryRequestInterrupts(DeviceInterruptHandler handler)
    {
        ThrowIfNotProbing(nameof(TryRequestInterrupts));
        ArgumentNullException.ThrowIfNull(handler);
        if (_interruptsRequested)
        {
            throw new InvalidOperationException("TryRequestInterrupts can be called once per Probe.");
        }

        _interruptsRequested = true;

        // One trampoline in front of the handler whatever the source, so
        // arming and disarming are a single flag either way.
        InterruptTrampoline trampoline = new(handler, DriverName, Path);
        if (TryEnableMsiX(trampoline))
        {
            _interrupts = trampoline;
            WriteLog("interrupts through MSI-X");
            return true;
        }

        if (InterruptPolling.TryStart(trampoline.OnTimerTick, out SoftwareTimer? pollTimer))
        {
            _pollTimer = pollTimer;
            _interrupts = trampoline;
            WriteLog("interrupts polled from the timer");
            return true;
        }

        WriteLog("no interrupts: no MSI-X the platform can route, and no ticking timer to poll from");
        return false;
    }

    /// <summary>Writes the configuration byte at <paramref name="offset"/>. Thread context only.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> is below 0x40, the header the kit manages, or past the configuration space.</exception>
    /// <exception cref="InvalidOperationException">The binding attempt was declined or failed.</exception>
    public void WriteConfig8(ushort offset, byte value)
    {
        ThrowIfNotDriverConfig(offset, sizeof(byte));
        _device.WriteRegister8((byte)offset, value);
    }

    /// <summary>Writes the 16-bit configuration register at <paramref name="offset"/>, a multiple of 2. Thread context only.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> is below 0x40, the header the kit manages, past the configuration space, or misaligned.</exception>
    /// <exception cref="InvalidOperationException">The binding attempt was declined or failed.</exception>
    public void WriteConfig16(ushort offset, ushort value)
    {
        ThrowIfNotDriverConfig(offset, sizeof(ushort));
        _device.WriteRegister16((byte)offset, value);
    }

    /// <summary>Writes the 32-bit configuration register at <paramref name="offset"/>, a multiple of 4. Thread context only.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> is below 0x40, the header the kit manages, past the configuration space, or misaligned.</exception>
    /// <exception cref="InvalidOperationException">The binding attempt was declined or failed.</exception>
    public void WriteConfig32(ushort offset, uint value)
    {
        ThrowIfNotDriverConfig(offset, sizeof(uint));
        _device.WriteRegister32((byte)offset, value);
    }

    /// <summary>
    /// Releases what a declined or failed attempt acquired, and makes every
    /// later use of the context, its regions, its buffers, its events and
    /// its work items fail.
    /// </summary>
    /// <remarks>
    /// The steps follow the kit's fixed teardown order. A message or tick
    /// that arrives at any point after step 1 stops at the disarmed
    /// trampoline. A message that arrives after step 5 finds, on x64, a
    /// vector with no handler, which the platform still acknowledges, and
    /// on ARM64 no translation at all: the ITS discarded it.
    /// </remarks>
    internal void TearDown()
    {
        State = DeviceContextState.TornDown;

        // 1. Disarm: the handler stops being called before anything it
        // might touch goes away. The MSI-X entry is masked while memory
        // decoding is still on, which the table write needs; the poll timer
        // leaves the timer's list.
        if (_interrupts is { } trampoline)
        {
            trampoline.Disarm();
            if (_msiX is { } maskedMsiX)
            {
                MsiX.MaskEntry(maskedMsiX, MsiXEntry);
            }

            if (_pollTimer is { } pollTimer)
            {
                InterruptPolling.Stop(pollTimer);
                _pollTimer = null;
            }
        }

        // 2. What Probe published never reaches a manager, work scheduled
        // during Probe never runs, and a thread waiting on one of the
        // attempt's events is told it is over.
        DropQueuedAndCancelEvents();

        // 3. The Command register as the attempt found it, but with an
        // endpoint's bus mastering off even if firmware had left it on: no
        // one owns the function now, and the next candidate turns it on
        // itself. Decoding the probe turned on goes off again, and INTx is
        // back as firmware left it. Before the DMA memory goes, so the
        // function can no longer master into it, nor send an MSI-X message.
        _device.Command = _restoredCommand;

        // Read back: an ECAM store can retire before the config write
        // reaches the function, and the read cannot complete ahead of it,
        // nor ahead of a DMA write the function sent before it.
        _ = _device.Command;

        // 4. DMA memory, now that nothing can reach it.
        for (int i = 0; i < _dmaBuffers.Count; i++)
        {
            _dmaBuffers[i].Release();
        }

        _dmaBuffers.Clear();

        // 5. MSI-X off, then its vector (x64) or its LPI and ITS mapping
        // (ARM64) back to the platform. Through config space, which the
        // function answers even with decoding now off.
        if (_msiX is { } enabledMsiX)
        {
            _msiX = null;
            ReleaseMsiX(enabledMsiX);
        }

        // 6. The regions last: nothing above touches them, and a driver still
        // holding one gets an exception instead of reaching a device that is
        // no longer its own.
        for (int i = 0; i < BarCount; i++)
        {
            _mmioRegions[i]?.Invalidate();
            _mmioRegions[i] = null;
            _portRegions[i]?.Invalidate();
            _portRegions[i] = null;
        }
    }

    /// <summary>
    /// Lets the requested interrupts through, now that Probe returned Bound.
    /// With MSI-X, also turns on bus mastering and unmasks the entry.
    /// </summary>
    private protected override void ArmInterrupts()
    {
        if (_interrupts is not { } trampoline)
        {
            return;
        }

        // Armed before the entry is unmasked: a message the device raised
        // during Probe waits in its pending bits and is sent on the unmask,
        // and must find the handler reachable.
        trampoline.Arm();
        if (_msiX is { } msiX)
        {
            // An MSI-X message is a memory write the function masters: sent
            // with bus mastering off, it would be lost rather than held.
            _device.Command |= PciCommand.Master;
            MsiX.UnmaskEntry(msiX, MsiXEntry);
        }
    }

    /// <summary>
    /// Enables MSI-X on the function and binds entry 0 to
    /// <paramref name="trampoline"/>, leaving the entry masked until Bound.
    /// </summary>
    /// <returns>
    /// False, with MSI-X left off, when the platform has no MSI binder, the
    /// function has no MSI-X capability, its table is not inside an assigned
    /// memory BAR that maps, the binder refuses the function, or any step
    /// throws: the caller polls instead.
    /// </returns>
    private bool TryEnableMsiX(InterruptTrampoline trampoline)
    {
        if (!MsiRouting.IsAvailable || !MsiX.TryGetTable(_device, out int bar, out ulong tableOffset, out ulong tableLength))
        {
            return false;
        }

        if (bar >= BarCount || _barIsIo[bar] || _barLengths[bar] == 0 || _barBases[bar] == 0
            || tableOffset >= _barLengths[bar] || _barLengths[bar] - tableOffset < tableLength)
        {
            WriteLog("the MSI-X table is not inside an assigned memory BAR");
            return false;
        }

        if (!TryMapBlocks(_barBases[bar] + tableOffset, tableLength))
        {
            WriteLog($"the MSI-X table in BAR {bar} could not be mapped");
            return false;
        }

        // MsiX.Enable masks every entry through the table before it turns
        // MSI-X on, and those writes only land while the function decodes
        // memory.
        PciCommand before = _device.Command;
        _device.Command = before | PciCommand.Memory;

        MsiXContext? enabled = null;
        try
        {
            enabled = MsiX.Enable(_device);
            if (enabled is { } context)
            {
                MsiX.SetEntryMasked(context, MsiXEntry, trampoline.OnMessage);
                _msiX = context;
                return true;
            }
        }
        catch (Exception exception)
        {
            WriteLog($"MSI-X setup failed: {exception.Message}");
            if (enabled is { } halfEnabled)
            {
                ReleaseMsiX(halfEnabled);
            }
        }

        // Decoding back off, unless it was on before or a BAR the driver
        // mapped needs it.
        if ((before & PciCommand.Memory) == 0 && !HasMappedMemoryBar())
        {
            _device.Command &= ~PciCommand.Memory;
        }

        return false;
    }

    /// <summary>
    /// Turns MSI-X off and hands the bound vector (x64) or LPI and ITS
    /// mapping (ARM64) back to the platform. A failure is logged, not
    /// thrown: whatever the platform could not take back stays allocated,
    /// which is better than leaving the rest of a teardown undone.
    /// </summary>
    private void ReleaseMsiX(MsiXContext context)
    {
        try
        {
            MsiX.Disable(context);
        }
        catch (Exception exception)
        {
            WriteLog($"MSI-X teardown failed, its vector may stay allocated: {exception.Message}");
        }
    }

    /// <summary>True when the driver has mapped at least one memory BAR, which needs memory decoding on.</summary>
    private bool HasMappedMemoryBar()
    {
        for (int i = 0; i < BarCount; i++)
        {
            if (_mmioRegions[i] is not null)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Sizes every BAR of a Type-0 header: writes all ones to the slot, reads
    /// back which address bits stick, and writes the original back. A BAR is
    /// as large as the lowest address bit it decodes. The caller has turned
    /// decoding off. Other header types report no BARs.
    /// </summary>
    private static void SizeBars(PciDevice device, ulong[] bases, ulong[] lengths, bool[] isIo)
    {
        if (device.HeaderType != PciHeaderType.Normal)
        {
            return;
        }

        int slot = 0;
        while (slot < BarCount)
        {
            byte offset = (byte)((byte)Config.Bar0 + slot * BarSlotSize);
            uint original = device.ReadRegister32(offset);

            if ((original & PciDevice.BarIoSpaceMask) != 0)
            {
                device.WriteRegister32(offset, BarSizingPattern);
                uint decoded = device.ReadRegister32(offset) & IoBarAddressMask;
                device.WriteRegister32(offset, original);

                isIo[slot] = true;
                bases[slot] = original & IoBarAddressMask;
                lengths[slot] = LowestSetBit(decoded);
                slot++;
                continue;
            }

            bool is64Bit = ((original >> PciDevice.BarTypeShift) & PciDevice.BarTypeMask) == PciDevice.BarType64Bit;
            if (!is64Bit)
            {
                device.WriteRegister32(offset, BarSizingPattern);
                uint decoded = device.ReadRegister32(offset) & PciDevice.BarMemoryAddressMask;
                device.WriteRegister32(offset, original);

                bases[slot] = original & PciDevice.BarMemoryAddressMask;
                lengths[slot] = LowestSetBit(decoded);
                slot++;
                continue;
            }

            // A 64-bit BAR in the last slot has no upper half: malformed,
            // and left as no BAR.
            if (slot + 1 >= BarCount)
            {
                break;
            }

            byte upperOffset = (byte)(offset + BarSlotSize);
            uint originalUpper = device.ReadRegister32(upperOffset);
            device.WriteRegister32(offset, BarSizingPattern);
            device.WriteRegister32(upperOffset, BarSizingPattern);
            ulong decoded64 = ((ulong)device.ReadRegister32(upperOffset) << PciDevice.BarUpperHalfShift)
                | (device.ReadRegister32(offset) & PciDevice.BarMemoryAddressMask);
            device.WriteRegister32(offset, original);
            device.WriteRegister32(upperOffset, originalUpper);

            bases[slot] = ((ulong)originalUpper << PciDevice.BarUpperHalfShift) | (original & PciDevice.BarMemoryAddressMask);
            lengths[slot] = LowestSetBit(decoded64);

            // The upper half is part of this BAR, not a BAR of its own.
            slot += 2;
        }
    }

    /// <summary>The lowest set bit of <paramref name="value"/>, or 0 when none is.</summary>
    private static ulong LowestSetBit(ulong value) => value & (~value + 1);

    /// <summary>
    /// Maps every 2 MiB block of the physical range
    /// [<paramref name="physical"/>, <paramref name="physical"/> + <paramref name="length"/>)
    /// through the platform. False as soon as one block cannot be mapped.
    /// </summary>
    private static bool TryMapBlocks(ulong physical, ulong length)
    {
        IPlatformInitializer? platform = PlatformHAL.Initializer;
        if (platform is null)
        {
            return false;
        }

        // Counted rather than compared against the end address, so a range
        // ending at the top of the address space cannot wrap the loop.
        ulong firstBlock = physical & ~(MmioBlockSize - 1);
        ulong blockCount = ((physical + length - 1 - firstBlock) / MmioBlockSize) + 1;
        for (ulong i = 0; i < blockCount; i++)
        {
            if (!platform.EnsureMmioMapped(firstBlock + i * MmioBlockSize))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Throws unless a driver may write <paramref name="size"/> bytes at
    /// <paramref name="offset"/>: outside the header the kit manages, inside
    /// the configuration space, naturally aligned, and while the context
    /// still belongs to the driver.
    /// </summary>
    private void ThrowIfNotDriverConfig(ushort offset, int size)
    {
        ThrowIfTornDown();
        ArgumentOutOfRangeException.ThrowIfLessThan(offset, FirstDriverConfigOffset);
        PciFunction.ThrowIfOutsideConfigSpace(offset, size);
    }
}
