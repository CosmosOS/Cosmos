// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.ARM64.Cpu;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.HAL.DriverKit.Platform;

namespace Cosmos.Kernel.HAL.ARM64;

/// <summary>
/// The virt machine's line routing: a platform line is a GIC INTID, its
/// handler lives in the dense table slot of that id (ARM64 dispatches by
/// INTID), and the GIC enables, disables and prioritises it. Every routed
/// line is configured level-triggered, which is how the virt machine
/// wires its virtio-mmio slots. One handler per line: a line another
/// handler holds is refused rather than shared.
/// </summary>
internal sealed class GicLineRouting : PlatformLineRouting
{
    /// <summary>The highest line the dense handler table holds: 256 slots, indexed by the INTID itself on ARM64.</summary>
    internal const uint LastTableLine = 255;

    /// <summary>The GIC priority every routed line gets: the middle of the range, the same the virtio lines had.</summary>
    private const byte MediumPriority = 0x80;

    // Filled on first use rather than by a static initializer: the machine
    // description creates its sources before the scheduler exists, and a
    // class constructor that allocates needs a current thread.
    private static GicLineRouting? s_instance;

    /// <summary>Only <see cref="Instance"/> constructs the routing.</summary>
    private GicLineRouting()
    {
    }

    /// <summary>The one routing of this platform. Thread context.</summary>
    public static GicLineRouting Instance => s_instance ??= new GicLineRouting();

    /// <inheritdoc/>
    public override bool TryConnect(uint line, InterruptManager.IrqDelegate handler)
    {
        if (line > LastTableLine || InterruptManager.HasHandler((byte)line))
        {
            return false;
        }

        // The handler goes in before the line is enabled: a level line that
        // is already asserted fires the moment the GIC enables it.
        InterruptManager.SetHandler((byte)line, handler);
        GIC.ConfigureInterrupt(line, edgeTriggered: false);
        GIC.SetPriority(line, MediumPriority);
        GIC.EnableInterrupt(line);
        return true;
    }

    /// <inheritdoc/>
    public override void Disconnect(uint line)
    {
        GIC.DisableInterrupt(line);
        InterruptManager.SetHandler((byte)line, null);
    }

    /// <inheritdoc/>
    public override void Mask(uint line) => GIC.DisableInterrupt(line);

    /// <inheritdoc/>
    public override void Unmask(uint line) => GIC.EnableInterrupt(line);
}
