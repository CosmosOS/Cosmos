// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;

namespace Cosmos.Kernel.Core.X64.Cpu;

/// <summary>
/// x64 LAPIC implementation of <see cref="IMsiBinder"/>. Allocates an IDT
/// vector for each MSI-X entry; the device's MSI-X table writes
/// <c>data = vector</c> to the LAPIC's MSI doorbell at
/// <c>0xFEE00000 | (apicId &lt;&lt; 12)</c>. Thread context only: the
/// vector allocator and the live-context list take plain spinlocks.
/// </summary>
internal sealed class X64MsiBinder : IMsiBinder
{
    /// <summary>MSI message address base for x64 (Intel SDM Vol. 3, "Message Address Register Format").</summary>
    private const ulong MsiAddressBase = 0xFEE00000UL;

    /// <summary>Bit position of the Destination ID field in the MSI message address (bits 19:12).</summary>
    private const int MsiDestinationIdShift = 12;

    /// <summary>Bit position of the bus number in the requester ID a context is keyed by (bus[15:8], PCIe routing ID layout).</summary>
    private const int BdfBusShift = 8;

    /// <summary>Bit position of the device (slot) number in the requester ID a context is keyed by (device[7:3], PCIe routing ID layout).</summary>
    private const int BdfSlotShift = 3;

    /// <summary>
    /// One function's MSI-X context. The vector behind each entry is kept so
    /// teardown frees exactly the vectors this device holds:
    /// <see cref="InterruptManager.FreeVector"/> takes a bare number, and
    /// freeing one twice would strip whichever device allocated it in between.
    /// </summary>
    private sealed class X64DevCtx
    {
        /// <summary>Requester ID of the function, the key a later PrepareDevice looks the context up by.</summary>
        public readonly uint Bdf;

        /// <summary>IDT vector bound per entry; 0 marks an unbound entry (no dynamic vector is below 0x40).</summary>
        public readonly byte[] Vectors;

        /// <summary>Set once, under <see cref="s_liveLock"/>, when the context is retired.</summary>
        public bool Released;

        /// <summary>Next context in the <see cref="s_live"/> list.</summary>
        public X64DevCtx? Next;

        public X64DevCtx(uint bdf, int entryCount)
        {
            Bdf = bdf;
            Vectors = new byte[entryCount];
        }
    }

    /// <summary>
    /// Every context not yet released, so a second PrepareDevice for the
    /// same function can find the first and free its vectors: the owner
    /// that walked away without <c>MsiX.Disable</c> never will, and each
    /// vector stays allocated with its handler rooted and invokable, out of
    /// 175 dynamic slots. Same take-over as the ARM64 binder. A plain list:
    /// a handful of MSI-X functions exist, and only bring-up and teardown
    /// walk it.
    /// </summary>
    private static X64DevCtx? s_live;
    private static Scheduler.SpinLock s_liveLock;

    public bool IsAvailable => LocalApic.IsInitialized;

    /// <summary>
    /// x64 MSI delivery is per-vector, not per-device: the LAPIC has no
    /// device-side state to set up. The context records the vector each
    /// entry holds. A context the function still has from an earlier
    /// owner is retired and its vectors freed first.
    /// </summary>
    public object? PrepareDevice(uint bus, uint slot, uint function, int entryCount)
    {
        uint bdf = (bus << BdfBusShift) | (slot << BdfSlotShift) | function;

        // An earlier MsiX.Enable on this function that was never disabled
        // still holds its vectors. MsiX.Enable has already masked every
        // entry of the table, so its owner receives nothing from here on:
        // free them the way ReleaseDevice would. A message already on its
        // way still gets its EOI from the dispatcher's no-handler path.
        X64DevCtx? previous = RetireLive(bdf);
        if (previous is not null)
        {
            Serial.WriteString("[MSI] function 0x");
            Serial.WriteHex(bdf);
            Serial.WriteString(" still had an unreleased context, releasing it first\n");
            ReleaseVectors(previous);
        }

        X64DevCtx ctx = new(bdf, entryCount);

        s_liveLock.Acquire();
        try
        {
            ctx.Next = s_live;
            s_live = ctx;
        }
        finally
        {
            s_liveLock.Release();
        }

        return ctx;
    }

