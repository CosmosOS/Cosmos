// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.System.Graphics;

namespace Cosmos.Kernel.Drivers.Pci.Display.VmwareSvga;

/// <summary>
/// The adapter behind a VMware SVGA II display: its capabilities, the
/// scanout enable bit, the FIFO positions a wire test inspects, and the
/// SVGA3D canvas whether or not the host negotiated 3D. A test and tooling
/// seam, found through <c>DisplayDevice.TryGetFacet</c> on the display
/// <see cref="VmwareSvgaDriver"/> published; a kernel that wants 3D uses
/// <see cref="Canvas.GetFullScreen()"/>. The facet exposes no register
/// names, command ids or command structs: only FIFO dwords by offset, the
/// enable bit and the negotiation facts. Thread context; one caller at a
/// time, as the ring's canvas is.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public interface ISvgaAdapter
{
    /// <summary>The adapter's Capabilities register: the SVGA_CAP_* bits.</summary>
    uint Capabilities { get; }

    /// <summary>True when the FIFO initialisation negotiated an SVGA3D version the command layer targets. QEMU never negotiates one.</summary>
    bool Is3DNegotiated { get; }

    /// <summary>The SVGA3D hardware version the host published, major in the high half and minor in the low half; 0 without 3D.</summary>
    uint Svga3DVersion { get; }

    /// <summary>The Enable register's shadow: true while the host scans out and consumes the FIFO.</summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Writes the Enable register. Disabling stops the host consuming the
    /// FIFO, so commands written afterwards sit inert in FIFO memory for
    /// inspection; enabling requires a programmed mode and is ignored
    /// before the first successful mode set.
    /// </summary>
    /// <param name="enabled">True to enable the scanout, false to disable it.</param>
    void SetEnabled(bool enabled);

    /// <summary>The FIFO MIN register: the byte offset of the first command slot.</summary>
    uint FifoMin { get; }

    /// <summary>The FIFO MAX register: the byte offset past the last command slot.</summary>
    uint FifoMax { get; }

    /// <summary>The FIFO NEXT_CMD register: the byte offset the next command is written at. Setting it rewinds, discarding what was written since.</summary>
    uint NextCommand { get; set; }

    /// <summary>Reads a dword of FIFO memory.</summary>
    /// <param name="byteOffset">The offset from the start of FIFO memory, a multiple of 4.</param>
    /// <returns>The dword at that offset.</returns>
    uint ReadFifo(uint byteOffset);

    /// <summary>
    /// Creates the SVGA3D canvas over <paramref name="display"/> regardless
    /// of the negotiation, so the command layer's wire format can be
    /// inspected on a host without 3D. Neither re-enables the adapter nor
    /// re-runs the FIFO initialisation; the canvas takes the mode the
    /// registers hold.
    /// </summary>
    /// <param name="display">The display this adapter publishes.</param>
    /// <returns>The 3D canvas, which draws on <paramref name="display"/>.</returns>
    Canvas3D CreateCanvas3D(DisplayDevice display);
}
