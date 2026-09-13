// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.Filesystems.Fat;

/// <summary>FAT directory-entry attribute bits (FAT spec).</summary>
[Flags]
internal enum FatAttr : byte
{
    None = 0,
    ReadOnly = 0x01,
    Hidden = 0x02,
    System = 0x04,
    VolumeId = 0x08,
    Directory = 0x10,
    Archive = 0x20,
    Lfn = ReadOnly | Hidden | System | VolumeId,
}

/// <summary>Parsed FAT directory entry. <see cref="ByteOffset"/> is the byte position of the 8.3 record within the buffer that produced it; LFN entries that contributed to <see cref="Name"/> precede it.</summary>
internal sealed class FatDirEntry
{
    public string Name { get; }
    public string ShortName { get; }
    public FatAttr Attributes { get; }
    public uint FirstCluster { get; }
    public uint Size { get; }
    public int ByteOffset { get; }
    public int LfnEntryCount { get; }

    public FatDirEntry(
        string name,
        string shortName,
        FatAttr attributes,
        uint firstCluster,
        uint size,
        int byteOffset,
        int lfnEntryCount)
    {
        Name = name;
        ShortName = shortName;
        Attributes = attributes;
        FirstCluster = firstCluster;
        Size = size;
        ByteOffset = byteOffset;
        LfnEntryCount = lfnEntryCount;
    }

    public bool IsDirectory => (Attributes & FatAttr.Directory) != 0;
    public bool IsVolumeId => (Attributes & FatAttr.VolumeId) != 0;
}

/// <summary>
/// Parser and encoder for FAT directory clusters. Operates over raw byte
/// spans; no I/O is performed here.
/// </summary>
internal static class FatDirectory
{
    /// <summary>Size in bytes of one 8.3 directory entry (fatgen103 §6).</summary>
    internal const int EntrySize = 32;

    /// <summary>First byte value marking a deleted entry.</summary>
    internal const byte DeletedMarker = 0xE5;

    /// <summary>First byte value terminating the directory (no entries follow).</summary>
    internal const byte EndOfDirectoryMarker = 0x00;

    /// <summary>Byte offset of the attribute byte within an 8.3 entry.</summary>
    internal const int AttributesOffset = 11;

    /// <summary>Byte offset of the high 16 bits of the first cluster (FAT32; reserved on FAT12/16).</summary>
    internal const int FirstClusterHighOffset = 20;

    /// <summary>Byte offset of the low 16 bits of the first cluster.</summary>
    internal const int FirstClusterLowOffset = 26;

    /// <summary>First-cluster value recorded for an entry that owns no data clusters (fatgen103 §6: DIR_FstClusLO/HI hold 0); also the FAT12/16 fixed root, whose storage lies outside the data area.</summary>
    internal const uint EmptyFirstCluster = 0u;

    /// <summary>Byte offset of the 32-bit file size.</summary>
    internal const int SizeOffset = 28;

    /// <summary>Byte width of each 16-bit half of the first-cluster field (DIR_FstClusLO / DIR_FstClusHI, fatgen103 §6).</summary>
    internal const int ClusterWordBytes = 2;

    /// <summary>Byte width of the 32-bit DIR_FileSize field (fatgen103 §6).</summary>
    internal const int SizeFieldBytes = 4;

    /// <summary>DIR_FileSize recorded for directory entries — fatgen103 §6 mandates 0 for directories.</summary>
    internal const uint DirectorySizeOnDisk = 0u;

    /// <summary>Shift placing DIR_FstClusHI in the upper 16 bits of the 32-bit cluster number (FAT32).</summary>
    internal const int ClusterHighShift = 16;

    /// <summary>Mask isolating one 16-bit half of a first-cluster number.</summary>
    internal const uint ClusterWordMask = 0xFFFFu;

    /// <summary>Longest name the LFN format allows.</summary>
    internal const int MaxLfnNameLength = 255;

    /// <summary>Length of the full 8.3 name field.</summary>
    internal const int ShortNameLength = 11;

