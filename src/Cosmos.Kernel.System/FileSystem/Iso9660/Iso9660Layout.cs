// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.FileSystem.Iso9660;

/// <summary>
/// On-disk layout constants for ISO 9660 (ECMA-119) volume descriptors and
/// directory records, and for the System Use Sharing Protocol entries the
/// Rock Ridge extension (RRIP 1.12) records in them. Both-endian fields are
/// read through their little-endian half.
/// </summary>
internal static class Iso9660Layout
{
    /// <summary>Size of a volume descriptor, and the logical sector the descriptors are addressed in.</summary>
    public const int DescriptorSize = 2048;

    /// <summary>Logical sector of the first volume descriptor: the system area takes the 16 before it.</summary>
    public const int FirstDescriptorSector = 16;

    /// <summary>Byte offset of the first volume descriptor on the volume.</summary>
    public const long FirstDescriptorOffset = (long)FirstDescriptorSector * DescriptorSize;

    /// <summary>Descriptors read before the set is given up as unterminated.</summary>
    public const int MaxDescriptors = 32;

    /// <summary>Descriptor type of the primary volume descriptor.</summary>
    public const byte PrimaryDescriptorType = 1;

    /// <summary>Descriptor type of the set terminator.</summary>
    public const byte TerminatorDescriptorType = 255;

    /// <summary>The standard identifier every descriptor carries at offset 1.</summary>
    public const string StandardIdentifier = "CD001";

    /// <summary>Byte offset of the descriptor type.</summary>
    public const int DescriptorTypeOffset = 0;

    /// <summary>Byte offset of the standard identifier (5 bytes).</summary>
    public const int StandardIdentifierOffset = 1;

    /// <summary>Byte offset of the descriptor version, 1 for the primary descriptor.</summary>
    public const int DescriptorVersionOffset = 6;

    /// <summary>The descriptor version this driver reads.</summary>
    public const byte PrimaryDescriptorVersion = 1;

    /// <summary>Byte offset of the volume identifier (32 a-characters, space padded).</summary>
    public const int VolumeIdentifierOffset = 40;

    /// <summary>Length of the volume identifier.</summary>
    public const int VolumeIdentifierLength = 32;

    /// <summary>Byte offset of the volume space size in logical blocks (both-endian 32-bit).</summary>
    public const int VolumeSpaceSizeOffset = 80;

    /// <summary>Byte offset of the logical block size (both-endian 16-bit).</summary>
    public const int LogicalBlockSizeOffset = 128;

    /// <summary>Byte offset of the root directory record (34 bytes).</summary>
    public const int RootDirectoryRecordOffset = 156;

    /// <summary>Length of the root directory record inside the descriptor.</summary>
    public const int RootDirectoryRecordLength = 34;

    /// <summary>Byte offset of a directory record's length.</summary>
    public const int RecordLengthOffset = 0;

    /// <summary>Byte offset of the extent's first logical block (both-endian 32-bit).</summary>
    public const int RecordExtentOffset = 2;

    /// <summary>Byte offset of the extent's data length in bytes (both-endian 32-bit).</summary>
    public const int RecordDataLengthOffset = 10;

    /// <summary>Byte offset of the recording date (7 bytes).</summary>
    public const int RecordDateOffset = 18;

    /// <summary>Byte offset of the file flags.</summary>
    public const int RecordFlagsOffset = 25;

    /// <summary>Byte offset of the file identifier's length.</summary>
    public const int RecordNameLengthOffset = 32;

    /// <summary>Byte offset of the file identifier.</summary>
    public const int RecordNameOffset = 33;

    /// <summary>Smallest directory record: the fixed part plus a one-byte identifier.</summary>
    public const int MinRecordLength = RecordNameOffset + 1;

    /// <summary>File flag: the record describes a directory.</summary>
    public const byte FlagDirectory = 0x02;

    /// <summary>File flag: the record is an associated file, which a reader skips.</summary>
    public const byte FlagAssociated = 0x04;

    /// <summary>File flag: the extent is not the file's last one (multi-extent file, unsupported).</summary>
    public const byte FlagNotFinalExtent = 0x80;

    /// <summary>The identifier of a directory's own record.</summary>
    public const byte CurrentDirectoryIdentifier = 0x00;

    /// <summary>The identifier of a directory's parent record.</summary>
    public const byte ParentDirectoryIdentifier = 0x01;

