// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.ARM64.Bridge;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;

namespace Cosmos.Kernel.Core.ARM64.Cpu;

/// <summary>
/// ARM64 GICv3 ITS implementation of <see cref="IMsiBinder"/>. For each
/// device <see cref="PrepareDevice"/> resolves the PCI BDF to an ITS
/// DeviceID through ACPI IORT (with identity fallback when IORT is
/// missing) and issues <c>MAPD</c>; for each MSI-X entry
/// <see cref="BindEntry"/> allocates an LPI, enables it, wires up
/// <c>(DeviceID, EventID = entryIndex)</c> via <c>MAPTI</c>, and returns
/// <c>(GITS_TRANSLATER_phys, EventID)</c> as the (addr, data) the device
/// will write. Teardown runs the same steps backwards: <c>DISCARD</c> and
/// free each LPI, then <c>MAPD</c> V=0 and free the ITT. Thread context
/// only: the LPI allocator, the ITS command queue and the mapped-device
/// list take plain spinlocks.
/// </summary>
internal sealed class Arm64MsiBinder : IMsiBinder
{
    /// <summary>Bit position of the bus number in a PCI BDF requester ID (bus[15:8], PCIe spec routing ID layout).</summary>
    private const int BdfBusShift = 8;

    /// <summary>Bit position of the device (slot) number in a PCI BDF requester ID (device[7:3], PCIe spec routing ID layout).</summary>
    private const int BdfSlotShift = 3;

    /// <summary>
    /// One mapped DeviceID. The ITT address and the LPI behind each EventID
    /// are kept because teardown needs both and the device only ever hands
    /// back the EventID (the MSI data is the entry index, not the LPI).
    /// </summary>
    private sealed class Arm64DevCtx
    {
        public readonly uint DeviceId;
        public readonly int EntryCount;
        public readonly ulong IttVirt;

        /// <summary>LPI INTID bound per entry; 0 marks an unbound entry (LPIs start at 8192).</summary>
        public readonly uint[] Lpis;

        /// <summary>Set once, under <see cref="s_mappedLock"/>, when the mapping is torn down.</summary>
        public bool Released;

        /// <summary>Next context in the <see cref="s_mapped"/> list.</summary>
        public Arm64DevCtx? Next;

        public Arm64DevCtx(uint deviceId, int entryCount, ulong ittVirt)
        {
            DeviceId = deviceId;
            EntryCount = entryCount;
            IttVirt = ittVirt;
            Lpis = new uint[entryCount];
        }
    }

    /// <summary>
    /// Every DeviceID currently mapped, so a second PrepareDevice for the
    /// same function can find the first mapping and release it: a second
    /// MAPD V=1 over a mapped DeviceID would orphan the first ITT and every
    /// LPI still translated through it. A plain list: a handful of MSI-X
    /// functions exist, and only bring-up and teardown walk it.
    /// </summary>
    private static Arm64DevCtx? s_mapped;
    private static Scheduler.SpinLock s_mappedLock;

    public bool IsAvailable => GICv3Its.IsInitialized && GICv3Lpi.IsInitialized;

    public object? PrepareDevice(uint bus, uint slot, uint function, int entryCount)
    {
        uint bdf = (bus << BdfBusShift) | (slot << BdfSlotShift) | function;
        // Resolve PCI requester ID -> ITS DeviceID via IORT. Fall back to
        // identity (DeviceID == BDF) if no IORT is present.
        //
        // Segment is hardcoded to 0: Cosmos's PCI scan today uses a single
        // ECAM region (one segment). When multi-segment support lands on
        // PciDevice, plumb the device's segment through MsiRouting.PrepareDevice
        // instead of assuming 0 here.
        if (AcpiIortNative.ResolveDeviceId(0, bdf, out uint devId) != 0)
        {
            devId = bdf;
        }

        // An earlier MsiX.Enable on this function that was never disabled
        // still owns the DeviceID. MsiX.Enable has already masked every
        // entry of the table, so its owner receives nothing from here on:
        // unmap it the way ReleaseDevice would before mapping afresh.
        Arm64DevCtx? previous = RetireMapped(devId);
        if (previous is not null)
        {
            Serial.WriteString("[MSI-ITS] DeviceID 0x");
            Serial.WriteHex(devId);
            Serial.WriteString(" was still mapped, releasing the earlier mapping first\n");
            Teardown(previous);
        }

        ulong ittVirt = GICv3Its.MapDevice(devId, (uint)entryCount);
        Arm64DevCtx ctx = new(devId, entryCount, ittVirt);

        s_mappedLock.Acquire();
        try
        {
            ctx.Next = s_mapped;
            s_mapped = ctx;
        }
        finally
        {
            s_mappedLock.Release();
        }

        return ctx;
    }

