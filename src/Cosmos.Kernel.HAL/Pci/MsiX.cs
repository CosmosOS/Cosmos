// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Boot.Limine;
using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Pci.Enums;

namespace Cosmos.Kernel.HAL.Pci;

/// <summary>
/// Programs the MSI-X capability of a PCI device (cap ID 0x11).
/// Architecture-neutral: the message address/data are produced by the
/// platform-registered <see cref="MsiRouting"/> backend, so the same
/// table programming works whether interrupts ultimately land on an x64
/// LAPIC or an ARM64 GICv3 ITS.
/// </summary>
internal static class MsiX
{
    public const byte CapId = 0x11;

    /// <summary>Offset of the Message Control register within the MSI-X capability (PCI 3.0 §6.8.2.3).</summary>
    private const byte MsgCtrlOffset = 0x02;
    /// <summary>Offset of the Table Offset/Table BIR register within the MSI-X capability (PCI 3.0 §6.8.2.4). Public: MsiX owns the capability layout; drivers decoding the table location share these.</summary>
    public const byte TableOffsetBirOffset = 0x04;

    /// <summary>Mask selecting the BAR Indicator Register (BIR) bits of the Table Offset/BIR register.</summary>
    public const uint TableBirMask = 0x7;
    /// <summary>Mask selecting the QWORD-aligned table offset bits of the Table Offset/BIR register.</summary>
    public const uint TableOffsetMask = 0xFFFFFFF8u;

    /// <summary>Right shift extracting the high 32 bits of the 64-bit message address.</summary>
    private const int AddrHighDwordShift = 32;

    /// <summary>Message Control bit 15 — MSI-X Enable (PCI 3.0 §6.8.2.3).</summary>
    private const ushort MsgCtrlEnable = 1 << 15;
    /// <summary>Message Control bit 14 — Function Mask: masks every table entry while set (PCI 3.0 §6.8.2.3).</summary>
    private const ushort MsgCtrlFunctionMask = 1 << 14;
    /// <summary>Message Control bits 10:0 — Table Size, encoded as entry count minus one (PCI 3.0 §6.8.2.3).</summary>
    private const ushort MsgCtrlTableSizeMask = 0x07FF;

    /// <summary>
    /// Per-entry layout (PCI 3.0 §6.8.2.9): 16 bytes, MsgAddrLo @0,
    /// MsgAddrHi @4, MsgData @8, VectorControl @12 (bit 0 = mask).
    /// </summary>
    private const uint EntryStride = 16;
    private const uint EntryAddrLo = 0;
    private const uint EntryAddrHi = 4;
    private const uint EntryData = 8;
    private const uint EntryVectorControl = 12;
    private const uint VectorControlMask = 1;

    /// <summary>
    /// Locates and enables the MSI-X capability on <paramref name="pci"/>:
    /// maps the table BAR via HHDM, masks every entry, sets MSI-X Enable,
    /// clears Function Mask, and disables INTx for the device. Returns
    /// null if the device has no MSI-X capability or the platform cannot
    /// route it. <see cref="Disable"/> undoes it. Thread context only.
    /// </summary>
    public static unsafe MsiXContext? Enable(PciDevice pci)
    {
        if (!MsiRouting.IsAvailable)
        {
            Serial.WriteString("[MSI-X] no routing backend registered, refusing to enable\n");
            return null;
        }

        byte cap = pci.FindCapability(CapId);
        if (cap == 0)
        {
            return null;
        }

        ushort msgCtrl = pci.ReadRegister16((byte)(cap + MsgCtrlOffset));
        int tableSize = (msgCtrl & MsgCtrlTableSizeMask) + 1;

        uint tableBirOff = pci.ReadRegister32((byte)(cap + TableOffsetBirOffset));
        int bir = (int)(tableBirOff & TableBirMask);
        uint tableOffset = tableBirOff & TableOffsetMask;

        ulong barPhys = pci.GetBar64Address(bir);
        if (barPhys == 0)
        {
            Serial.WriteString("[MSI-X] table BAR is I/O or out of range\n");
            return null;
        }

        ulong hhdmOffset = Limine.HHDM.Response != null ? Limine.HHDM.Response->Offset : 0;
        ulong tableVirt = barPhys + hhdmOffset + tableOffset;

        // The table BAR is not necessarily the register BAR the driver
        // already mapped (QEMU's NVMe puts the table in its own BAR): make
        // sure both ends of the table's HHDM alias are device-mapped before
        // the masking loop below dereferences it. No-op on x64 below 4 GiB.
        PlatformHAL.Initializer?.EnsureMmioMapped(barPhys + tableOffset);
        PlatformHAL.Initializer?.EnsureMmioMapped(barPhys + tableOffset + (ulong)tableSize * EntryStride - 1);

        // Mask every entry before turning the function on so no stale
        // garbage in the table can fire as soon as we set MSI-X Enable.
        for (int i = 0; i < tableSize; i++)
        {
            ulong entry = tableVirt + (ulong)i * EntryStride;
            Native.MMIO.Write32(entry + EntryVectorControl, VectorControlMask);
        }

        // Per-arch device prep (ARM64 ITS allocates an ITT + MAPDs the
        // device here; x64 tracks vectors). A binder that cannot route this
        // device (e.g. its ITS DeviceID exceeds the device table) throws —
        // turn that into "no MSI-X" so the driver takes its polled
        // fallback instead of enabling MSI-X that can never deliver.
        object? deviceCtx;
        try
        {
            deviceCtx = MsiRouting.PrepareDevice(pci.Bus, pci.Slot, pci.Function, tableSize);
        }
        catch (System.InvalidOperationException)
        {
            Serial.WriteString("[MSI-X] platform binder rejected the device, leaving MSI-X disabled\n");
            return null;
        }

        // Enable MSI-X, clear function mask.
        msgCtrl = (ushort)((msgCtrl & ~MsgCtrlFunctionMask) | MsgCtrlEnable);
        pci.WriteRegister16((byte)(cap + MsgCtrlOffset), msgCtrl);

        // Disable legacy INTx delivery so the same line can't double-fire.
        ushort cmd = pci.ReadRegister16((byte)Config.Command);
        pci.WriteRegister16((byte)Config.Command, (ushort)(cmd | (ushort)PciCommand.InterruptDisable));

        return new MsiXContext(pci, cap, tableVirt, tableSize, deviceCtx);
    }

