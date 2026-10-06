// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.System.Graphics;

namespace Cosmos.Kernel.Drivers.Pci.Display.VmwareSvga;

/// <summary>
/// The <see cref="VmwareSvgaState"/> published when the host negotiated
/// SVGA3D: the same display and facets, plus the ring's
/// <see cref="ICanvas3DFactory"/>, so <see cref="Canvas.GetFullScreen()"/>
/// hands out the SVGA3D canvas. QEMU never negotiates 3D, so on QEMU the
/// base state is published and the 3D canvas is reachable only through
/// <see cref="ISvgaAdapter.CreateCanvas3D"/>. Thread context; one caller
/// at a time, as the ring's canvas is.
/// </summary>
public sealed class VmwareSvga3DState : VmwareSvgaState, ICanvas3DFactory
{
    /// <summary>
    /// Takes the registers, the FIFO and the VRAM the probe mapped, as the
    /// base does. Thread context, from the probe.
    /// </summary>
    /// <param name="binding">The device's binding, for the log.</param>
    /// <param name="fifo">The adapter's registers and FIFO, initialised, with 3D negotiated.</param>
    /// <param name="vram">The adapter's VRAM, BAR 1.</param>
    internal VmwareSvga3DState(DeviceBinding binding, VmwareSvgaFifo fifo, DeviceRegion vram)
        : base(binding, fifo, vram)
    {
    }
}
