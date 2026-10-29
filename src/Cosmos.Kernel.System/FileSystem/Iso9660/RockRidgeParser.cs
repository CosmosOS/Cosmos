// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Text;

namespace Cosmos.Kernel.System.FileSystem.Iso9660;

/// <summary>
/// Reads the System Use Sharing Protocol entries in a directory record's
/// system use area and the continuation area a <c>CE</c> entry names, and
/// collects what the Rock Ridge extension recorded there. Entries the driver
/// does not read (<c>ER</c>, <c>ES</c>, <c>RR</c>, <c>SF</c>, <c>PN</c>,
/// <c>PL</c>, anything unknown) are stepped over by their length. Every
/// length and offset is untrusted: an entry that runs past its area ends the
/// walk, and a continuation outside the volume is ignored.
/// </summary>
internal sealed class RockRidgeParser
{
    /// <summary>Continuation areas a record may chain through before the walk stops; a loop is corrupt.</summary>
    private const int MaxContinuations = 4;

    private readonly Iso9660Superblock _superblock;

    /// <summary>Bytes of a record's system use area the volume's SP entry says to skip.</summary>
    private readonly int _skip;

    /// <summary>The parsed name, accumulated across NM entries.</summary>
    private readonly StringBuilder _name = new();

    /// <summary>The parsed link target, accumulated across SL entries.</summary>
    private readonly StringBuilder _target = new();

    /// <summary>True while the last SL component continues into the next SL entry.</summary>
    private bool _componentContinues;

    /// <summary>
    /// Creates a parser for one volume.
    /// </summary>
    /// <param name="superblock">The volume, for the continuation areas.</param>
    /// <param name="skip">Bytes to skip at the start of every system use area (the SP entry's skip length).</param>
    public RockRidgeParser(Iso9660Superblock superblock, int skip)
    {
        _superblock = superblock;
        _skip = skip;
    }

    /// <summary>
    /// True when <paramref name="systemUse"/>, the system use area of a
    /// root directory's first record, opens with an SP entry whose check
    /// bytes prove the volume uses SUSP. Reports the skip length it names.
    /// </summary>
    /// <param name="systemUse">The system use area.</param>
    /// <param name="skip">The bytes to skip at the start of every system use area.</param>
    public static bool HasSuspIndicator(ReadOnlySpan<byte> systemUse, out int skip)
    {
        skip = 0;
        if (systemUse.Length < Iso9660Layout.SuspHeaderLength + 3)
        {
            return false;
        }

        if (systemUse[0] != (byte)'S' || systemUse[1] != (byte)'P')
        {
            return false;
        }

        int length = systemUse[Iso9660Layout.SuspLengthOffset];
        if (length < Iso9660Layout.SuspHeaderLength + 3 || length > systemUse.Length)
        {
            return false;
        }

        ReadOnlySpan<byte> payload = systemUse.Slice(Iso9660Layout.SuspHeaderLength);
        if (BitConverter.ToUInt16(payload.Slice(Iso9660Layout.SpCheckOffset, 2)) != Iso9660Layout.SuspCheckBytes)
        {
            return false;
        }

        skip = payload[Iso9660Layout.SpSkipOffset];
        return true;
    }

    /// <summary>
    /// Parses the system use area of one directory record.
    /// </summary>
    /// <param name="systemUse">The bytes after the record's identifier and pad.</param>
    /// <param name="attributes">What Rock Ridge recorded.</param>
    /// <param name="name">The Rock Ridge name, or null when the record has no NM entry.</param>
    public void Parse(ReadOnlySpan<byte> systemUse, out RockRidgeAttributes attributes, out string? name)
    {
        attributes = default;
        _name.Clear();
        _target.Clear();
        _componentContinues = false;
        bool hasName = false;

        ReadOnlySpan<byte> area = systemUse.Length > _skip ? systemUse.Slice(_skip) : [];
        bool hasContinuation = ParseArea(area, ref attributes, ref hasName, out ulong continuationOffset, out int continuationLength);
        int continuations = 0;
        while (hasContinuation && continuations < MaxContinuations)
        {
            continuations++;
            if (!_superblock.TryReadRange(continuationOffset, continuationLength, out byte[]? continuation))
            {
                break;
            }

            hasContinuation = ParseArea(continuation, ref attributes, ref hasName, out continuationOffset, out continuationLength);
        }

        name = hasName ? _name.ToString() : null;
        if (_target.Length > 0)
        {
            attributes.SymbolicLinkTarget = _target.ToString();
        }
    }

