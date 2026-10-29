// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.Devices.Storage;

namespace Cosmos.Kernel.System.FileSystem.Iso9660;

/// <summary>
/// Per-mount ISO 9660 volume: the primary volume descriptor's geometry, the
/// root directory, whether the volume carries Rock Ridge, and the one place
/// bytes are read through. Every number from the medium is untrusted: a
/// range that leaves the volume is refused, never clamped, so a corrupt
/// extent cannot read the device past it.
/// </summary>
internal sealed class Iso9660Superblock : IVfsSuperblock
{
    /// <summary>Largest device block the bounce buffer is sized for.</summary>
    private const ulong MaxDeviceBlockSize = 1024 * 1024;

    /// <summary>One device block, for the partial blocks at either end of a read.</summary>
    private readonly byte[] _bounce;

    /// <summary>The block device holding the volume.</summary>
    public IBlockDevice Device { get; }

    /// <summary>Logical block size the volume is addressed in: 512, 1024 or 2048 bytes.</summary>
    public uint BlockSize { get; }

    /// <summary>Volume space size in logical blocks.</summary>
    public uint VolumeBlocks { get; }

    /// <summary>Volume size in bytes: the bound every read is checked against.</summary>
    public ulong VolumeBytes => (ulong)VolumeBlocks * BlockSize;

    /// <summary>The volume identifier the descriptor carries, trailing spaces trimmed.</summary>
    public string VolumeIdentifier { get; }

    /// <summary>True when the root directory's own record carries the SUSP indicator: names and attributes come from Rock Ridge.</summary>
    public bool HasRockRidge => RockRidgeParser is not null;

    /// <summary>The Rock Ridge parser for this volume, or null without Rock Ridge.</summary>
    internal RockRidgeParser? RockRidgeParser { get; }

    /// <summary>Directory and metadata operations for this volume.</summary>
    public Iso9660InodeOperations InodeOps { get; }

    /// <summary>Open-file byte I/O for this volume.</summary>
    public Iso9660FileOperations FileOps { get; }

    /// <summary>Superblock-level callbacks for this volume.</summary>
    public Iso9660SuperblockOperations SuperOps { get; }

    /// <summary>The root directory.</summary>
    public Iso9660Inode RootInode { get; }

    /// <inheritdoc />
    public IVfsInode Root => RootInode;

    /// <inheritdoc />
    public ISuperblockOperations SuperOperations => SuperOps;

    long IVfsSuperblock.BlockSize => BlockSize;

    /// <inheritdoc />
    public ulong MaxNameLength => Iso9660Layout.MaxNameLength;

    /// <inheritdoc />
    public bool IsReadOnly => true;

    private Iso9660Superblock(IBlockDevice device, uint blockSize, uint volumeBlocks, string volumeIdentifier, ReadOnlySpan<byte> rootRecord)
    {
        Device = device;
        BlockSize = blockSize;
        VolumeBlocks = volumeBlocks;
        VolumeIdentifier = volumeIdentifier;
        _bounce = new byte[device.BlockSize];
        InodeOps = new Iso9660InodeOperations(this);
        FileOps = new Iso9660FileOperations(this);
        SuperOps = new Iso9660SuperblockOperations();

        uint rootExtent = BitConverter.ToUInt32(rootRecord.Slice(Iso9660Layout.RecordExtentOffset, 4));
        uint rootLength = BitConverter.ToUInt32(rootRecord.Slice(Iso9660Layout.RecordDataLengthOffset, 4));
        long rootTime = Iso9660Date.FromRecordingDate(rootRecord.Slice(Iso9660Layout.RecordDateOffset, Iso9660Layout.ShortDateLength));

        // The root's own record, first in its extent, is where the SUSP
        // indicator lives, and where Rock Ridge put the root's attributes.
        RockRidgeAttributes rootAttributes = default;
        int headerLength = (int)Math.Min(rootLength, blockSize);
        if (TryReadRange((ulong)rootExtent * blockSize, headerLength, out byte[]? header)
            && Iso9660DirectoryReader.TryGetSystemUseArea(header, 0, out ReadOnlySpan<byte> systemUse)
            && RockRidgeParser.HasSuspIndicator(systemUse, out int skip))
        {
            RockRidgeParser = new RockRidgeParser(this, skip);
            RockRidgeParser.Parse(systemUse, out rootAttributes, out _);
        }

        RootInode = new Iso9660Inode(
            this,
            parent: null,
            inodeNumber: (ulong)Iso9660Layout.FirstDescriptorOffset + Iso9660Layout.RootDirectoryRecordOffset,
            name: "/",
            extent: rootExtent,
            dataLength: rootLength,
            isDirectory: true,
            recordingTime: rootTime,
            rockRidge: rootAttributes);
    }

