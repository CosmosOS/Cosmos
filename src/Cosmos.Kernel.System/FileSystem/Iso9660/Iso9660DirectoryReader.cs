// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.FileSystem.Iso9660;

/// <summary>
/// Walks a directory extent record by record (ECMA-119 9.1) and builds the
/// inodes of its entries. A record of length 0 is the padding that ends a
/// logical block; a record shorter than its fixed part or running past the
/// extent ends the walk with what it has. The two first records, the
/// directory itself and its parent, are never listed, nor are associated
/// files, the extents of a multi-extent file, or a directory Rock Ridge
/// relocated into <c>rr_moved</c>, which is reached through its placeholder.
/// </summary>
internal static class Iso9660DirectoryReader
{
    /// <summary>
    /// Parses the entries of <paramref name="directory"/>. An extent that
    /// leaves the volume lists as empty.
    /// </summary>
    /// <param name="superblock">The volume.</param>
    /// <param name="directory">The directory to list.</param>
    public static Iso9660Inode[] ReadChildren(Iso9660Superblock superblock, Iso9660Inode directory)
    {
        uint length = Math.Min(directory.DataLength, Iso9660Layout.MaxDirectoryLength);
        if (!superblock.TryReadRange((ulong)directory.Extent * superblock.BlockSize, (int)length, out byte[]? data))
        {
            return [];
        }

        List<Iso9660Inode> children = [];
        int blockSize = (int)superblock.BlockSize;
        int position = 0;
        bool ended = false;
        bool skipNextExtent = false;
        while (!ended && position < data.Length)
        {
            int recordLength = data[position + Iso9660Layout.RecordLengthOffset];
            if (recordLength == 0)
            {
                // The rest of this logical block is padding.
                position = ((position / blockSize) + 1) * blockSize;
                continue;
            }

            if (recordLength < Iso9660Layout.MinRecordLength || position + recordLength > data.Length)
            {
                ended = true;
                continue;
            }

            ReadOnlySpan<byte> record = data.AsSpan(position, recordLength);
            int recordPosition = position;
            position += recordLength;

            int nameLength = record[Iso9660Layout.RecordNameLengthOffset];
            if (nameLength == 0 || Iso9660Layout.RecordNameOffset + nameLength > recordLength)
            {
                continue;
            }

            byte flags = record[Iso9660Layout.RecordFlagsOffset];
            ReadOnlySpan<byte> identifier = record.Slice(Iso9660Layout.RecordNameOffset, nameLength);

            // A multi-extent file is the records flagged not-final and the
            // one after the last of them; none of them is presented.
            bool partOfMultiExtent = skipNextExtent;
            skipNextExtent = (flags & Iso9660Layout.FlagNotFinalExtent) != 0;
            if (partOfMultiExtent || skipNextExtent)
            {
                continue;
            }

            if ((flags & Iso9660Layout.FlagAssociated) != 0)
            {
                continue;
            }

            if (nameLength == 1 && (identifier[0] == Iso9660Layout.CurrentDirectoryIdentifier || identifier[0] == Iso9660Layout.ParentDirectoryIdentifier))
            {
                continue;
            }

            uint extent = BitConverter.ToUInt32(record.Slice(Iso9660Layout.RecordExtentOffset, 4));
            uint dataLength = BitConverter.ToUInt32(record.Slice(Iso9660Layout.RecordDataLengthOffset, 4));
            bool isDirectory = (flags & Iso9660Layout.FlagDirectory) != 0;

            RockRidgeAttributes rockRidge = default;
            string? rockRidgeName = null;
            if (superblock.RockRidgeParser is not null && TryGetSystemUseArea(record, 0, out ReadOnlySpan<byte> systemUse))
            {
                superblock.RockRidgeParser.Parse(systemUse, out rockRidge, out rockRidgeName);
            }

            if (rockRidge.IsRelocated)
            {
                continue;
            }

            if (rockRidge.IsRelocationPlaceholder)
            {
                // The placeholder's own extent is empty: the directory's
                // contents are where CL points, with their length in the
                // first record there.
                if (!TryReadDirectoryLength(superblock, rockRidge.RelocatedExtent, out dataLength))
                {
                    continue;
                }

                extent = rockRidge.RelocatedExtent;
                isDirectory = true;
            }

            if (((ulong)extent * superblock.BlockSize) + dataLength > superblock.VolumeBytes)
            {
                // A record whose extent leaves the volume is corrupt: it is
                // not listed, so no read can be asked of it.
                continue;
            }

            string name = rockRidgeName ?? PlainName(identifier);
            if (name.Length == 0)
            {
                continue;
            }

            long recordingTime = Iso9660Date.FromRecordingDate(record.Slice(Iso9660Layout.RecordDateOffset, Iso9660Layout.ShortDateLength));
            ulong inodeNumber = ((ulong)directory.Extent * superblock.BlockSize) + (ulong)recordPosition;
            children.Add(new Iso9660Inode(superblock, directory, inodeNumber, name, extent, dataLength, isDirectory, recordingTime, rockRidge));
        }

        return [.. children];
    }

