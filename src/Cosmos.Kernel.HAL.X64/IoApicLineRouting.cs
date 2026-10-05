// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.HAL.DriverKit.Platform;

namespace Cosmos.Kernel.HAL.X64;

/// <summary>
/// The q35 machine's line routing: a platform line is an ISA IRQ number 0
/// to 15, its handler lives in the slot at vector 0x20 + line, and the I/O
/// APIC routes it edge-triggered and active-high with the MADT override
/// applied, as <see cref="InterruptManager.SetIrqHandler"/> programs it.
/// One handler per line: a line another handler holds (the PIT's IRQ 0, a
/// PCI function's legacy line) is refused rather than shared, and
/// <c>PciLineInterruptSource</c> refuses a line this routing holds the same
/// way.
/// </summary>
internal sealed class IoApicLineRouting : PlatformLineRouting
{
    /// <summary>The last ISA line: the platform's line primitives are defined for 0 to 15.</summary>
    private const uint LastIsaLine = 15;

    // Filled on first use rather than by a static initializer: the machine
    // description creates its sources before the scheduler exists, and a
    // class constructor that allocates needs a current thread.
    private static IoApicLineRouting? s_instance;

    /// <summary>Only <see cref="Instance"/> constructs the routing.</summary>
    private IoApicLineRouting()
    {
    }

    /// <summary>The one routing of this platform. Thread context.</summary>
    public static IoApicLineRouting Instance => s_instance ??= new IoApicLineRouting();

    /// <inheritdoc/>
    public override bool TryConnect(uint line, InterruptManager.IrqDelegate handler)
    {
        if (line > LastIsaLine || !InterruptManager.IsControllerInitialized || InterruptManager.HasIrqHandler((byte)line))
        {
            return false;
        }

        // The slot is installed first and the redirection entry then routed
        // unmasked. The entry is edge-triggered, so a device must be told to
        // raise (the 8042's configuration bits) only after this returned,
        // and a byte already in its output buffer produces no edge: the
        // driver drains it.
        InterruptManager.SetIrqHandler((byte)line, handler);
        return true;
    }

    /// <inheritdoc/>
    public override void Disconnect(uint line) => InterruptManager.ClearIrqHandler((byte)line);

    /// <inheritdoc/>
    public override void Mask(uint line) => InterruptManager.MaskIrq((byte)line);

    /// <inheritdoc/>
    public override void Unmask(uint line) => InterruptManager.UnmaskIrq((byte)line);
}
