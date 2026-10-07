// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers.Pci.Display.VmwareSvga.Structs;

/// <summary>SVGAGuestPtr: a guest memory region id and a byte offset into it, 8 bytes; SVGA_GMR_FRAMEBUFFER addresses VRAM.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGAGuestPtr
{
    /// <summary>The guest memory region; SVGA_GMR_FRAMEBUFFER for VRAM.</summary>
    public uint gmrId;
    /// <summary>Byte offset within the region.</summary>
    public uint offset;
}
