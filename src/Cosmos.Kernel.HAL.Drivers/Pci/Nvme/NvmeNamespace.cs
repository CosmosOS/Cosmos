// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Nvme;

/// <summary>
/// One NVMe namespace as a disk, named <c>nvme</c> + the controller's
/// number + <c>n</c> + the namespace ID, such as <c>nvme0n1</c>. Reads and
/// writes are issued one logical block at a time through the controller's
/// slots, so several namespaces, or several threads on one namespace, run
/// in parallel up to the controller's queue depth minus one.
///
/// <para>Error contract: a command the controller completes with an error
/// status, or never completes, throws <see cref="IOException"/>, so
/// <see cref="ReadBlock"/> never hands back a bounce page the command did
/// not fill.</para>
/// </summary>
internal sealed class NvmeNamespace : IBlockDevice
{
    private readonly NvmeController _controller;
    private readonly uint _namespaceId;

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public ulong BlockCount { get; }

    /// <inheritdoc />
    public ulong BlockSize { get; }

    /// <summary>Creates namespace <paramref name="namespaceId"/> of <paramref name="controller"/>, as its Identify Namespace data describes it.</summary>
    internal NvmeNamespace(NvmeController controller, uint namespaceId, ulong blockCount, ulong blockSize)
    {
        _controller = controller;
        _namespaceId = namespaceId;
        Name = $"nvme{controller.Index}n{namespaceId}";
        BlockCount = blockCount;
        BlockSize = blockSize;
    }

    /// <inheritdoc />
    public void ReadBlock(ulong blockNo, ulong blockCount, Span<byte> data)
    {
        int sector = (int)BlockSize;

        // Overflow-free guard (divide form): `(int)i * sector` wrapped for
        // >= 4M-block counts and could land back in range, silently copying
        // at wrong offsets instead of failing fast.
        ArgumentOutOfRangeException.ThrowIfGreaterThan(blockCount, (ulong)data.Length / (uint)sector, nameof(blockCount));

        for (ulong i = 0; i < blockCount; i++)
        {
            ulong lba = blockNo + i;
            uint status = _controller.Read(_namespaceId, lba, data.Slice((int)((long)i * sector), sector));
            ThrowIfFailed(status, "read", lba);
        }
    }

    /// <inheritdoc />
    public void WriteBlock(ulong blockNo, ulong blockCount, ReadOnlySpan<byte> data)
    {
        int sector = (int)BlockSize;
        ArgumentOutOfRangeException.ThrowIfGreaterThan(blockCount, (ulong)data.Length / (uint)sector, nameof(blockCount));

        for (ulong i = 0; i < blockCount; i++)
        {
            ulong lba = blockNo + i;
            uint status = _controller.Write(_namespaceId, lba, data.Slice((int)((long)i * sector), sector));
            ThrowIfFailed(status, "write", lba);
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// Issues Flush so completed writes reach stable media. WriteBlock does
    /// not flush per write: durability is the caller's policy, matching the
    /// shared IBlockDevice contract (and AHCI's behavior).
    /// </remarks>
    public void Flush()
    {
        uint status = _controller.Flush(_namespaceId);
        if (status != 0)
        {
            _controller.Context.WriteLog($"flush of {Name} failed, status 0x{status:X}");
            throw new IOException($"NVMe: flush of {Name} failed, status 0x{status:X}.");
        }
    }

    /// <summary>Logs and throws when a command on <paramref name="lba"/> completed with an error status.</summary>
    private void ThrowIfFailed(uint status, string operation, ulong lba)
    {
        if (status == 0)
        {
            return;
        }

        _controller.Context.WriteLog($"{operation} of {Name} failed at LBA {lba}, status 0x{status:X}");
        throw new IOException($"NVMe: {operation} of {Name} failed at LBA {lba}, status 0x{status:X}.");
    }
}
