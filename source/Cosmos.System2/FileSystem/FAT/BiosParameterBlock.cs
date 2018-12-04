using System.Text;

namespace Cosmos.System.FileSystem.FAT
{
    internal sealed class BiosParameterBlock : DataStructure
    {
        public static readonly ByteField JmpBoot0 = new ByteField(0);
        public static readonly ByteField JmpBoot1 = new ByteField(1);
        public static readonly ByteField JmpBoot2 = new ByteField(2);

        public static readonly StringField OemName = new StringField(3, 8, Encoding.ASCII);

        public static readonly UInt16Field BytesPerSector = new UInt16Field(11);
        public static readonly ByteField SectorsPerCluster = new ByteField(13);

        public static readonly UInt16Field ReservedSectorCount = new UInt16Field(14);

        public static readonly ByteField FatCount = new ByteField(16);

        public static readonly UInt16Field RootEntryCount = new UInt16Field(17);

        public static readonly UInt16Field TotalSectorCount16 = new UInt16Field(19);

        public static readonly EnumField<FatMediaKind, byte> MediaKind = new EnumField<FatMediaKind, byte>(new ByteField(21));

        public static readonly UInt16Field FatSectorCount16 = new UInt16Field(22);

        public static readonly UInt16Field SectorsPerTrack = new UInt16Field(24);
        public static readonly UInt16Field HeadCount = new UInt16Field(26);
        public static readonly UInt32Field HiddenSectorCount = new UInt32Field(28);

        public static readonly UInt32Field TotalSectorCount32 = new UInt32Field(32);

        public static readonly UInt16Field Signature = new UInt16Field(510);

        public static class Fat12
        {
            public static readonly ByteField DriveNumber = new ByteField(36);

            public static readonly ByteField Reserved1 = new ByteField(37);

            public static readonly ByteField BootSignature = new ByteField(38);

            public static readonly UInt32Field VolumeId = new UInt32Field(39);
            public static readonly StringField VolumeLabel = new StringField(43, 11, Encoding.ASCII);

            public static readonly StringField FileSystemType = new StringField(54, 8, Encoding.ASCII);
        }

        public static class Fat32
        {
            public static readonly UInt32Field FatSectorCount32 = new UInt32Field(36);

            public static readonly EnumField<Fat32ExtendedFlags, ushort> ExtendedFlags =
                new EnumField<Fat32ExtendedFlags, ushort>(new UInt16Field(40));

            public static readonly UInt16Field FileSystemVersion = new UInt16Field(42);

            public static readonly UInt32Field RootCluster = new UInt32Field(44);

            public static readonly UInt16Field FileSystemInfo = new UInt16Field(48);

            public static readonly UInt16Field BackupBootSector = new UInt16Field(50);

            public static readonly ByteField DriveNumber = new ByteField(64);

            public static readonly ByteField Reserved1 = new ByteField(65);

            public static readonly ByteField BootSignature = new ByteField(66);

            public static readonly UInt32Field VolumeId = new UInt32Field(67);
            public static readonly StringField VolumeLabel = new StringField(71, 11, Encoding.ASCII);

            public static readonly StringField FileSystemType = new StringField(82, 8, Encoding.ASCII);
        }

        public BiosParameterBlock(byte[] data)
            : base(data)
        {
        }
    }
}
