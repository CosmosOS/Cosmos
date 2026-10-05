// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Firmware;

/// <summary>
/// The constants of the flattened device tree format (the devicetree
/// specification's FDT, version 17): the header layout, the structure
/// block's tokens, the cell sizes, the defaults a node inherits when its
/// parent names none, and the GIC's three-cell interrupt form the virt
/// machine uses. Every value in the blob is big-endian.
/// </summary>
internal static class DeviceTreeFormat
{
    /// <summary>The magic the blob starts with.</summary>
    internal const uint Magic = 0xd00dfeedu;

    /// <summary>Bytes of the version 17 header: ten big-endian cells.</summary>
    internal const int HeaderBytes = 40;

    /// <summary>Header offset of the magic.</summary>
    internal const uint MagicOffset = 0;

    /// <summary>Header offset of the blob's total size in bytes.</summary>
    internal const uint TotalSizeOffset = 4;

    /// <summary>Header offset of the structure block's offset.</summary>
    internal const uint StructOffset = 8;

    /// <summary>Header offset of the strings block's offset.</summary>
    internal const uint StringsOffset = 12;

    /// <summary>Header offset of the memory reservation block's offset.</summary>
    internal const uint MemoryReservationOffset = 16;

    /// <summary>Header offset of the format version.</summary>
    internal const uint VersionOffset = 20;

    /// <summary>Header offset of the oldest version a reader may implement.</summary>
    internal const uint LastCompatibleVersionOffset = 24;

    /// <summary>Header offset of the strings block's size in bytes.</summary>
    internal const uint StringsSizeOffset = 32;

    /// <summary>Header offset of the structure block's size in bytes.</summary>
    internal const uint StructSizeOffset = 36;

    /// <summary>The oldest format version the parser reads: the first whose header carries the structure block's size.</summary>
    internal const uint MinimumVersion = 17;

    /// <summary>The newest last compatible version the parser accepts: a blob that demands a newer reader is refused.</summary>
    internal const uint MaximumLastCompatibleVersion = 17;

    /// <summary>The token that opens a node; its NUL-terminated name follows, padded to a cell.</summary>
    internal const uint TokenBeginNode = 1;

    /// <summary>The token that closes a node.</summary>
    internal const uint TokenEndNode = 2;

    /// <summary>The token that starts a property record: length, name offset, value padded to a cell.</summary>
    internal const uint TokenProperty = 3;

    /// <summary>The token a reader skips.</summary>
    internal const uint TokenNop = 4;

    /// <summary>The token that ends the structure block.</summary>
    internal const uint TokenEnd = 9;

    /// <summary>Bytes of one token.</summary>
    internal const int TokenBytes = 4;

    /// <summary>Bytes of a property record's header: the value length and the name offset.</summary>
    internal const int PropertyHeaderBytes = 8;

    /// <summary>Bytes of one cell.</summary>
    internal const int CellBytes = 4;

    /// <summary>The address cells a node's children use when it names no <c>#address-cells</c>.</summary>
    internal const uint DefaultAddressCells = 2;

    /// <summary>The size cells a node's children use when it names no <c>#size-cells</c>.</summary>
    internal const uint DefaultSizeCells = 1;

    /// <summary>The deepest nesting the phandle scan follows: the bound of its stack of open nodes.</summary>
    internal const int MaxDepth = 32;

    /// <summary>Cells of one GIC interrupt specifier: type, number, flags.</summary>
    internal const uint GicInterruptCells = 3;

    /// <summary>The GIC interrupt type of a shared peripheral interrupt.</summary>
    internal const uint GicSpiType = 0;

    /// <summary>The INTID of SPI 0: a specifier's number plus this is the line.</summary>
    internal const uint GicSpiBase = 32;

    /// <summary>Rounds a byte offset or length up to the next cell boundary. Any context; allocation-free.</summary>
    /// <param name="value">The offset or length.</param>
    internal static uint AlignUp(uint value) => (value + 3u) & ~3u;

    /// <summary>
    /// Rounds a length read from the blob up to the next cell boundary in
    /// 64 bits, so a property length within three of 2^32 does not wrap to
    /// 0 and the record it ends lands past the structure block. Any
    /// context; allocation-free.
    /// </summary>
    /// <param name="value">The length.</param>
    internal static ulong AlignUp(ulong value) => (value + 3UL) & ~3UL;
}