    /// <summary>
    /// Walks one area. Returns true with the continuation's position when a
    /// CE entry named one inside the volume.
    /// </summary>
    private bool ParseArea(ReadOnlySpan<byte> area, ref RockRidgeAttributes attributes, ref bool hasName, out ulong continuationOffset, out int continuationLength)
    {
        continuationOffset = 0;
        continuationLength = 0;
        bool hasContinuation = false;

        int position = 0;
        bool ended = false;
        while (!ended && position + Iso9660Layout.SuspHeaderLength <= area.Length)
        {
            int length = area[position + Iso9660Layout.SuspLengthOffset];
            if (length < Iso9660Layout.SuspHeaderLength || position + length > area.Length)
            {
                ended = true;
                continue;
            }

            byte first = area[position];
            byte second = area[position + 1];
            ReadOnlySpan<byte> payload = area.Slice(position + Iso9660Layout.SuspHeaderLength, length - Iso9660Layout.SuspHeaderLength);

            if (first == (byte)'S' && second == (byte)'T')
            {
                // ST closes the area: nothing after it is an entry.
                ended = true;
            }
            else if (first == (byte)'C' && second == (byte)'E')
            {
                hasContinuation = TryReadContinuation(payload, out continuationOffset, out continuationLength);
            }
            else if (first == (byte)'P' && second == (byte)'X')
            {
                ReadPosixAttributes(payload, ref attributes);
            }
            else if (first == (byte)'T' && second == (byte)'F')
            {
                ReadTimeStamps(payload, ref attributes);
            }
            else if (first == (byte)'N' && second == (byte)'M')
            {
                hasName = true;
                AppendName(payload);
            }
            else if (first == (byte)'S' && second == (byte)'L')
            {
                AppendLinkComponents(payload);
            }
            else if (first == (byte)'C' && second == (byte)'L')
            {
                if (payload.Length >= Iso9660Layout.ClExtentOffset + 4)
                {
                    attributes.IsRelocationPlaceholder = true;
                    attributes.RelocatedExtent = BitConverter.ToUInt32(payload.Slice(Iso9660Layout.ClExtentOffset, 4));
                }
            }
            else if (first == (byte)'R' && second == (byte)'E')
            {
                attributes.IsRelocated = true;
            }

            position += length;
        }

        return hasContinuation;
    }

    private bool TryReadContinuation(ReadOnlySpan<byte> payload, out ulong offset, out int length)
    {
        offset = 0;
        length = 0;
        if (payload.Length < Iso9660Layout.CeLengthOffset + 4)
        {
            return false;
        }

        uint block = BitConverter.ToUInt32(payload.Slice(Iso9660Layout.CeBlockOffset, 4));
        uint offsetInBlock = BitConverter.ToUInt32(payload.Slice(Iso9660Layout.CeOffsetOffset, 4));
        uint byteLength = BitConverter.ToUInt32(payload.Slice(Iso9660Layout.CeLengthOffset, 4));
        if (byteLength == 0 || byteLength > _superblock.BlockSize || offsetInBlock >= _superblock.BlockSize)
        {
            return false;
        }

        offset = ((ulong)block * (ulong)_superblock.BlockSize) + offsetInBlock;
        length = (int)byteLength;
        return true;
    }

    private static void ReadPosixAttributes(ReadOnlySpan<byte> payload, ref RockRidgeAttributes attributes)
    {
        if (payload.Length < Iso9660Layout.PxMinLength)
        {
            return;
        }

        attributes.HasPosixAttributes = true;
        attributes.Mode = BitConverter.ToUInt32(payload.Slice(Iso9660Layout.PxModeOffset, 4));
        attributes.LinkCount = BitConverter.ToUInt32(payload.Slice(Iso9660Layout.PxLinksOffset, 4));
        attributes.Uid = BitConverter.ToUInt32(payload.Slice(Iso9660Layout.PxUidOffset, 4));
        attributes.Gid = BitConverter.ToUInt32(payload.Slice(Iso9660Layout.PxGidOffset, 4));
    }

