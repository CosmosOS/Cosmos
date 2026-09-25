// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Core.CPU;

/// <summary>
/// Platform-specific MSI binding backend. Implemented once per arch.
/// Every member runs in thread context only: the vector and LPI
/// allocators behind them take plain spinlocks, which an ISR must never
/// contend for.
/// </summary>
internal interface IMsiBinder
{
    /// <summary>True if the underlying interrupt controller is online and ready to route MSIs.</summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Per-device prep: the binder allocates the per-device state it needs
    /// (the per-entry slot bookkeeping on every arch, plus the ARM64 ITS
    /// Interrupt Translation Table and its MAPD). Returns an opaque context
    /// the binder receives back in <see cref="BindEntry"/>,
    /// <see cref="UnbindEntry"/> and <see cref="ReleaseDevice"/>. Calling it
    /// again for a function whose earlier context was never released
    /// releases that context first (its vectors on x64; its LPIs, DeviceID
    /// mapping and ITT on ARM64), so the new owner starts from a clean
    /// mapping, and a later release of the earlier context does nothing.
    /// </summary>
    object? PrepareDevice(uint bus, uint slot, uint function, int entryCount);

    /// <summary>
    /// Allocate a routing slot for <paramref name="handler"/> and produce
    /// the MSI-X table entry payload (addr/data the device will write).
    /// Binding an entry that is already bound releases its previous slot
    /// first.
    /// </summary>
    void BindEntry(object? deviceCtx, int entryIndex, InterruptManager.IrqDelegate handler,
                   uint targetCpu, out ulong address, out uint data);

    /// <summary>
    /// Return the routing slot <see cref="BindEntry"/> gave entry
    /// <paramref name="entryIndex"/> to its allocator (x64 IDT vector,
    /// ARM64 LPI after an ITS <c>DISCARD</c>). The caller masks the entry
    /// first: a message the device still sends afterwards finds no handler
    /// and is dropped. No-op for an entry that is not bound, for a null
    /// context and for a released one.
    /// </summary>
    void UnbindEntry(object? deviceCtx, int entryIndex);

    /// <summary>
    /// Release everything <see cref="PrepareDevice"/> set up, unbinding any
    /// entry still bound first (ARM64 then unmaps the DeviceID and frees its
    /// ITT). The context is dead afterwards: binding through it throws, and
    /// unbinding or releasing it again does nothing.
    /// </summary>
    void ReleaseDevice(object? deviceCtx);
}