    /// <summary>
    /// Reads the volume descriptors of <paramref name="device"/> and builds
    /// the superblock when a primary volume descriptor describes a volume
    /// the device holds.
    /// </summary>
    /// <param name="device">The block device holding the volume.</param>
    /// <param name="superblock">The mounted volume on success.</param>
    /// <returns>false when no primary descriptor is found, its geometry is not one the driver reads or leaves the device, or the device fails while being read.</returns>
    public static bool TryCreate(IBlockDevice device, [NotNullWhen(true)] out Iso9660Superblock? superblock)
    {
        superblock = null;
        try
        {
            ulong deviceBlockSize = device.BlockSize;
            if (deviceBlockSize == 0 || deviceBlockSize > MaxDeviceBlockSize)
            {
                return false;
            }

            ulong deviceBytes = device.BlockCount * deviceBlockSize;
            byte[] bounce = new byte[deviceBlockSize];
            byte[] descriptor = new byte[Iso9660Layout.DescriptorSize];
            bool found = false;
            bool ended = false;
            for (int i = 0; i < Iso9660Layout.MaxDescriptors && !found && !ended; i++)
            {
                ulong offset = (ulong)Iso9660Layout.FirstDescriptorOffset + ((ulong)i * Iso9660Layout.DescriptorSize);
                if (offset + Iso9660Layout.DescriptorSize > deviceBytes)
                {
                    ended = true;
                    continue;
                }

                ReadBytes(device, bounce, offset, descriptor);
                if (!HasStandardIdentifier(descriptor))
                {
                    ended = true;
                    continue;
                }

                byte type = descriptor[Iso9660Layout.DescriptorTypeOffset];
                if (type == Iso9660Layout.TerminatorDescriptorType)
                {
                    ended = true;
                }
                else if (type == Iso9660Layout.PrimaryDescriptorType
                    && descriptor[Iso9660Layout.DescriptorVersionOffset] == Iso9660Layout.PrimaryDescriptorVersion)
                {
                    found = true;
                }
            }

            if (!found)
            {
                return false;
            }

            uint blockSize = BitConverter.ToUInt16(descriptor.AsSpan(Iso9660Layout.LogicalBlockSizeOffset, 2));
            if (blockSize is not (512 or 1024 or 2048))
            {
                return false;
            }

            uint volumeBlocks = BitConverter.ToUInt32(descriptor.AsSpan(Iso9660Layout.VolumeSpaceSizeOffset, 4));
            ulong volumeBytes = (ulong)volumeBlocks * blockSize;
            if (volumeBlocks == 0 || volumeBytes > deviceBytes)
            {
                return false;
            }

            ReadOnlySpan<byte> rootRecord = descriptor.AsSpan(Iso9660Layout.RootDirectoryRecordOffset, Iso9660Layout.RootDirectoryRecordLength);
            if (rootRecord[Iso9660Layout.RecordLengthOffset] < Iso9660Layout.MinRecordLength
                || (rootRecord[Iso9660Layout.RecordFlagsOffset] & Iso9660Layout.FlagDirectory) == 0)
            {
                return false;
            }

            uint rootExtent = BitConverter.ToUInt32(rootRecord.Slice(Iso9660Layout.RecordExtentOffset, 4));
            uint rootLength = BitConverter.ToUInt32(rootRecord.Slice(Iso9660Layout.RecordDataLengthOffset, 4));
            if (rootLength < Iso9660Layout.MinRecordLength
                || rootLength > Iso9660Layout.MaxDirectoryLength
                || ((ulong)rootExtent * blockSize) + rootLength > volumeBytes)
            {
                return false;
            }

            string volumeIdentifier = ReadTrimmed(descriptor.AsSpan(Iso9660Layout.VolumeIdentifierOffset, Iso9660Layout.VolumeIdentifierLength));
            superblock = new Iso9660Superblock(device, blockSize, volumeBlocks, volumeIdentifier, rootRecord);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Reads <paramref name="destination"/>.Length bytes at a byte offset on
    /// the volume: a partial device block at either end goes through the
    /// bounce buffer, the whole blocks between them straight into the
    /// caller's span in one device read.
    /// </summary>
    /// <param name="volumeOffset">Byte offset on the volume.</param>
    /// <param name="destination">Where the bytes go; its length is the byte count.</param>
    /// <exception cref="ArgumentOutOfRangeException">The range leaves the volume.</exception>
    internal void ReadBytes(ulong volumeOffset, Span<byte> destination)
    {
        if (destination.Length == 0)
        {
            return;
        }

        ulong end = volumeOffset + (ulong)destination.Length;
        if (end < volumeOffset || end > VolumeBytes)
        {
            throw new ArgumentOutOfRangeException(nameof(volumeOffset));
        }

        ReadBytes(Device, _bounce, volumeOffset, destination);
    }

    /// <summary>
    /// Reads a byte range into a fresh array, or reports false when the range
    /// leaves the volume: the form the parsers use on offsets they read from
    /// the medium.
    /// </summary>
    /// <param name="volumeOffset">Byte offset on the volume.</param>
    /// <param name="length">Bytes to read.</param>
    /// <param name="bytes">The bytes on success.</param>
    internal bool TryReadRange(ulong volumeOffset, int length, [NotNullWhen(true)] out byte[]? bytes)
    {
        bytes = null;
        if (length <= 0)
        {
            return false;
        }

        ulong end = volumeOffset + (ulong)length;
        if (end < volumeOffset || end > VolumeBytes)
        {
            return false;
        }

        bytes = new byte[length];
        ReadBytes(volumeOffset, bytes);
        return true;
    }

    /// <summary>
    /// Forgets the directory listings parsed so far: the root's children and,
    /// through them, every other directory's.
    /// </summary>
    internal void Drop()
    {
        RootInode.Children = null;
    }

    private static void ReadBytes(IBlockDevice device, byte[] bounce, ulong volumeOffset, Span<byte> destination)
    {
        ulong deviceBlockSize = device.BlockSize;
        ulong offset = volumeOffset;
        int copied = 0;
        int remaining = destination.Length;

        ulong headOffset = offset % deviceBlockSize;
        if (headOffset != 0)
        {
            device.ReadBlock(offset / deviceBlockSize, 1, bounce);
            int chunk = (int)Math.Min((ulong)remaining, deviceBlockSize - headOffset);
            bounce.AsSpan((int)headOffset, chunk).CopyTo(destination);
            copied += chunk;
            offset += (ulong)chunk;
            remaining -= chunk;
        }

        ulong wholeBlocks = (ulong)remaining / deviceBlockSize;
        if (wholeBlocks > 0)
        {
            int bytes = (int)(wholeBlocks * deviceBlockSize);
            device.ReadBlock(offset / deviceBlockSize, wholeBlocks, destination.Slice(copied, bytes));
            copied += bytes;
            offset += (ulong)bytes;
            remaining -= bytes;
        }

        if (remaining > 0)
        {
            device.ReadBlock(offset / deviceBlockSize, 1, bounce);
            bounce.AsSpan(0, remaining).CopyTo(destination.Slice(copied, remaining));
        }
    }

    private static bool HasStandardIdentifier(ReadOnlySpan<byte> descriptor)
    {
        ReadOnlySpan<byte> identifier = descriptor.Slice(Iso9660Layout.StandardIdentifierOffset, Iso9660Layout.StandardIdentifier.Length);
        for (int i = 0; i < identifier.Length; i++)
        {
            if (identifier[i] != (byte)Iso9660Layout.StandardIdentifier[i])
            {
                return false;
            }
        }

        return true;
    }

    private static string ReadTrimmed(ReadOnlySpan<byte> field)
    {
        int length = field.Length;
        while (length > 0 && (field[length - 1] == (byte)' ' || field[length - 1] == 0))
        {
            length--;
        }

        Span<char> chars = stackalloc char[length];
        for (int i = 0; i < length; i++)
        {
            chars[i] = (char)field[i];
        }

        return new string(chars);
    }
}
