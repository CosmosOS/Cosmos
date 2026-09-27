// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Pci;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Svga;

/// <summary>
/// The built-in VMware SVGA II driver: takes the adapter, agrees the version 2
/// protocol, starts its command FIFO and publishes it as the kernel's display.
/// </summary>
/// <remarks>
/// It matches the adapter by device ID rather than by class, because the
/// adapter reports the VGA-compatible class every emulated display does, and
/// binding every one of those would take the boot framebuffer away from
/// firmware. It programs no mode: the pass binds long before anything asks for
/// the screen, and the boot console is still drawing through firmware's
/// framebuffer until a canvas acquires the display and chooses a mode.
/// </remarks>
[Experimental("COSMOS0003")]
public sealed class SvgaDriver : PciDriver
{
    private const string DriverName = "vmware-svga";
    private const ushort VmwareVendorId = 0x15AD;
    private const ushort SvgaIIDeviceId = 0x0405;

    /// <summary>The register ports, index and value, one dword apart.</summary>
    private const int PortBarIndex = 0;

    /// <summary>Video memory, which holds the visible frame and the back buffer.</summary>
    private const int FrameBufferBarIndex = 1;

    /// <summary>The command FIFO the host reads from.</summary>
    private const int FifoBarIndex = 2;

    private SvgaDriver()
    {
    }

    /// <summary>
    /// The registration <c>Global.StartKernel</c> hands the kit, under the
    /// reserved built-in name <c>vmware-svga</c>.
    /// </summary>
    public static PciDriverRegistration CreateRegistration() =>
        new(DriverName, static () => new SvgaDriver(),
            PciMatch.Device(VmwareVendorId, SvgaIIDeviceId));

    /// <inheritdoc />
    protected override ProbeResult Probe(PciDeviceContext context)
    {
        // Port space only: the adapter's registers are an I/O BAR, and no
        // architecture without port space carries one of these.
        if (!context.TryMapIOBar(PortBarIndex, out PortRegion? ports))
        {
            return ProbeResult.Declined;
        }

        if (!context.TryMapBar(FrameBufferBarIndex, out MmioRegion? frameBuffer) ||
            !context.TryMapBar(FifoBarIndex, out MmioRegion? fifo))
        {
            return ProbeResult.Declined;
        }

        SvgaSurface surface;
        try
        {
            surface = new SvgaSurface(ports, frameBuffer, fifo, context.WriteLog);
        }
        catch (InvalidOperationException exception)
        {
            // An adapter that refuses the version 2 protocol is one this
            // driver cannot drive, and the screen stays firmware's.
            context.WriteLog($"declined the adapter: {exception.Message}");
            return ProbeResult.Declined;
        }

        context.PublishDisplay(surface);
        return ProbeResult.Bound;
    }
}