    private static void ReadTimeStamps(ReadOnlySpan<byte> payload, ref RockRidgeAttributes attributes)
    {
        if (payload.Length < 1)
        {
            return;
        }

        byte flags = payload[Iso9660Layout.TfFlagsOffset];
        bool longForm = (flags & Iso9660Layout.TfLongForm) != 0;
        int stampLength = longForm ? Iso9660Layout.LongDateLength : Iso9660Layout.ShortDateLength;
        int position = 1;

        // The stamps present follow in a fixed order: creation, modify,
        // access, attributes.
        if ((flags & Iso9660Layout.TfCreation) != 0)
        {
            position += stampLength;
        }

        if ((flags & Iso9660Layout.TfModify) != 0)
        {
            if (TryReadStamp(payload, position, longForm, out attributes.ModificationTime))
            {
                attributes.HasModificationTime = true;
            }

            position += stampLength;
        }

        if ((flags & Iso9660Layout.TfAccess) != 0)
        {
            if (TryReadStamp(payload, position, longForm, out attributes.AccessTime))
            {
                attributes.HasAccessTime = true;
            }

            position += stampLength;
        }

        if ((flags & Iso9660Layout.TfAttributes) != 0 && TryReadStamp(payload, position, longForm, out attributes.ChangeTime))
        {
            attributes.HasChangeTime = true;
        }
    }

    private static bool TryReadStamp(ReadOnlySpan<byte> payload, int position, bool longForm, out long seconds)
    {
        seconds = 0;
        int stampLength = longForm ? Iso9660Layout.LongDateLength : Iso9660Layout.ShortDateLength;
        if (position + stampLength > payload.Length)
        {
            return false;
        }

        ReadOnlySpan<byte> stamp = payload.Slice(position, stampLength);
        seconds = longForm ? Iso9660Date.FromDescriptorDate(stamp) : Iso9660Date.FromRecordingDate(stamp);
        return true;
    }

    private void AppendName(ReadOnlySpan<byte> payload)
    {
        if (payload.Length < 1)
        {
            return;
        }

        // The current and parent flags name "." and "..", which a listed
        // entry never carries; only the bytes after the flags are the name.
        ReadOnlySpan<byte> bytes = payload.Slice(1);
        if (bytes.Length > 0)
        {
            _name.Append(Encoding.UTF8.GetString(bytes));
        }
    }

    private void AppendLinkComponents(ReadOnlySpan<byte> payload)
    {
        if (payload.Length < 1)
        {
            return;
        }

        int position = 1;
        while (position + Iso9660Layout.SlComponentHeaderLength <= payload.Length)
        {
            byte flags = payload[position];
            int length = payload[position + 1];
            if (position + Iso9660Layout.SlComponentHeaderLength + length > payload.Length)
            {
                return;
            }

            ReadOnlySpan<byte> component = payload.Slice(position + Iso9660Layout.SlComponentHeaderLength, length);
            position += Iso9660Layout.SlComponentHeaderLength + length;

            if ((flags & Iso9660Layout.SlComponentRoot) != 0)
            {
                _target.Clear();
                _target.Append('/');
                _componentContinues = false;
                continue;
            }

            if (!_componentContinues)
            {
                AppendSeparatorIfNeeded();
            }

            if ((flags & Iso9660Layout.SlComponentCurrent) != 0)
            {
                _target.Append('.');
            }
            else if ((flags & Iso9660Layout.SlComponentParent) != 0)
            {
                _target.Append("..");
            }
            else if (component.Length > 0)
            {
                _target.Append(Encoding.UTF8.GetString(component));
            }

            _componentContinues = (flags & Iso9660Layout.SlComponentContinue) != 0;
        }
    }

    private void AppendSeparatorIfNeeded()
    {
        if (_target.Length > 0 && _target[_target.Length - 1] != '/')
        {
            _target.Append('/');
        }
    }
}
