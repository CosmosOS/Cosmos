// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Buffers.Binary;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.MassStorage.BulkOnly;

/// <summary>
/// The Command Status Wrapper that ends a command, read from the bulk IN
/// endpoint (BOT 1.0 s5.2): 13 bytes, little-endian. Its fields mean
/// something only once <see cref="IsValidFor"/> said so.
/// </summary>
internal readonly ref struct CommandStatusWrapper
{
    /// <summary>Size of a wrapper in bytes.</summary>
    internal const int Size = 13;

    /// <summary>bCSWStatus: the command passed.</summary>
    internal const byte CommandPassed = 0x00;

    /// <summary>bCSWStatus: the command failed; REQUEST SENSE says why.</summary>
    internal const byte CommandFailed = 0x01;

    /// <summary>dCSWSignature: "USBS" in little-endian order.</summary>
    private const uint Signature = 0x53425355;

    private const int TagOffset = 4;
    private const int DataResidueOffset = 8;
    private const int StatusOffset = 12;

    private readonly ReadOnlySpan<byte> _wrapper;

    /// <summary>dCSWDataResidue: bytes of the data stage the device did not move.</summary>
    internal uint DataResidue => BinaryPrimitives.ReadUInt32LittleEndian(_wrapper[DataResidueOffset..]);

    /// <summary>bCSWStatus: <see cref="CommandPassed"/>, <see cref="CommandFailed"/>, or a phase error.</summary>
    internal byte Status => _wrapper[StatusOffset];

    /// <summary>Views what arrived on the bulk IN endpoint: <paramref name="received"/> holds only the bytes the device sent.</summary>
    internal CommandStatusWrapper(ReadOnlySpan<byte> received)
    {
        _wrapper = received;
    }

    /// <summary>
    /// True when the wrapper is valid and answers the command tagged
    /// <paramref name="tag"/> (s6.3.1): all 13 bytes arrived, with the
    /// signature and that tag.
    /// </summary>
    internal bool IsValidFor(uint tag) =>
        _wrapper.Length == Size
        && BinaryPrimitives.ReadUInt32LittleEndian(_wrapper) == Signature
        && BinaryPrimitives.ReadUInt32LittleEndian(_wrapper[TagOffset..]) == tag;
}
