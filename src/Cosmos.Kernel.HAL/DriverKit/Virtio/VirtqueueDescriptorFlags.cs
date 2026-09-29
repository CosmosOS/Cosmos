// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Virtio;

/// <summary>
/// The flags of a split virtqueue descriptor (VIRTQ_DESC_F_*, virtio
/// specification section 2.6.5). Any context.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
[Flags]
public enum VirtqueueDescriptorFlags : ushort
{
    /// <summary>A device-readable buffer that ends the chain.</summary>
    None = 0,

    /// <summary>The chain continues at the descriptor named by the next field (VIRTQ_DESC_F_NEXT).</summary>
    Next = 1,

    /// <summary>The device writes the buffer rather than reads it (VIRTQ_DESC_F_WRITE).</summary>
    Write = 2,

    /// <summary>The buffer holds a table of descriptors (VIRTQ_DESC_F_INDIRECT).</summary>
    Indirect = 4,
}
