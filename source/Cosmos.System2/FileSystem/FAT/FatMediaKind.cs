namespace Cosmos.System.FileSystem.FAT
{
    public enum FatMediaKind
    {
        Removable = 0xF0,
        Fixed = 0xF8,
        Floppy1 = 0xF9,
        Floppy2 = 0xFA,
        Floppy3 = 0xFB,
        Floppy4 = 0xFC,
        Floppy5 = 0xFD,
        Floppy6 = 0xFE,
        Floppy7 = 0xFF
    }
}
