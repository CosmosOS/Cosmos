// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.FileSystem.Iso9660;

/// <summary>
/// What the Rock Ridge extension recorded about one directory entry: the
/// POSIX attributes (<c>PX</c>), the time stamps (<c>TF</c>), the symbolic
/// link target (<c>SL</c>) and the relocation markers (<c>CL</c>, <c>RE</c>).
/// The name (<c>NM</c>) is parsed beside it. Fields a volume does not record
/// keep their defaults, and <see cref="HasPosixAttributes"/> says whether
/// the mode, link count and ids are the volume's or the driver's defaults.
/// </summary>
internal struct RockRidgeAttributes
{
    /// <summary>True when a <c>PX</c> entry gave the mode, link count, uid and gid.</summary>
    public bool HasPosixAttributes;

    /// <summary>The POSIX mode: file type plus permission bits.</summary>
    public uint Mode;

    /// <summary>The POSIX link count.</summary>
    public uint LinkCount;

    /// <summary>The owner's user id.</summary>
    public uint Uid;

    /// <summary>The owner's group id.</summary>
    public uint Gid;

    /// <summary>True when a <c>TF</c> entry carried a modification stamp.</summary>
    public bool HasModificationTime;

    /// <summary>Modification time in Unix seconds, when <see cref="HasModificationTime"/>.</summary>
    public long ModificationTime;

    /// <summary>True when a <c>TF</c> entry carried an access stamp.</summary>
    public bool HasAccessTime;

    /// <summary>Access time in Unix seconds, when <see cref="HasAccessTime"/>.</summary>
    public long AccessTime;

    /// <summary>True when a <c>TF</c> entry carried an attribute-change stamp.</summary>
    public bool HasChangeTime;

    /// <summary>Attribute-change time in Unix seconds, when <see cref="HasChangeTime"/>.</summary>
    public long ChangeTime;

    /// <summary>The symbolic link target, or null when the entry is not a symbolic link.</summary>
    public string? SymbolicLinkTarget;

    /// <summary>True when a <c>CL</c> entry relocates the directory: its contents are at <see cref="RelocatedExtent"/>.</summary>
    public bool IsRelocationPlaceholder;

    /// <summary>The logical block of the relocated directory's extent, when <see cref="IsRelocationPlaceholder"/>.</summary>
    public uint RelocatedExtent;

    /// <summary>True when an <c>RE</c> entry marks the entry as a relocated directory, which a listing hides.</summary>
    public bool IsRelocated;
}