    /// <summary>ASCII distance between upper- and lower-case letters.</summary>
    private const int CaseDistance = 32;

    /// <summary>Bit marking the last (highest-ordinal) entry of an LFN chain.</summary>
    private const byte LfnLastBit = 0x40;

    /// <summary>UCS-2 characters carried per LFN entry (5 + 6 + 2).</summary>
    private const int LfnCharsPerEntry = 13;

    /// <summary>Maximum LFN entries per name (255 chars / 13 per entry).</summary>
    private const int MaxLfnEntries = 20;

    /// <summary>The ordinal field is the low 6 bits of the LFN sequence byte.</summary>
    private const byte LfnOrdinalMask = 0x3F;

    /// <summary>Lowest valid LFN ordinal; ordinals are 1-based on disk, so 0 marks a corrupt sequence byte (fatgen103 long-name spec).</summary>
    private const int LfnFirstOrdinal = 1;

    /// <summary>Byte offset of the LFN sequence/ordinal byte (LDIR_Ord) within an LFN entry.</summary>
    private const int LfnSequenceOffset = 0;

    /// <summary>Byte offset of the LFN type field (always 0).</summary>
    private const int LfnTypeOffset = 12;

    /// <summary>LDIR_Type value marking an LFN name-component entry (always 0, fatgen103).</summary>
    private const byte LfnTypeNameEntry = 0;

    /// <summary>Byte offset of the 8.3-name checksum within an LFN entry.</summary>
    private const int LfnChecksumOffset = 13;

    /// <summary>Bit injected at the top when the fatgen103 ChkSum rotate-right carries the low bit into bit 7.</summary>
    private const byte ChecksumCarryBit = 0x80;

    /// <summary>First LFN name region: offset / byte length (5 UCS-2 chars).</summary>
    private const int LfnName1Offset = 1;
    private const int LfnName1Bytes = 10;

    /// <summary>Second LFN name region: offset / byte length (6 UCS-2 chars).</summary>
    private const int LfnName2Offset = 14;
    private const int LfnName2Bytes = 12;

    /// <summary>Third LFN name region: offset / byte length (2 UCS-2 chars).</summary>
    private const int LfnName3Offset = 28;
    private const int LfnName3Bytes = 4;

    /// <summary>UCS-2 characters held by the first LFN name region.</summary>
    private const int LfnName1Chars = 5;

    /// <summary>UCS-2 characters held by the second LFN name region.</summary>
    private const int LfnName2Chars = 6;

    /// <summary>UCS-2 characters held by the third LFN name region.</summary>
    private const int LfnName3Chars = 2;

    /// <summary>Fill character padding unused LFN name slots after the NUL terminator (fatgen103).</summary>
    private const char LfnPadChar = (char)0xFFFF;

    /// <summary>Bytes per UCS-2 character in the on-disk LFN name regions (UTF-16LE).</summary>
    private const int Ucs2BytesPerChar = 2;

    /// <summary>Byte offset of the high-order byte within one little-endian UCS-2 character.</summary>
    private const int Ucs2HighByteOffset = 1;

    /// <summary>Shift between the low and high byte of a little-endian UCS-2 character.</summary>
    private const int BitsPerByte = 8;

    /// <summary>Mask isolating the low byte of a UCS-2 character.</summary>
    private const int LowByteMask = 0xFF;

    /// <summary>Space, the 8.3 pad byte.</summary>
    private const byte PadByte = 0x20;

    /// <summary>Stored first byte substituting a real 0xE5 (Kanji lead byte).</summary>
    private const byte KanjiLeadSubstitute = 0x05;

    /// <summary>Length of the 8.3 base name field.</summary>
    private const int ShortBaseLength = 8;

    /// <summary>Length of the 8.3 extension field.</summary>
    private const int ShortExtLength = 3;

    /// <summary>Longest rendered 8.3 name: 8 base + dot + 3 extension.</summary>
    private const int MaxShortNameChars = 12;

