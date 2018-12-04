using System;

namespace Cosmos.System.FileSystem.FAT
{
    [Flags]
    internal enum Fat32ExtendedFlags
    {
        ActiveFatMask = 0x000F,
        Mirrored = 0x0080
    }
}