    /// <summary>
    /// The system use area of the record at <paramref name="offset"/> in
    /// <paramref name="buffer"/>: what follows the identifier and its pad
    /// byte, up to the record's length. False when the record does not fit
    /// the buffer.
    /// </summary>
    /// <param name="buffer">Bytes holding the record.</param>
    /// <param name="offset">Where the record starts.</param>
    /// <param name="systemUse">The area; empty when the record has none.</param>
    public static bool TryGetSystemUseArea(ReadOnlySpan<byte> buffer, int offset, out ReadOnlySpan<byte> systemUse)
    {
        systemUse = [];
        if (offset + Iso9660Layout.MinRecordLength > buffer.Length)
        {
            return false;
        }

        int recordLength = buffer[offset + Iso9660Layout.RecordLengthOffset];
        int nameLength = buffer[offset + Iso9660Layout.RecordNameLengthOffset];
        if (recordLength < Iso9660Layout.MinRecordLength || offset + recordLength > buffer.Length)
        {
            return false;
        }

        // The identifier is padded to an even length so the area starts on
        // an even offset.
        int pad = nameLength % 2 == 0 ? 1 : 0;
        int start = Iso9660Layout.RecordNameOffset + nameLength + pad;
        if (start > recordLength)
        {
            return false;
        }

        systemUse = buffer.Slice(offset + start, recordLength - start);
        return true;
    }

    /// <summary>
    /// The ISO 9660 identifier as a reader sees it: without its version
    /// (<c>;1</c>) and without the dot an empty extension leaves behind,
    /// decoded byte for byte.
    /// </summary>
    private static string PlainName(ReadOnlySpan<byte> identifier)
    {
        int length = identifier.IndexOf(Iso9660Layout.VersionSeparator);
        if (length < 0)
        {
            length = identifier.Length;
        }

        if (length > 1 && identifier[length - 1] == (byte)'.')
        {
            length--;
        }

        Span<char> chars = stackalloc char[length];
        for (int i = 0; i < length; i++)
        {
            chars[i] = (char)identifier[i];
        }

        return new string(chars);
    }

    /// <summary>
    /// The data length of the directory whose extent starts at
    /// <paramref name="extent"/>, read from its own first record.
    /// </summary>
    private static bool TryReadDirectoryLength(Iso9660Superblock superblock, uint extent, out uint dataLength)
    {
        dataLength = 0;
        if (!superblock.TryReadRange((ulong)extent * superblock.BlockSize, Iso9660Layout.MinRecordLength, out byte[]? header))
        {
            return false;
        }

        if (header[Iso9660Layout.RecordLengthOffset] < Iso9660Layout.MinRecordLength
            || (header[Iso9660Layout.RecordFlagsOffset] & Iso9660Layout.FlagDirectory) == 0)
        {
            return false;
        }

        dataLength = BitConverter.ToUInt32(header.AsSpan(Iso9660Layout.RecordDataLengthOffset, 4));
        return dataLength >= Iso9660Layout.MinRecordLength && dataLength <= Iso9660Layout.MaxDirectoryLength;
    }
}
