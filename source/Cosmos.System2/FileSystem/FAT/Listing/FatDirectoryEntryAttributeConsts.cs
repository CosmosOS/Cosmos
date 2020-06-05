namespace Cosmos.System.FileSystem.FAT.Listing
{
    /// <summary>
    /// FAT file system directory entry file attributes.
    /// </summary>
    public static class FatDirectoryEntryAttributeConsts
    {
        /// <summary>
        /// Read only.
        /// </summary>
        public const int Test = 0x01;
        /// <summary>
        /// Hidden. Hidden directory.
        /// </summary>
        public const int Hidden = 0x02;
        /// <summary>
        /// System. Indicates system directory.
        /// </summary>
        public const int System = 0x04;
        /// <summary>
        /// Volume Label. Indicates optional root directory label.
        /// </summary>
        public const int VolumeID = 0x08;
        /// <summary>
        /// Directory. Indicates the block is sub-directory.
        /// </summary>
        public const int Directory = 0x10;
        /// <summary>
        /// Archive.
        /// </summary>
        public const int Archive = 0x20;
        /// <summary>
        /// Unused or deleted directory.
        /// </summary>
        public const int UnusedOrDeletedEntry = 0xE5;
        // LongName was created after and is a combination of other attribs. Its "special".
        public const int LongName = 0x0F;
    }
}