    /// <summary>An 8.3-representable name carries at most one dot (the base/extension separator).</summary>
    private const int MaxShortNameDots = 1;

    /// <summary>Numeric ~N short-name tails are rendered in decimal (fatgen103 numeric-tail generation).</summary>
    private const uint NumericTailRadix = 10;

    /// <summary>First tail value probed when mangling a long name (~1 before ~2, fatgen103 numeric-tail generation).</summary>
    private const uint FirstNumericTail = 1;

    /// <summary>Base-name slots consumed by the '~' separator of a numeric tail.</summary>
    private const int NumericTailTildeChars = 1;

    /// <summary>
    /// Parse the raw directory data. <paramref name="fat32"/> selects
    /// whether the FstClusHI word participates in <see cref="FatDirEntry.FirstCluster"/> —
    /// on FAT12/16 that field is reserved (OS/2/NT stored the EA handle
    /// there) and must be ignored.
    /// </summary>
    internal static List<FatDirEntry> Parse(ReadOnlySpan<byte> buffer, bool fat32)
    {
        List<FatDirEntry> result = new();
        Span<char> lfnAccum = stackalloc char[LfnCharsPerEntry * MaxLfnEntries];
        int lfnLength = 0;
        int lfnEntryCount = 0;
        byte lfnChecksum = 0;

        for (int offset = 0; offset + EntrySize <= buffer.Length; offset += EntrySize)
        {
            byte first = buffer[offset];
            if (first == EndOfDirectoryMarker)
            {
                break;
            }

            if (first == DeletedMarker)
            {
                lfnAccum.Clear();
                lfnLength = 0;
                lfnEntryCount = 0;
                continue;
            }

            FatAttr attr = (FatAttr)buffer[offset + AttributesOffset];

            if (attr == FatAttr.Lfn)
            {
                // LFN metadata is untrusted: validate the 6-bit ordinal
                // and require one checksum across the chain, or a stale
                // accumulator splices two names together.
                byte sequence = buffer[offset];
                int seqIndex = (sequence & LfnOrdinalMask) - LfnFirstOrdinal;
                byte checksum = buffer[offset + LfnChecksumOffset];
                if (seqIndex < 0 || seqIndex >= MaxLfnEntries
                    || (lfnEntryCount > 0 && checksum != lfnChecksum))
                {
                    lfnAccum.Clear();
                    lfnLength = 0;
                    lfnEntryCount = 0;
                    continue;
                }

                lfnChecksum = checksum;
                int destBase = seqIndex * LfnCharsPerEntry;
                ReadLfnChars(buffer.Slice(offset, EntrySize), lfnAccum.Slice(destBase, LfnCharsPerEntry));

                int candidateLength = destBase + LfnCharsPerEntry;
                if (candidateLength > lfnLength)
                {
                    lfnLength = candidateLength;
                }
                lfnEntryCount++;
                continue;
            }

            string shortName = DecodeShortName(buffer.Slice(offset, ShortNameLength), first);

            // An LFN chain only belongs to this record when its checksum
            // matches the 8.3 name; otherwise it is an orphan left by a
            // non-LFN-aware tool and must not lend its name (or its slot
            // count, which drives deletion) to this entry.
            bool lfnValid = lfnLength > 0
                && lfnChecksum == ComputeShortChecksum(buffer.Slice(offset, ShortNameLength));
            string longName = lfnValid
                ? TrimLfn(lfnAccum.Slice(0, lfnLength))
                : shortName;

            uint firstClusterLow = BitConverter.ToUInt16(buffer.Slice(offset + FirstClusterLowOffset, ClusterWordBytes));
            uint firstCluster = firstClusterLow;
            if (fat32)
            {
                uint firstClusterHigh = BitConverter.ToUInt16(buffer.Slice(offset + FirstClusterHighOffset, ClusterWordBytes));
                firstCluster |= firstClusterHigh << ClusterHighShift;
            }
            uint size = BitConverter.ToUInt32(buffer.Slice(offset + SizeOffset, SizeFieldBytes));

            result.Add(new FatDirEntry(
                longName,
                shortName,
                attr,
                firstCluster,
                size,
                offset,
                lfnValid ? lfnEntryCount : 0));

            lfnAccum.Clear();
            lfnLength = 0;
            lfnEntryCount = 0;
        }

        return result;
    }

