// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.RISCV64.Cpu;
using Cosmos.Kernel.HAL.DriverKit.Buses.Platform;

namespace Cosmos.Kernel.HAL.RISCV64;

/// <summary>
/// The virt machine's line routing: a platform line is a PLIC source, its
/// handler lives in the dense table slot <see cref="RISCV64InterruptController.ExternalVectorBase"/>
/// plus the source (the interrupt controller dispatches a claimed source
/// there), and the PLIC enables, disables and prioritises it. The PLIC
/// latches level and edge sources alike, so there is no trigger mode to
/// program. One handler per line: a line another handler holds is refused
/// rather than shared.
/// </summary>
internal sealed class PlicLineRouting : PlatformLineRouting
{
    // Filled on first use rather than by a static initializer: the machine
    // description creates its sources before the scheduler exists, and a
    // class constructor that allocates needs a current thread.
    private static PlicLineRouting? s_instance;

    /// <summary>Only <see cref="Instance"/> constructs the routing.</summary>
    private PlicLineRouting()
    {
    }

    /// <summary>The one routing of this platform. Thread context.</summary>
    public static PlicLineRouting Instance => s_instance ??= new PlicLineRouting();

    /// <inheritdoc/>
    public override bool TryConnect(uint line, InterruptManager.IrqDelegate handler)
    {
        if (line == 0 || line > RISCV64InterruptController.MaxExternalSource)
        {
            return false;
        }

        byte vector = (byte)(RISCV64InterruptController.ExternalVectorBase + line);
        if (InterruptManager.HasHandler(vector))
        {
            return false;
        }

        // The handler goes in before the source is enabled: a level source
        // that is already asserted interrupts the moment it is enabled.
        InterruptManager.SetHandler(vector, handler);
        Plic.SetPriority(line, Plic.DefaultPriority);
        Plic.Enable(line);
        return true;
    }

    /// <inheritdoc/>
    public override void Disconnect(uint line)
    {
        if (line == 0 || line > RISCV64InterruptController.MaxExternalSource)
        {
            return;
        }

        Plic.Disable(line);
        InterruptManager.SetHandler((byte)(RISCV64InterruptController.ExternalVectorBase + line), null);
    }

    /// <inheritdoc/>
    public override void Mask(uint line) => Plic.Disable(line);

    /// <inheritdoc/>
    public override void Unmask(uint line) => Plic.Enable(line);
}