    /// <summary>
    /// Allocates a vector for <paramref name="handler"/>. With a context
    /// from <see cref="PrepareDevice"/> the vector is recorded against
    /// <paramref name="entryIndex"/>, releasing any vector that entry held;
    /// a null context allocates an untracked vector that nothing frees.
    /// </summary>
    public void BindEntry(object? deviceCtx, int entryIndex, InterruptManager.IrqDelegate handler,
                          uint targetCpu, out ulong address, out uint data)
    {
        X64DevCtx? device = deviceCtx as X64DevCtx;
        if (device is not null)
        {
            if (device.Released)
            {
                throw new System.InvalidOperationException("X64MsiBinder: the device context was released");
            }

            System.ArgumentOutOfRangeException.ThrowIfNegative(entryIndex);
            System.ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(entryIndex, device.Vectors.Length);
            ReleaseVector(device, entryIndex);
        }

        byte vector = InterruptManager.AllocateVector(handler);
        device?.Vectors[entryIndex] = vector;

        // The CPU index is not the LAPIC destination ID: the BSP's APIC ID
        // is not architecturally guaranteed to be 0 (QEMU's is; real
        // multi-socket parts often differ), and an MSI aimed at a
        // nonexistent LAPIC is silently dropped. Until SMP brings a
        // MADT-backed index→APIC-ID map, resolve the boot CPU's real ID at
        // bind time; other indices are unreachable today (single-CPU).
        byte apicId = targetCpu == 0 ? LocalApic.GetId() : (byte)targetCpu;
        address = MsiAddressBase | ((ulong)apicId << MsiDestinationIdShift);
        data = vector;
    }

    /// <summary>
    /// Frees the vector entry <paramref name="entryIndex"/> holds. A message
    /// already latched in the LAPIC for it still gets its EOI from the
    /// dispatcher's no-handler path, so freeing is safe once the entry is
    /// masked. Thread context only.
    /// </summary>
    public void UnbindEntry(object? deviceCtx, int entryIndex)
    {
        if (deviceCtx is not X64DevCtx device || device.Released)
        {
            return;
        }

        System.ArgumentOutOfRangeException.ThrowIfNegative(entryIndex);
        System.ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(entryIndex, device.Vectors.Length);
        ReleaseVector(device, entryIndex);
    }

    /// <summary>
    /// Retires the context and frees every vector it still holds. There is
    /// no device-side state to undo on x64. A context already retired, by
    /// an earlier release or by a later PrepareDevice for the same
    /// function, is left alone: its vectors are already free, and may be
    /// another device's by now. Thread context only.
    /// </summary>
    public void ReleaseDevice(object? deviceCtx)
    {
        if (deviceCtx is not X64DevCtx device || !TryRetire(device))
        {
            return;
        }

        ReleaseVectors(device);
    }

    private static void ReleaseVectors(X64DevCtx device)
    {
        for (int i = 0; i < device.Vectors.Length; i++)
        {
            ReleaseVector(device, i);
        }
    }

    private static void ReleaseVector(X64DevCtx device, int entryIndex)
    {
        byte vector = device.Vectors[entryIndex];
        if (vector == 0)
        {
            return;
        }

        device.Vectors[entryIndex] = 0;
        InterruptManager.FreeVector(vector);
    }

    /// <summary>
    /// Marks <paramref name="device"/> released and unlinks it, unless it
    /// was released already. Returns whether this call retired it.
    /// </summary>
    private static bool TryRetire(X64DevCtx device)
    {
        s_liveLock.Acquire();
        try
        {
            if (device.Released)
            {
                return false;
            }

            device.Released = true;
            Unlink(device);
            return true;
        }
        finally
        {
            s_liveLock.Release();
        }
    }

    /// <summary>
    /// Retires and returns the live context of requester ID
    /// <paramref name="bdf"/>, or null when the function has none.
    /// </summary>
    private static X64DevCtx? RetireLive(uint bdf)
    {
        s_liveLock.Acquire();
        try
        {
            for (X64DevCtx? device = s_live; device is not null; device = device.Next)
            {
                if (device.Bdf == bdf)
                {
                    device.Released = true;
                    Unlink(device);
                    return device;
                }
            }

            return null;
        }
        finally
        {
            s_liveLock.Release();
        }
    }

    /// <summary>Removes <paramref name="device"/> from <see cref="s_live"/>. Caller holds <see cref="s_liveLock"/>.</summary>
    private static void Unlink(X64DevCtx device)
    {
        if (s_live == device)
        {
            s_live = device.Next;
        }
        else
        {
            for (X64DevCtx? prev = s_live; prev is not null; prev = prev.Next)
            {
                if (prev.Next == device)
                {
                    prev.Next = device.Next;
                    break;
                }
            }
        }

        device.Next = null;
    }
}
