namespace Cosmos.System.FileSystem.FAT.Listing
{
    public sealed class FatDirectoryEntryMetadata
    {
        public readonly uint DataOffset;
        public readonly uint DataLength;

        private FatDirectoryEntryMetadata(uint aDataOffset, uint aDataLength)
        {
            DataOffset = aDataOffset;
            DataLength = aDataLength;
        }

        public static readonly FatDirectoryEntryMetadata FirstByte = new FatDirectoryEntryMetadata(0x00, 1);
        public static readonly FatDirectoryEntryMetadata ShortName = new FatDirectoryEntryMetadata(0x00, 11);
        public static readonly FatDirectoryEntryMetadata Attributes = new FatDirectoryEntryMetadata(0x0B, 1);
        public static readonly FatDirectoryEntryMetadata NTReserved = new FatDirectoryEntryMetadata(0x0C, 1);
        public static readonly FatDirectoryEntryMetadata CreationTimeTenthsOfASecond = new FatDirectoryEntryMetadata(0x0D, 1);
        public static readonly FatDirectoryEntryMetadata CreationTime = new FatDirectoryEntryMetadata(0x0E, 2);
        public static readonly FatDirectoryEntryMetadata CreationDate = new FatDirectoryEntryMetadata(0x10, 2);
        public static readonly FatDirectoryEntryMetadata AccessedDate = new FatDirectoryEntryMetadata(0x12, 2);
        public static readonly FatDirectoryEntryMetadata FirstClusterHigh = new FatDirectoryEntryMetadata(0x14, 2);
        public static readonly FatDirectoryEntryMetadata ModifiedTime = new FatDirectoryEntryMetadata(0x16, 2);
        public static readonly FatDirectoryEntryMetadata ModifiedDate = new FatDirectoryEntryMetadata(0x18, 2);
        public static readonly FatDirectoryEntryMetadata FirstClusterLow = new FatDirectoryEntryMetadata(0x1A, 2);
        public static readonly FatDirectoryEntryMetadata Size = new FatDirectoryEntryMetadata(0x1C, 4);

        public sealed class LongFilenameEntryMetadata
        {
            public static readonly FatDirectoryEntryMetadata SequenceNumberAndAllocationStatus = new FatDirectoryEntryMetadata(0x00, 1);
            public static readonly FatDirectoryEntryMetadata LongName1 = new FatDirectoryEntryMetadata(0x01, 10);
            public static readonly FatDirectoryEntryMetadata Attributes = new FatDirectoryEntryMetadata(0x0B, 1);
            //public static readonly FatDirectoryEntryMetadata Reserved1 = new FatDirectoryEntryMetadata(0x0C, 1);
            public static readonly FatDirectoryEntryMetadata Checksum = new FatDirectoryEntryMetadata(0x0D, 1);
            public static readonly FatDirectoryEntryMetadata LongName2 = new FatDirectoryEntryMetadata(0x0E, 12);
            //public static readonly FatDirectoryEntryMetadata Reserved2 = new FatDirectoryEntryMetadata(0x1A, 2);
            public static readonly FatDirectoryEntryMetadata LongName3 = new FatDirectoryEntryMetadata(0x1C, 4);
        }
    }
}