    /// <summary>
    /// Allocate a routing slot (IDT vector on x64, LPI on ARM64) for
    /// <paramref name="handler"/> via <see cref="MsiRouting"/>, then
    /// program entry <paramref name="index"/> to deliver to it and unmask.
    /// Thread context only.
    /// </summary>
    public static void SetEntry(MsiXContext ctx, int index, InterruptManager.IrqDelegate handler, uint targetCpu = 0)
    {
        ProgramEntry(ctx, index, handler, targetCpu);
        Native.MMIO.Write32(EntryAddress(ctx, index) + EntryVectorControl, 0);
    }

    /// <summary>
    /// Same as <see cref="SetEntry"/>, but entry <paramref name="index"/> is
    /// left masked, so a driver still being built never sees the vector
    /// fire. A message the device raises meanwhile is held in its Pending
    /// Bit Array and delivered once <see cref="UnmaskEntry"/> runs. Thread
    /// context only.
    /// </summary>
    public static void SetEntryMasked(MsiXContext ctx, int index, InterruptManager.IrqDelegate handler, uint targetCpu = 0) =>
        ProgramEntry(ctx, index, handler, targetCpu);

    /// <summary>
    /// Undoes <see cref="Enable"/>: sets Function Mask, masks every entry,
    /// clears MSI-X Enable with Function Mask left set, then hands each
    /// entry's vector / LPI and the device's routing state back to the
    /// platform binder. <paramref name="ctx"/> is dead afterwards; a new
    /// <see cref="Enable"/> on the function starts over.
    /// <para>
    /// The Command register is left alone, so INTx stays disabled as
    /// Enable left it and the function raises no interrupt at all. Enable
    /// does not record what that bit held before, so the owner that saved
    /// the Command register restores it.
    /// </para>
    /// <para>Thread context only: the binder's allocators take plain spinlocks.</para>
    /// </summary>
    /// <exception cref="System.ArgumentException"><paramref name="ctx"/> was not returned by <see cref="Enable"/>.</exception>
    public static void Disable(MsiXContext ctx)
    {
        PciDevice device = ctx.Device
            ?? throw new System.ArgumentException("Only a context returned by MsiX.Enable has a function to disable", nameof(ctx));
        byte msgCtrlRegister = (byte)(ctx.CapabilityOffset + MsgCtrlOffset);

        // Function Mask first: one config write masks every vector at once,
        // and config space answers even with memory decode off.
        ushort msgCtrl = (ushort)(device.ReadRegister16(msgCtrlRegister) | MsgCtrlFunctionMask);
        device.WriteRegister16(msgCtrlRegister, msgCtrl);

        // Per-entry masks leave the table as Enable expects to find it, but
        // only with memory decode on: Function Mask already covers every
        // entry, and a table write the function does not decode is an
        // unclaimed posted write, dropped on x64 and an asynchronous abort
        // on some ARM64 root complexes.
        if ((device.Command & PciCommand.Memory) != 0)
        {
            for (int i = 0; i < ctx.EntryCount; i++)
            {
                Native.MMIO.Write32(EntryAddress(ctx, i) + EntryVectorControl, VectorControlMask);
            }
        }

        device.WriteRegister16(msgCtrlRegister, (ushort)(msgCtrl & ~MsgCtrlEnable));

        // Read back: an ECAM store can retire before the config write
        // reaches the function, and the read cannot complete ahead of it.
        // The function has stopped signalling before any vector goes back
        // to the allocator for another device to be handed.
        _ = device.ReadRegister16(msgCtrlRegister);

        for (int i = 0; i < ctx.EntryCount; i++)
        {
            MsiRouting.UnbindEntry(ctx.DeviceCtx, i);
        }

        MsiRouting.ReleaseDevice(ctx.DeviceCtx);
    }