    /// <summary>
    /// Locate the first free entry slot (deleted or unused) where
    /// <paramref name="entriesNeeded"/> consecutive 32-byte slots fit.
    /// Per spec the 0x00 terminator frees its own slot and everything
    /// after it, so scanning stops there and the remaining buffer counts
    /// as one free region — stale bytes past the terminator must never
    /// reset the run (or be treated as occupied). When the returned run
    /// overlaps the terminator, <paramref name="consumedTerminator"/> is
    /// true and the caller must re-terminate after the new entries.
    /// </summary>
    internal static int FindFreeRun(ReadOnlySpan<byte> buffer, int entriesNeeded, out bool consumedTerminator)
    {
        consumedTerminator = false;
        int run = 0;
        int runStart = -1;

        for (int offset = 0; offset + EntrySize <= buffer.Length; offset += EntrySize)
        {
            byte first = buffer[offset];
            if (first == EndOfDirectoryMarker)
            {
                if (run == 0)
                {
                    runStart = offset;
                }
                int available = run + (buffer.Length - offset) / EntrySize;
                if (available >= entriesNeeded)
                {
                    consumedTerminator = runStart + entriesNeeded * EntrySize > offset;
                    return runStart;
                }
                return -1;
            }
            if (first == DeletedMarker)
            {
                if (run == 0)
                {
                    runStart = offset;
                }
                run++;
                if (run >= entriesNeeded)
                {
                    return runStart;
                }
            }
            else
            {
                run = 0;
                runStart = -1;
            }
        }

        return -1;
    }

    internal static int LfnEntryCountFor(ReadOnlySpan<char> name)
    {
        if (FitsInShortName(name))
        {
            return 0;
        }
        return (name.Length + LfnCharsPerEntry - 1) / LfnCharsPerEntry;
    }

    /// <summary>Write an 8.3 entry to <paramref name="dest"/> at <paramref name="offset"/>; <paramref name="dest"/> must be writable.</summary>
    internal static void WriteShortEntry(
        Span<byte> dest,
        int offset,
        ReadOnlySpan<char> shortName11,
        FatAttr attributes,
        uint firstCluster,
        uint size)
    {
        Span<byte> entry = dest.Slice(offset, EntrySize);
        entry.Clear();

        for (int i = 0; i < ShortNameLength && i < shortName11.Length; i++)
        {
            entry[i] = (byte)shortName11[i];
        }
        for (int i = shortName11.Length; i < ShortNameLength; i++)
        {
            entry[i] = PadByte;
        }

        entry[AttributesOffset] = (byte)attributes;
        BitConverter.TryWriteBytes(entry.Slice(FirstClusterHighOffset, ClusterWordBytes), (ushort)((firstCluster >> ClusterHighShift) & ClusterWordMask));
        BitConverter.TryWriteBytes(entry.Slice(FirstClusterLowOffset, ClusterWordBytes), (ushort)(firstCluster & ClusterWordMask));
        BitConverter.TryWriteBytes(entry.Slice(SizeOffset, SizeFieldBytes), size);
    }

    /// <summary>
    /// Case-insensitive ASCII comparison used for FAT name matching —
    /// the single comparer shared by the superblock and inode layers.
    /// </summary>
    internal static bool NameEqualsIgnoreCase(string a, string b)
    {
        if (a.Length != b.Length)
        {
            return false;
        }
        for (int i = 0; i < a.Length; i++)
        {
            char ac = a[i];
            char bc = b[i];
            if (ac >= 'a' && ac <= 'z')
            {
                ac = (char)(ac - CaseDistance);
            }
            if (bc >= 'a' && bc <= 'z')
            {
                bc = (char)(bc - CaseDistance);
            }
            if (ac != bc)
            {
                return false;
            }
        }
        return true;
    }

