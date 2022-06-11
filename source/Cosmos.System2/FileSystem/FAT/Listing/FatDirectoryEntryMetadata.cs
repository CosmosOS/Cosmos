namespace Cosmos.System.FileSystem.FAT.Listing;

/// <summary>
///     Fat directory entry metadata class.
/// </summary>
public sealed class FatDirectoryEntryMetadata
{
    /// <summary>
    ///     Directory entry first byte matadata. This is filename first byte.
    /// </summary>
    public static readonly FatDirectoryEntryMetadata FirstByte = new(0x00, 1);

    /// <summary>
    ///     Directory entry short name matadata. This is the filename + filename extension.
    ///     The dot '.' separate filename and filename extention is implied.
    /// </summary>
    public static readonly FatDirectoryEntryMetadata ShortName = new(0x00, 11);

    /// <summary>
    ///     Directory entry attributes matadata. This contains information about the directory.
    /// </summary>
    public static readonly FatDirectoryEntryMetadata Attributes = new(0x0B, 1);

    /// <summary>
    ///     Directory entry reserved matadata.
    /// </summary>
    public static readonly FatDirectoryEntryMetadata NTReserved = new(0x0C, 1);

    /// <summary>
    ///     Directory entry creation time tenths of a second matadata.
    /// </summary>
    public static readonly FatDirectoryEntryMetadata CreationTimeTenthsOfASecond = new(0x0D, 1);

    /// <summary>
    ///     Directory entry creation time matadata.
    /// </summary>
    public static readonly FatDirectoryEntryMetadata CreationTime = new(0x0E, 2);

    /// <summary>
    ///     Directory entry creation date matadata.
    /// </summary>
    public static readonly FatDirectoryEntryMetadata CreationDate = new(0x10, 2);

    /// <summary>
    ///     Directory entry last accessed date matadata.
    /// </summary>
    public static readonly FatDirectoryEntryMetadata AccessedDate = new(0x12, 2);

    /// <summary>
    ///     Directory entry first cluster high matadata.
    /// </summary>
    public static readonly FatDirectoryEntryMetadata FirstClusterHigh = new(0x14, 2);

    /// <summary>
    ///     Directory entry last modified time matadata.
    /// </summary>
    public static readonly FatDirectoryEntryMetadata ModifiedTime = new(0x16, 2);

    /// <summary>
    ///     Directory entry last modified date matadata.
    /// </summary>
    public static readonly FatDirectoryEntryMetadata ModifiedDate = new(0x18, 2);

    /// <summary>
    ///     Directory entry first cluster low matadata.
    /// </summary>
    public static readonly FatDirectoryEntryMetadata FirstClusterLow = new(0x1A, 2);

    /// <summary>
    ///     Directory entry drectory size matadata.
    /// </summary>
    public static readonly FatDirectoryEntryMetadata Size = new(0x1C, 4);

    /// <summary>
    ///     Entry data length.
    /// </summary>
    public readonly uint DataLength;

    /// <summary>
    ///     Entry data offset.
    /// </summary>
    public readonly uint DataOffset;

    private FatDirectoryEntryMetadata(uint aDataOffset, uint aDataLength)
    {
        DataOffset = aDataOffset;
        DataLength = aDataLength;
    }

    /// <summary>
    ///     Long filename entry metadata class. This class can not be inherited.
    /// </summary>
    public sealed class LongFilenameEntryMetadata
    {
        /// <summary>
        ///     Directory entry sequence number and allocation status matadata.
        /// </summary>
        public static readonly FatDirectoryEntryMetadata SequenceNumberAndAllocationStatus = new(0x00, 1);

        /// <summary>
        ///     Directory entry LongName1 matadata.
        /// </summary>
        public static readonly FatDirectoryEntryMetadata LongName1 = new(0x01, 10);

        /// <summary>
        ///     Directory entry attributes matadata.
        /// </summary>
        public static readonly FatDirectoryEntryMetadata Attributes = new(0x0B, 1);

        //public static readonly FatDirectoryEntryMetadata Reserved1 = new FatDirectoryEntryMetadata(0x0C, 1);
        /// <summary>
        ///     Directory entry checksum matadata.
        /// </summary>
        public static readonly FatDirectoryEntryMetadata Checksum = new(0x0D, 1);

        /// <summary>
        ///     Directory entry LongName2 matadata.
        /// </summary>
        public static readonly FatDirectoryEntryMetadata LongName2 = new(0x0E, 12);

        //public static readonly FatDirectoryEntryMetadata Reserved2 = new FatDirectoryEntryMetadata(0x1A, 2);
        /// <summary>
        ///     Directory entry LongName3 matadata.
        /// </summary>
        public static readonly FatDirectoryEntryMetadata LongName3 = new(0x1C, 4);
    }
}