    public static void MaskEntry(MsiXContext ctx, int index)
    {
        if (index < 0 || index >= ctx.EntryCount)
        {
            throw new System.ArgumentOutOfRangeException(nameof(index));
        }

        ulong entry = ctx.TableVirt + (ulong)index * EntryStride;
        Native.MMIO.Write32(entry + EntryVectorControl, VectorControlMask);
    }

    public static void UnmaskEntry(MsiXContext ctx, int index)
    {
        if (index < 0 || index >= ctx.EntryCount)
        {
            throw new System.ArgumentOutOfRangeException(nameof(index));
        }

        ulong entry = ctx.TableVirt + (ulong)index * EntryStride;
        Native.MMIO.Write32(entry + EntryVectorControl, 0);
    }

    /// <summary>
    /// Masks entry <paramref name="index"/>, binds a routing slot for
    /// <paramref name="handler"/> and writes the message address and data,
    /// leaving the entry masked. The mask comes first because an entry
    /// whose address or data changes while unmasked is undefined (PCI 3.0
    /// §6.8.3.5), which matters when an entry is bound a second time.
    /// </summary>
    private static void ProgramEntry(MsiXContext ctx, int index, InterruptManager.IrqDelegate handler, uint targetCpu)
    {
        System.ArgumentOutOfRangeException.ThrowIfNegative(index);
        System.ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, ctx.EntryCount);

        ulong entry = EntryAddress(ctx, index);
        Native.MMIO.Write32(entry + EntryVectorControl, VectorControlMask);

        MsiRouting.BindEntry(ctx.DeviceCtx, index, handler, targetCpu, out ulong address, out uint data);

        Native.MMIO.Write32(entry + EntryAddrLo, (uint)address);
        Native.MMIO.Write32(entry + EntryAddrHi, (uint)(address >> AddrHighDwordShift));
        Native.MMIO.Write32(entry + EntryData, data);
    }

    private static ulong EntryAddress(MsiXContext ctx, int index) => ctx.TableVirt + (ulong)index * EntryStride;
}

/// <summary>
/// Handle returned by <see cref="MsiX.Enable"/> identifying the mapped
/// MSI-X table for a device. Carries the platform-binder's per-device
/// state (e.g. ARM64 ITS DeviceID + ITT pointer) so subsequent
/// <see cref="MsiX.SetEntry"/> calls can route through the same context,
/// and the function and capability offset <see cref="MsiX.Disable"/>
/// writes.
/// </summary>
internal readonly struct MsiXContext
{
    public ulong TableVirt { get; }
    public int EntryCount { get; }
    public object? DeviceCtx { get; }

    /// <summary>The function whose capability this context programs; null for a table-only context.</summary>
    public PciDevice? Device { get; }

    /// <summary>Config-space offset of the MSI-X capability; 0 for a table-only context.</summary>
    public byte CapabilityOffset { get; }

    /// <summary>Context for the MSI-X capability of <paramref name="device"/>, as <see cref="MsiX.Enable"/> builds it.</summary>
    public MsiXContext(PciDevice device, byte capabilityOffset, ulong tableVirt, int entryCount, object? deviceCtx)
    {
        Device = device;
        CapabilityOffset = capabilityOffset;
        TableVirt = tableVirt;
        EntryCount = entryCount;
        DeviceCtx = deviceCtx;
    }

    /// <summary>
    /// Table-only context: entries can be programmed and masked, but there
    /// is no Message Control to write, so <see cref="MsiX.Disable"/>
    /// refuses it. The white-box interrupt tests build one over a scratch
    /// page so they never touch a live device's table.
    /// </summary>
    public MsiXContext(ulong tableVirt, int entryCount, object? deviceCtx)
    {
        TableVirt = tableVirt;
        EntryCount = entryCount;
        DeviceCtx = deviceCtx;
    }
}
