// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.FileSystem.Fat;

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
