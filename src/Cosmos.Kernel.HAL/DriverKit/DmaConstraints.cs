// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit;

/// <summary>
/// What a device can address, for <see cref="DeviceBinding.AllocateDma"/>.
/// The default asks for nothing beyond the alignment.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public readonly struct DmaConstraints
{
    private DmaConstraints(bool below4GiB)
    {
        Below4GiB = below4GiB;
    }

    /// <summary>The buffer must end below 4 GiB, for a device that takes 32-bit addresses.</summary>
    public bool Below4GiB { get; }

    /// <summary>No constraint beyond the alignment asked for.</summary>
    public static DmaConstraints None => default;

    /// <summary>A 32-bit device: the whole buffer must sit below 4 GiB.</summary>
    public static DmaConstraints Addressable32Bit => new(below4GiB: true);
}
