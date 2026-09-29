// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Virtio;

/// <summary>
/// The device ids of virtio specification section 5, as a transport reads
/// them from the function's device id (PCI) or the DeviceID register
/// (MMIO). The list is not exhaustive: a value outside it is a type the kit
/// has no name for, and <see cref="VirtioIdentity.Describe"/> prints the
/// number. Any context.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public enum VirtioDeviceType : uint
{
    /// <summary>A network card (virtio-net).</summary>
    Network = 1,

    /// <summary>A block device (virtio-blk).</summary>
    Block = 2,

    /// <summary>A console.</summary>
    Console = 3,

    /// <summary>An entropy source (virtio-rng).</summary>
    Entropy = 4,

    /// <summary>A memory balloon.</summary>
    Balloon = 5,

    /// <summary>A SCSI host.</summary>
    Scsi = 8,

    /// <summary>A GPU (virtio-gpu).</summary>
    Gpu = 16,

    /// <summary>An input device: a keyboard, a mouse or a tablet (virtio-input).</summary>
    Input = 18,

    /// <summary>A socket device (vsock).</summary>
    Socket = 19,

    /// <summary>A crypto device.</summary>
    Crypto = 20,

    /// <summary>A sound device.</summary>
    Sound = 25,

    /// <summary>A file system (virtio-fs).</summary>
    FileSystem = 26,
}
