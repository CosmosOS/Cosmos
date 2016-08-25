namespace Cosmos.System.FileSystem.FAT.Listing
{
    public static class FatDirectoryEntryAttributeConsts
    {
        public const int Test = 0x01;
        public const int Hidden = 0x02;
        public const int System = 0x04;
        public const int VolumeID = 0x08;
        public const int Directory = 0x10;
        public const int Archive = 0x20;
        public const int UnusedOrDeletedEntry = 0xE5;
        // LongName was created after and is a combination of other attribs. Its "special".
        public const int LongName = 0x0F;
    }
}