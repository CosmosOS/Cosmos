// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Devices;

namespace Cosmos.Kernel.Drivers.Pci.Storage.Nvme;

/// <summary>
/// One namespace of an NVMe controller, the block device
/// <see cref="NvmeDriver"/> publishes to the ring: <c>nvme{i}n{nsid}</c>,
/// with the geometry Identify Namespace reported. Reads and writes go one
/// logical block per command through a slot of the controller's
/// <see cref="NvmeState"/>, so concurrent callers on the same or another
/// namespace of the controller run in parallel up to its slot count. Every
/// method is thread context, entered by the ring from any thread; a failed
/// command surfaces as an <see cref="IOException"/>, as the contract asks.
/// </summary>
public sealed class NvmeNamespace : IBlockDevice
{
    // --- Private fields ---

    private readonly NvmeState _controller;
    private readonly uint _namespaceId;
    private readonly ulong _blockCount;
    private readonly ulong _blockSize;
    private string? _name;

    // --- Constructor ---

    /// <summary>
    /// Takes the controller and the geometry the probe read from Identify
    /// Namespace. The name is built on its first read, after the probe has
    /// assigned the controller's index at publication. Thread context, from
    /// the probe.
    /// </summary>
    /// <param name="controller">The controller the namespace belongs to.</param>
    /// <param name="namespaceId">The NSID.</param>
    /// <param name="blockCount">NSZE: the namespace's size in logical blocks.</param>
    /// <param name="blockSize">The logical block size in bytes, from the active LBA format.</param>
    internal NvmeNamespace(NvmeState controller, uint namespaceId, ulong blockCount, ulong blockSize)
    {
        _controller = controller;
        _namespaceId = namespaceId;
        _blockCount = blockCount;
        _blockSize = blockSize;
    }

    // --- IBlockDevice ---

    /// <inheritdoc/>
    public ulong BlockCount => _blockCount;

    /// <inheritdoc/>
    public ulong BlockSize => _blockSize;

    /// <summary><c>nvme{controller index}n{nsid}</c>, built once on the first read. Any context after publication.</summary>
    public string Name => _name ??= "nvme" + _controller.Index + "n" + _namespaceId;

    /// <summary>
    /// Reads whole blocks, one command per block through a slot's bounce
    /// page. Thread context; any thread.
    /// </summary>
    /// <param name="blockNo">The first block.</param>
    /// <param name="blockCount">How many blocks.</param>
    /// <param name="data">Where they go; at least <paramref name="blockCount"/> times <see cref="BlockSize"/> bytes.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="data"/> is shorter than the blocks asked for.</exception>
    /// <exception cref="IOException">A command failed, timed out, or the device is being detached.</exception>
    public void ReadBlock(ulong blockNo, ulong blockCount, Span<byte> data)
    {
        int blockSize = (int)_blockSize;
        ArgumentOutOfRangeException.ThrowIfGreaterThan(blockCount, (ulong)data.Length / (uint)blockSize, nameof(blockCount));
        for (ulong i = 0; i < blockCount; i++)
        {
            ulong lba = blockNo + i;
            Span<byte> block = data.Slice((int)((long)i * blockSize), blockSize);
            ushort status = _controller.Read(_namespaceId, lba, block);
            if (status != 0)
            {
                _controller.Log($"read failed lba={lba} status=0x{status:X}");
                throw new IOException("NVMe Read error");
            }
        }
    }

    /// <summary>
    /// Writes whole blocks, one command per block through a slot's bounce
    /// page, the rest of the page zeroed. Thread context; any thread.
    /// </summary>
    /// <param name="blockNo">The first block.</param>
    /// <param name="blockCount">How many blocks.</param>
    /// <param name="data">Their bytes; at least <paramref name="blockCount"/> times <see cref="BlockSize"/> bytes.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="data"/> is shorter than the blocks given.</exception>
    /// <exception cref="IOException">A command failed, timed out, or the device is being detached.</exception>
    public void WriteBlock(ulong blockNo, ulong blockCount, ReadOnlySpan<byte> data)
    {
        int blockSize = (int)_blockSize;
        ArgumentOutOfRangeException.ThrowIfGreaterThan(blockCount, (ulong)data.Length / (uint)blockSize, nameof(blockCount));
        for (ulong i = 0; i < blockCount; i++)
        {
            ulong lba = blockNo + i;
            ReadOnlySpan<byte> block = data.Slice((int)((long)i * blockSize), blockSize);
            ushort status = _controller.Write(_namespaceId, lba, block);
            if (status != 0)
            {
                _controller.Log($"write failed lba={lba} status=0x{status:X}");
                throw new IOException("NVMe Write error");
            }
        }
    }

    /// <summary>Sends the Flush command for the namespace. Thread context; any thread.</summary>
    /// <exception cref="IOException">The command failed, timed out, or the device is being detached.</exception>
    public void Flush()
    {
        ushort status = _controller.Flush(_namespaceId);
        if (status != 0)
        {
            throw new IOException("NVMe Flush error");
        }
    }

    // --- Properties ---

    /// <summary>The NSID. Any context.</summary>
    public uint NamespaceId => _namespaceId;

    /// <summary>The controller the namespace belongs to. Any context.</summary>
    public NvmeState Controller => _controller;
}