    internal static void WriteLfnEntries(
        Span<byte> dest,
        int offset,
        ReadOnlySpan<char> longName,
        ReadOnlySpan<char> shortName11)
    {
        // Writer-side rejection: 255 UCS-2 chars is the LFN cap, and our
        // own parser drops ordinals past 20 — longer names would leave
        // orphaned LFN slots behind on deletion.
        if (longName.Length > MaxLfnNameLength)
        {
            throw new ArgumentOutOfRangeException(nameof(longName), "LFN names are limited to 255 characters.");
        }

        int entries = LfnEntryCountFor(longName);
        if (entries == 0)
        {
            return;
        }

        byte checksum = ComputeShortChecksum(shortName11);
        Span<char> chunk = stackalloc char[LfnCharsPerEntry];
        for (int i = 0; i < entries; i++)
        {
            int seq = entries - i;
            int sourceStart = (seq - LfnFirstOrdinal) * LfnCharsPerEntry;
            for (int c = 0; c < LfnCharsPerEntry; c++)
            {
                int srcIdx = sourceStart + c;
                if (srcIdx < longName.Length)
                {
                    chunk[c] = longName[srcIdx];
                }
                else if (srcIdx == longName.Length)
                {
                    chunk[c] = '\0';
                }
                else
                {
                    chunk[c] = LfnPadChar;
                }
            }

            byte sequence = (byte)seq;
            if (i == 0)
            {
                sequence |= LfnLastBit;
            }

            Span<byte> entry = dest.Slice(offset + i * EntrySize, EntrySize);
            entry.Clear();
            entry[LfnSequenceOffset] = sequence;
            entry[AttributesOffset] = (byte)FatAttr.Lfn;
            entry[LfnTypeOffset] = LfnTypeNameEntry;
            entry[LfnChecksumOffset] = checksum;

            WriteUcs2(chunk.Slice(0, LfnName1Chars), entry.Slice(LfnName1Offset, LfnName1Bytes));
            WriteUcs2(chunk.Slice(LfnName1Chars, LfnName2Chars), entry.Slice(LfnName2Offset, LfnName2Bytes));
            WriteUcs2(chunk.Slice(LfnName1Chars + LfnName2Chars, LfnName3Chars), entry.Slice(LfnName3Offset, LfnName3Bytes));
        }
    }

    internal static byte ComputeShortChecksum(ReadOnlySpan<char> shortName11)
    {
        byte sum = 0;
        for (int i = 0; i < ShortNameLength; i++)
        {
            byte b = i < shortName11.Length ? (byte)shortName11[i] : PadByte;
            sum = (byte)(((sum & 1) != 0 ? ChecksumCarryBit : 0) + (sum >> 1) + b);
        }
        return sum;
    }

    /// <summary>Checksum over the raw 11 on-disk name bytes (fatgen103 algorithm).</summary>
    internal static byte ComputeShortChecksum(ReadOnlySpan<byte> raw11)
    {
        byte sum = 0;
        for (int i = 0; i < ShortNameLength; i++)
        {
            sum = (byte)(((sum & 1) != 0 ? ChecksumCarryBit : 0) + (sum >> 1) + raw11[i]);
        }
        return sum;
    }

