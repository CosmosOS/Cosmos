// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Core.CPU;

/// <summary>
/// Platform-specific MSI binding backend. Implemented once per arch.
/// </summary>
internal interface IMsiBinder
{
    /// <summary>True if the underlying interrupt controller is online and ready to route MSIs.</summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Per-device prep: the binder may need to allocate per-device state
    /// (e.g. ARM64 ITS Interrupt Translation Table, issue a MAPD command).
    /// Return an opaque object the binder will receive in
    /// <see cref="BindEntry"/>, or null if no state is needed (x64).
    /// </summary>
    object? PrepareDevice(uint bus, uint slot, uint function, int entryCount);

    /// <summary>
    /// Allocate a routing slot for <paramref name="handler"/> and produce
    /// the MSI-X table entry payload (addr/data the device will write).
    /// </summary>
    void BindEntry(object? deviceCtx, int entryIndex, InterruptManager.IrqDelegate handler,
                   uint targetCpu, out ulong address, out uint data);
}
