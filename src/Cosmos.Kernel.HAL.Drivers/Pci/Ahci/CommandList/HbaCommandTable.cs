// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Ahci.CommandList;

/// <summary>
/// One command table in the command region: the command FIS at its start,
/// then its PRDT at 0x80 (AHCI 1.3.1 s4.2.3). The HBA reaches it through
/// the CTBA written into the owning <see cref="HbaCommandHeader"/>.
/// Creating one clears the FIS, ATAPI and reserved areas and each PRDT
/// entry it views.
/// </summary>
internal sealed class HbaCommandTable
{
    /// <summary>Offset of the PRDT within the command table; the CFIS, ACMD and reserved areas occupy the first 0x80 bytes (AHCI spec 4.2.3).</summary>
    private const int PrdtOffsetBytes = 0x80;

    private readonly HbaPrdtEntry[] _prdtEntries;

    /// <summary>Where the command FIS starts in the command region: the table's own start.</summary>
    internal int CommandFisOffset { get; }

    /// <summary>
    /// Clears the command table at <paramref name="offset"/> in
    /// <paramref name="memory"/>, and its first
    /// <paramref name="prdtCount"/> PRDT entries, and views them.
    /// </summary>
    /// <param name="memory">The controller's command region.</param>
    /// <param name="offset">Where the command table starts in it.</param>
    /// <param name="prdtCount">PRDT entries the command uses.</param>
    internal HbaCommandTable(DmaBuffer memory, int offset, uint prdtCount)
    {
        CommandFisOffset = offset;

        memory.Span.Slice(offset, PrdtOffsetBytes).Clear();

        _prdtEntries = new HbaPrdtEntry[prdtCount];
        for (uint i = 0; i < prdtCount; i++)
        {
            _prdtEntries[i] = new HbaPrdtEntry(memory, offset + PrdtOffsetBytes, i);
        }
    }

    /// <summary>PRDT entry <paramref name="index"/>, one of those the table was created with.</summary>
    internal HbaPrdtEntry GetPrdtEntry(int index) => _prdtEntries[index];
}