    /// <summary>
    /// Build the 8.3 name for <paramref name="longName"/>. When the name
    /// does not fit 8.3 as-is, a numeric tail (~1, ~2, ...) is chosen so
    /// the result collides with no live short name in
    /// <paramref name="directoryData"/> — duplicate short names are
    /// invalid on FAT, and short-name lookups would resolve to the wrong
    /// file.
    /// </summary>
    internal static void BuildShortName(string longName, Span<char> dest11, ReadOnlySpan<byte> directoryData)
    {
        for (int i = 0; i < ShortNameLength; i++)
        {
            dest11[i] = ' ';
        }

        if (string.IsNullOrEmpty(longName))
        {
            return;
        }

        int dot = longName.LastIndexOf('.');
        ReadOnlySpan<char> baseName = dot >= 0 ? longName.AsSpan(0, dot) : longName.AsSpan();
        ReadOnlySpan<char> ext = dot >= 0 && dot + 1 < longName.Length
            ? longName.AsSpan(dot + 1)
            : ReadOnlySpan<char>.Empty;

        int e = 0;
        for (int i = 0; i < ext.Length && e < ShortExtLength; i++)
        {
            char c = NormalizeShort(ext[i]);
            if (c != '\0')
            {
                dest11[ShortBaseLength + e++] = c;
            }
        }

        if (FitsInShortName(longName))
        {
            int b = 0;
            for (int i = 0; i < baseName.Length && b < ShortBaseLength; i++)
            {
                char c = NormalizeShort(baseName[i]);
                if (c != '\0')
                {
                    dest11[b++] = c;
                }
            }
            return;
        }

        // Collect the normalized base once, then probe ~1, ~2, ...
        // (shrinking the kept base as the tail widens) until the result
        // is unique within the directory.
        Span<char> normBase = stackalloc char[ShortBaseLength];
        int normLen = 0;
        for (int i = 0; i < baseName.Length && normLen < ShortBaseLength; i++)
        {
            char c = NormalizeShort(baseName[i]);
            if (c != '\0')
            {
                normBase[normLen++] = c;
            }
        }

        for (uint tail = FirstNumericTail; ; tail++)
        {
            int digits = CountDigits(tail);
            int keep = ShortBaseLength - NumericTailTildeChars - digits;
            if (keep > normLen)
            {
                keep = normLen;
            }
            if (keep < 0)
            {
                keep = 0;
            }

            int b = 0;
            for (; b < keep; b++)
            {
                dest11[b] = normBase[b];
            }
            dest11[b++] = '~';
            uint value = tail;
            for (int i = digits - 1; i >= 0; i--)
            {
                dest11[b + i] = (char)('0' + value % NumericTailRadix);
                value /= NumericTailRadix;
            }
            b += digits;
            for (int i = b; i < ShortBaseLength; i++)
            {
                dest11[i] = ' ';
            }

            if (!ShortNameExists(directoryData, dest11))
            {
                return;
            }
        }
    }

