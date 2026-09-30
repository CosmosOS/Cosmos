// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Devices;

/// <summary>
/// Bits per sample in one channel. The value is the channel's size in
/// bytes, so <c>(int)depth</c> is what a stride calculation needs.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public enum AudioBitDepth : byte
{
    /// <summary>8 bits per channel, one byte. Unsigned by convention.</summary>
    Bits8 = 1,

    /// <summary>16 bits per channel, two bytes. The format every output supports.</summary>
    Bits16 = 2,

    /// <summary>24 bits per channel, three bytes, packed.</summary>
    Bits24 = 3,

    /// <summary>32 bits per channel, four bytes.</summary>
    Bits32 = 4,
}