    public void BindEntry(object? deviceCtx, int entryIndex, InterruptManager.IrqDelegate handler,
                          uint targetCpu, out ulong address, out uint data)
    {
        if (deviceCtx is not Arm64DevCtx ctx)
        {
            throw new System.InvalidOperationException("Arm64MsiBinder: PrepareDevice was not called");
        }

        if (ctx.Released)
        {
            throw new System.InvalidOperationException("Arm64MsiBinder: the device context was released");
        }

        System.ArgumentOutOfRangeException.ThrowIfNegative(entryIndex);
        System.ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(entryIndex, ctx.EntryCount);

        // The ITT holds one translation per EventID: re-binding an entry
        // discards the old one first, which also frees its LPI instead of
        // leaving it allocated with the old handler still registered.
        ReleaseEvent(ctx, entryIndex);

        uint lpi = ARM64InterruptController.AllocateLpi(handler);
        // Recorded before the ITS commands so a teardown after a failed
        // MAPTI still discards the event and frees the LPI.
        ctx.Lpis[entryIndex] = lpi;
        GICv3Lpi.EnableLpi(lpi);
        GICv3Its.MapEvent(ctx.DeviceId, (uint)entryIndex, lpi);

        address = GICv3Its.TranslaterPhysAddr;
        data = (uint)entryIndex;
    }

    /// <summary>
    /// DISCARDs the entry's event, then frees its LPI. The mapping goes
    /// first so no message can translate to the LPI once another device
    /// owns it. Thread context only.
    /// </summary>
    public void UnbindEntry(object? deviceCtx, int entryIndex)
    {
        if (deviceCtx is not Arm64DevCtx ctx || ctx.Released)
        {
            return;
        }

        System.ArgumentOutOfRangeException.ThrowIfNegative(entryIndex);
        System.ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(entryIndex, ctx.EntryCount);
        ReleaseEvent(ctx, entryIndex);
    }

    /// <summary>
    /// Unbinds every entry still bound, unmaps the DeviceID and frees its
    /// ITT. A context already retired, by an earlier release or by a later
    /// PrepareDevice for the same function, is left alone: it no longer
    /// owns the DeviceID, and unmapping it again would cut off the new
    /// owner. Thread context only.
    /// </summary>
    public void ReleaseDevice(object? deviceCtx)
    {
        if (deviceCtx is not Arm64DevCtx ctx || !TryRetire(ctx))
        {
            return;
        }

        Teardown(ctx);
    }

    private static void Teardown(Arm64DevCtx ctx)
    {
        for (int i = 0; i < ctx.EntryCount; i++)
        {
            ReleaseEvent(ctx, i);
        }

        GICv3Its.UnmapDevice(ctx.DeviceId, ctx.IttVirt);
    }

    private static void ReleaseEvent(Arm64DevCtx ctx, int entryIndex)
    {
        uint lpi = ctx.Lpis[entryIndex];
        if (lpi == 0)
        {
            return;
        }

        // A DISCARD that throws leaves the LPI recorded and allocated: the
        // ITS may still translate to it, so handing it out would be worse
        // than leaking it.
        GICv3Its.DiscardEvent(ctx.DeviceId, (uint)entryIndex);
        ARM64InterruptController.FreeLpi(lpi);
        ctx.Lpis[entryIndex] = 0;
    }

    /// <summary>
    /// Marks <paramref name="ctx"/> released and unlinks it, unless it was
    /// released already. Returns whether this call retired it.
    /// </summary>
    private static bool TryRetire(Arm64DevCtx ctx)
    {
        s_mappedLock.Acquire();
        try
        {
            if (ctx.Released)
            {
                return false;
            }

            ctx.Released = true;
            Unlink(ctx);
            return true;
        }
        finally
        {
            s_mappedLock.Release();
        }
    }

    /// <summary>
    /// Retires and returns the live context mapping <paramref name="deviceId"/>,
    /// or null when the DeviceID is not mapped.
    /// </summary>
    private static Arm64DevCtx? RetireMapped(uint deviceId)
    {
        s_mappedLock.Acquire();
        try
        {
            for (Arm64DevCtx? ctx = s_mapped; ctx is not null; ctx = ctx.Next)
            {
                if (ctx.DeviceId == deviceId)
                {
                    ctx.Released = true;
                    Unlink(ctx);
                    return ctx;
                }
            }

            return null;
        }
        finally
        {
            s_mappedLock.Release();
        }
    }

    /// <summary>Removes <paramref name="ctx"/> from <see cref="s_mapped"/>. Caller holds <see cref="s_mappedLock"/>.</summary>
    private static void Unlink(Arm64DevCtx ctx)
    {
        if (s_mapped == ctx)
        {
            s_mapped = ctx.Next;
        }
        else
        {
            for (Arm64DevCtx? prev = s_mapped; prev is not null; prev = prev.Next)
            {
                if (prev.Next == ctx)
                {
                    prev.Next = ctx.Next;
                    break;
                }
            }
        }

        ctx.Next = null;
    }
}