    internal static bool FitsInShortName(ReadOnlySpan<char> name)
    {
        if (name.Length == 0 || name.Length > MaxShortNameChars)
        {
            return false;
        }

        int dot = name.LastIndexOf('.');
        int baseLen = dot >= 0 ? dot : name.Length;
        int extLen = dot >= 0 ? name.Length - dot - 1 : 0;
        if (baseLen == 0 || baseLen > ShortBaseLength || extLen > ShortExtLength)
        {
            return false;
        }

        int dots = 0;
        for (int i = 0; i < name.Length; i++)
        {
            char c = name[i];
            if (c == '.')
            {
                dots++;
                if (dots > MaxShortNameDots)
                {
                    return false;
                }
                continue;
            }
            // Any character the 8.3 encoder would alter (upcase, mangle
            // to '_', or drop) must take the LFN path, or the requested
            // spelling exists nowhere on the volume after a re-parse.
            if (NormalizeShort(c) != c)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>True when a live entry already carries this exact 11-char short name.</summary>
    private static bool ShortNameExists(ReadOnlySpan<byte> directoryData, ReadOnlySpan<char> short11)
    {
        for (int offset = 0; offset + EntrySize <= directoryData.Length; offset += EntrySize)
        {
            byte first = directoryData[offset];
            if (first == EndOfDirectoryMarker)
            {
                return false;
            }
            if (first == DeletedMarker || (FatAttr)directoryData[offset + AttributesOffset] == FatAttr.Lfn)
            {
                continue;
            }
            bool same = true;
            for (int i = 0; i < ShortNameLength && same; i++)
            {
                same = directoryData[offset + i] == (byte)short11[i];
            }
            if (same)
            {
                return true;
            }
        }
        return false;
    }

    private static int CountDigits(uint value)
    {
        int digits = 1;
        while (value >= NumericTailRadix)
        {
            value /= NumericTailRadix;
            digits++;
        }
        return digits;
    }

    internal static void MarkDeleted(Span<byte> dest, int offset, int entryCount)
    {
        for (int i = 0; i < entryCount; i++)
        {
            dest[offset + i * EntrySize] = DeletedMarker;
        }
    }

    private static void ReadLfnChars(ReadOnlySpan<byte> entry, Span<char> dest13)
    {
        ReadUcs2(entry.Slice(LfnName1Offset, LfnName1Bytes), dest13.Slice(0, LfnName1Chars));
        ReadUcs2(entry.Slice(LfnName2Offset, LfnName2Bytes), dest13.Slice(LfnName1Chars, LfnName2Chars));
        ReadUcs2(entry.Slice(LfnName3Offset, LfnName3Bytes), dest13.Slice(LfnName1Chars + LfnName2Chars, LfnName3Chars));
    }

    private static void ReadUcs2(ReadOnlySpan<byte> src, Span<char> dest)
    {
        for (int i = 0; i < dest.Length; i++)
        {
            dest[i] = (char)(src[i * Ucs2BytesPerChar] | (src[i * Ucs2BytesPerChar + Ucs2HighByteOffset] << BitsPerByte));
        }
    }

    private static void WriteUcs2(ReadOnlySpan<char> src, Span<byte> dest)
    {
        for (int i = 0; i < src.Length; i++)
        {
            dest[i * Ucs2BytesPerChar] = (byte)(src[i] & LowByteMask);
            dest[i * Ucs2BytesPerChar + Ucs2HighByteOffset] = (byte)((src[i] >> BitsPerByte) & LowByteMask);
        }
    }

    private static string TrimLfn(Span<char> chars)
    {
        int end = chars.Length;
        for (int i = 0; i < chars.Length; i++)
        {
            if (chars[i] == '\0' || chars[i] == LfnPadChar)
            {
                end = i;
                break;
            }
        }
        return new string(chars.Slice(0, end));
    }

    private static string DecodeShortName(ReadOnlySpan<byte> raw11, byte firstByte)
    {
        Span<char> chars = stackalloc char[MaxShortNameChars];
        int len = 0;

        byte effectiveFirst = firstByte == KanjiLeadSubstitute ? DeletedMarker : firstByte;
        chars[len++] = (char)effectiveFirst;

        for (int i = 1; i < ShortBaseLength; i++)
        {
            if (raw11[i] == PadByte)
            {
                break;
            }
            chars[len++] = (char)raw11[i];
        }

        bool hasExt = false;
        for (int i = ShortBaseLength; i < ShortNameLength; i++)
        {
            if (raw11[i] != PadByte)
            {
                hasExt = true;
                break;
            }
        }

        if (hasExt)
        {
            chars[len++] = '.';
            for (int i = ShortBaseLength; i < ShortNameLength; i++)
            {
                if (raw11[i] == PadByte)
                {
                    break;
                }
                chars[len++] = (char)raw11[i];
            }
        }

        return new string(chars.Slice(0, len));
    }

    private static char NormalizeShort(char c)
    {
        if (c == ' ')
        {
            return '\0';
        }
        if (c >= 'a' && c <= 'z')
        {
            return (char)(c - CaseDistance);
        }
        if ((c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'))
        {
            return c;
        }
        if (c == '$' || c == '%' || c == '\'' || c == '-' || c == '_'
            || c == '@' || c == '~' || c == '`' || c == '!' || c == '('
            || c == ')' || c == '{' || c == '}' || c == '^' || c == '#' || c == '&')
        {
            return c;
        }
        return '_';
    }
}