    /// <summary>The separator before the version of a file identifier (<c>;1</c>).</summary>
    public const byte VersionSeparator = (byte)';';

    /// <summary>Largest directory extent the walker reads; a directory record claiming more is corrupt.</summary>
    public const uint MaxDirectoryLength = 16 * 1024 * 1024;

    /// <summary>Longest name an entry can present: a Rock Ridge name is bounded by the system use areas.</summary>
    public const ulong MaxNameLength = 255;

    /// <summary>Years in the recording date count from this one.</summary>
    public const int RecordDateBaseYear = 1900;

    /// <summary>Length of a 7-byte recording date.</summary>
    public const int ShortDateLength = 7;

    /// <summary>Length of a 17-byte descriptor-format date.</summary>
    public const int LongDateLength = 17;

    /// <summary>Byte offset of the length in a SUSP entry.</summary>
    public const int SuspLengthOffset = 2;

    /// <summary>Size of a SUSP entry's header; the payload follows it.</summary>
    public const int SuspHeaderLength = 4;

    /// <summary>The check bytes an SP entry carries, proving the volume uses SUSP.</summary>
    public const ushort SuspCheckBytes = 0xEFBE;

    /// <summary>Byte offset of the check bytes in an SP payload.</summary>
    public const int SpCheckOffset = 0;

    /// <summary>Byte offset of the skip length in an SP payload.</summary>
    public const int SpSkipOffset = 2;

    /// <summary>Byte offset of the continuation block in a CE payload (both-endian 32-bit).</summary>
    public const int CeBlockOffset = 0;

    /// <summary>Byte offset of the offset inside that block in a CE payload (both-endian 32-bit).</summary>
    public const int CeOffsetOffset = 8;

    /// <summary>Byte offset of the continuation's length in a CE payload (both-endian 32-bit).</summary>
    public const int CeLengthOffset = 16;

    /// <summary>Byte offset of the mode in a PX payload (both-endian 32-bit).</summary>
    public const int PxModeOffset = 0;

    /// <summary>Byte offset of the link count in a PX payload (both-endian 32-bit).</summary>
    public const int PxLinksOffset = 8;

    /// <summary>Byte offset of the user id in a PX payload (both-endian 32-bit).</summary>
    public const int PxUidOffset = 16;

    /// <summary>Byte offset of the group id in a PX payload (both-endian 32-bit).</summary>
    public const int PxGidOffset = 24;

    /// <summary>Smallest PX payload: the four both-endian fields, without the serial number.</summary>
    public const int PxMinLength = 32;

    /// <summary>The file type mask in a POSIX mode.</summary>
    public const uint PosixTypeMask = 0xF000;

    /// <summary>POSIX directory type.</summary>
    public const uint PosixDirectory = 0x4000;

    /// <summary>POSIX symbolic link type.</summary>
    public const uint PosixSymbolicLink = 0xA000;

    /// <summary>The permission, setuid, setgid and sticky bits in a POSIX mode.</summary>
    public const uint PosixPermissionMask = 0x0FFF;

    /// <summary>Byte offset of the flags in a TF payload.</summary>
    public const int TfFlagsOffset = 0;

    /// <summary>TF flag: a creation stamp is present.</summary>
    public const byte TfCreation = 0x01;

    /// <summary>TF flag: a modification stamp is present.</summary>
    public const byte TfModify = 0x02;

    /// <summary>TF flag: an access stamp is present.</summary>
    public const byte TfAccess = 0x04;

    /// <summary>TF flag: an attribute-change stamp is present.</summary>
    public const byte TfAttributes = 0x08;

    /// <summary>TF flag: the stamps take the 17-byte descriptor form.</summary>
    public const byte TfLongForm = 0x80;

    /// <summary>SL component flag: the component continues in the next SL entry.</summary>
    public const byte SlComponentContinue = 0x01;

    /// <summary>SL component flag: the component is the current directory.</summary>
    public const byte SlComponentCurrent = 0x02;

    /// <summary>SL component flag: the component is the parent directory.</summary>
    public const byte SlComponentParent = 0x04;

    /// <summary>SL component flag: the component is the root directory.</summary>
    public const byte SlComponentRoot = 0x08;

    /// <summary>Size of an SL component's header: its flags and its length.</summary>
    public const int SlComponentHeaderLength = 2;

    /// <summary>Byte offset of the relocated directory's extent in a CL payload (both-endian 32-bit).</summary>
    public const int ClExtentOffset = 0;
}
