// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.IO;

namespace Cosmos.Kernel.HAL.Devices.Storage;

/// <summary>
/// One NVMe namespace exposed as an <see cref="HAL.Interfaces.Devices.IBlockDevice"/>.
/// Reads and writes are issued one LBA at a time through the parent
/// controller's per-slot bounce buffers — multiple namespaces (or
/// concurrent callers on the same namespace) execute in parallel up to
/// the controller's I/O queue depth minus one (the NVMe queue-full rule).
/// </summary>
internal unsafe class NvmeNamespace : BlockDevice
{
    private readonly NvmeController _controller;
    private readonly uint _nsid;
    private readonly string _name;

    /// <inheritdoc />
    public override string Name => _name;

    public NvmeNamespace(NvmeController controller, uint nsid, ulong blockCount, ulong blockSize)
    {
        _controller = controller;
        _nsid = nsid;
        // Unique per device instance ("nvme0n1" style) so multi-controller /
        // multi-namespace systems get distinguishable device and partition
        // names. Built via BuildDeviceName: CoreLib int formatting crashes
        // this early in boot.
        _name = BuildDeviceName("nvme", (uint)controller.Index, "n", nsid);
        BlockCount = blockCount;
        BlockSize = blockSize;
    }

    /// <inheritdoc />
    public override void ReadBlock(ulong blockNo, ulong blockCount, Span<byte> data)
    {
        int sector = (int)BlockSize;
        // Overflow-free guard (divide form): `(int)i * sector` wrapped for
        // >= 4M-block counts and could land back in range, silently copying
        // at wrong offsets instead of failing fast.
        if (blockCount > (ulong)data.Length / (uint)sector)
        {
            throw new ArgumentOutOfRangeException(nameof(blockCount), "Span shorter than the requested transfer.");
        }

        for (ulong i = 0; i < blockCount; i++)
        {
            Span<byte> dst = data.Slice((int)((long)i * sector), sector);
            uint sc = _controller.Read(_nsid, blockNo + i, dst, numLogicalBlocksMinusOne: 0);
            if (sc != 0)
            {
                Serial.WriteString("[NVMe] Read failed lba=");
                Serial.WriteNumber(blockNo + i);
                Serial.WriteString(" status=0x");
                Serial.WriteHex(sc);
                Serial.WriteString("\n");
                throw new Exception("NVMe Read error");
            }
        }
    }

    /// <inheritdoc />
    public override void WriteBlock(ulong blockNo, ulong blockCount, ReadOnlySpan<byte> data)
    {
        int sector = (int)BlockSize;
        if (blockCount > (ulong)data.Length / (uint)sector)
        {
            throw new ArgumentOutOfRangeException(nameof(blockCount), "Span shorter than the requested transfer.");
        }

        for (ulong i = 0; i < blockCount; i++)
        {
            ReadOnlySpan<byte> src = data.Slice((int)((long)i * sector), sector);
            uint sc = _controller.Write(_nsid, blockNo + i, src, numLogicalBlocksMinusOne: 0);
            if (sc != 0)
            {
                Serial.WriteString("[NVMe] Write failed lba=");
                Serial.WriteNumber(blockNo + i);
                Serial.WriteString(" status=0x");
                Serial.WriteHex(sc);
                Serial.WriteString("\n");
                throw new Exception("NVMe Write error");
            }
        }
    }

    /// <inheritdoc />
    public override void Flush()
    {
        uint sc = _controller.Flush(_nsid);
        if (sc != 0)
        {
            Serial.WriteString("[NVMe] Flush failed status=0x");
            Serial.WriteHex(sc);
            Serial.WriteString("\n");
            throw new Exception("NVMe Flush error");
        }
    }
}
