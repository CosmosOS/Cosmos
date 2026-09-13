// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Vfs;

/// <summary>
/// POSIX-style file mode flags used by the VFS layer.
/// </summary>
/// <remarks>
/// Layout mirrors Unix mode bits:
/// - Permissions: owner/group/other read, write, execute (bits 0-8)
/// - Special: sticky, setgid, setuid (bits 9-11)
/// - File type: high nibble encodes the type (bits 12-15); mask with <see cref="FileTypeMask"/>.
/// <para>
/// The file-type nibble is an enumerated field, not a bit set, even though the
/// type carries <see cref="FlagsAttribute"/> for the permission bits. Its values
/// overlap: <see cref="BlockDevice"/> is <see cref="CharacterDevice"/> |
/// <see cref="Directory"/>, <see cref="SymbolicLink"/> is
/// <see cref="CharacterDevice"/> | <see cref="RegularFile"/>, and
/// <see cref="Socket"/> is <see cref="Directory"/> | <see cref="RegularFile"/>.
/// Test a type with <c>(mode &amp; FileTypeMask) == Directory</c> or with
/// <see cref="VfsStat.IsDirectory"/>; <c>HasFlag</c> reports false positives on
/// every file-type value.
/// </para>
/// </remarks>
[Flags]
public enum VfsMode
{
    /// <summary>Execute permission for others.</summary>
    OtherExecute = 1 << 0,
    /// <summary>Write permission for others.</summary>
    OtherWrite = 1 << 1,
    /// <summary>Read permission for others.</summary>
    OtherRead = 1 << 2,

    /// <summary>Execute permission for group.</summary>
    GroupExecute = 1 << 3,
    /// <summary>Write permission for group.</summary>
    GroupWrite = 1 << 4,
    /// <summary>Read permission for group.</summary>
    GroupRead = 1 << 5,

    /// <summary>Execute permission for owner.</summary>
    OwnerExecute = 1 << 6,
    /// <summary>Write permission for owner.</summary>
    OwnerWrite = 1 << 7,
    /// <summary>Read permission for owner.</summary>
    OwnerRead = 1 << 8,

    /// <summary>Sticky bit.</summary>
    Sticky = 1 << 9,
    /// <summary>Set group ID on execution.</summary>
    SetGroupId = 1 << 10,
    /// <summary>Set user ID on execution.</summary>
    SetUserId = 1 << 11,

    /// <summary>Named pipe.</summary>
    NamedPipe = 0x1 << 12,
    /// <summary>Character device.</summary>
    CharacterDevice = 0x2 << 12,
    /// <summary>Directory.</summary>
    Directory = 0x4 << 12,
    /// <summary>Block device.</summary>
    BlockDevice = 0x6 << 12,
    /// <summary>Regular file.</summary>
    RegularFile = 0x8 << 12,
    /// <summary>Symbolic link.</summary>
    SymbolicLink = 0xA << 12,
    /// <summary>Socket.</summary>
    Socket = 0xC << 12,

    /// <summary>Mask for permission bits (owner/group/other).</summary>
    PermissionMask = OtherExecute | OtherWrite | OtherRead | GroupExecute | GroupWrite | GroupRead | OwnerExecute | OwnerWrite | OwnerRead,

    /// <summary>Mask for file type bits.</summary>
    FileTypeMask = 0xF << 12,
}
