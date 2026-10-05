// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.Core.IO;

namespace Cosmos.Kernel.HAL.Firmware;

/// <summary>
/// A flattened device tree blob read in place: the header validated once
/// by <see cref="TryOpen"/>, then every read bounded by the header's block
/// offsets and sizes and never past the blob's total size. Cells are
/// assembled big-endian from bytes. Nothing here allocates on a walk but
/// the two diagnostic string readers; a malformed structure block ends
/// the walk that met it (<see cref="MarkWalkStopped"/>) and never faults.
/// </summary>
internal sealed unsafe class DeviceTree
{
    /// <summary>The blob's first byte, a virtual address the bootloader handed over.</summary>
    private readonly byte* _blob;

    /// <summary>Bytes of the whole blob, from the header.</summary>
    private readonly uint _totalSize;

    /// <summary>Offset of the structure block.</summary>
    private readonly uint _structOffset;

    /// <summary>Bytes of the structure block.</summary>
    private readonly uint _structSize;

    /// <summary>Offset of the strings block.</summary>
    private readonly uint _stringsOffset;

    /// <summary>Bytes of the strings block.</summary>
    private readonly uint _stringsSize;

    /// <summary>The first offset past the structure block.</summary>
    private readonly uint _structEnd;

    /// <summary>The phandle the last scan looked for.</summary>
    private uint _cachedPhandle;

    /// <summary>A scan ran for <see cref="_cachedPhandle"/>; false until the first scan, so phandle 0 is not mistaken for a cached answer.</summary>
    private bool _cacheValid;

    /// <summary>That scan's answer: true with <see cref="_cachedInterruptCells"/>, false when the node was not found, carried no #interrupt-cells, or the scan stopped.</summary>
    private bool _cachedResolved;

    /// <summary>The cells of the cached answer, 0 when <see cref="_cachedResolved"/> is false.</summary>
    private uint _cachedInterruptCells;

    /// <summary>True once a walk met an unknown token.</summary>
    private bool _walkStopped;

    /// <summary>Builds the tree over header values <see cref="TryOpen"/> validated.</summary>
    private DeviceTree(byte* blob, uint totalSize, uint structOffset, uint structSize, uint stringsOffset, uint stringsSize)
    {
        _blob = blob;
        _totalSize = totalSize;
        _structOffset = structOffset;
        _structSize = structSize;
        _stringsOffset = stringsOffset;
        _stringsSize = stringsSize;
        _structEnd = structOffset + structSize;
    }

    /// <summary>
    /// Validates the header of the blob at <paramref name="address"/>, a
    /// virtual address the bootloader handed over; the blob is read in
    /// place and never copied. Boot thread; allocates the tree object and,
    /// on refusal, the reason.
    /// </summary>
    /// <param name="address">The blob's first byte.</param>
    /// <param name="tree">The tree, or null when the header was refused.</param>
    /// <param name="reason">Why the header was refused, or null.</param>
    internal static bool TryOpen(void* address, [NotNullWhen(true)] out DeviceTree? tree, [NotNullWhen(false)] out string? reason)
    {
        tree = null;
        reason = null;
        if (address == null)
        {
            reason = "no address";
            return false;
        }

        byte* blob = (byte*)address;
        uint magic = ReadCellRaw(blob, DeviceTreeFormat.MagicOffset);
        if (magic != DeviceTreeFormat.Magic)
        {
            reason = $"bad magic 0x{magic:X8}";
            return false;
        }

        uint totalSize = ReadCellRaw(blob, DeviceTreeFormat.TotalSizeOffset);
        uint structOffset = ReadCellRaw(blob, DeviceTreeFormat.StructOffset);
        uint stringsOffset = ReadCellRaw(blob, DeviceTreeFormat.StringsOffset);
        uint version = ReadCellRaw(blob, DeviceTreeFormat.VersionOffset);
        uint lastCompatibleVersion = ReadCellRaw(blob, DeviceTreeFormat.LastCompatibleVersionOffset);
        if (version < DeviceTreeFormat.MinimumVersion || lastCompatibleVersion > DeviceTreeFormat.MaximumLastCompatibleVersion)
        {
            reason = $"unsupported version {version} (last compatible {lastCompatibleVersion})";
            return false;
        }

        uint stringsSize = ReadCellRaw(blob, DeviceTreeFormat.StringsSizeOffset);
        uint structSize = ReadCellRaw(blob, DeviceTreeFormat.StructSizeOffset);
        if (totalSize < (uint)DeviceTreeFormat.HeaderBytes || (ulong)structOffset + structSize > totalSize)
        {
            reason = "struct block outside the blob";
            return false;
        }

        if ((ulong)stringsOffset + stringsSize > totalSize)
        {
            reason = "strings block outside the blob";
            return false;
        }

        DeviceTree candidate = new(blob, totalSize, structOffset, structSize, stringsOffset, stringsSize);
        if (!candidate.TryReadToken(structOffset, out uint token) || token != DeviceTreeFormat.TokenBeginNode)
        {
            reason = "no root node";
            return false;
        }

        tree = candidate;
        return true;
    }

    /// <summary>The blob's virtual address. Any context.</summary>
    internal ulong Address => (ulong)_blob;

    /// <summary>Bytes of the whole blob, from the header. Any context.</summary>
    internal uint TotalSize => _totalSize;

    /// <summary>The format version the header names. Any context; allocation-free.</summary>
    internal uint Version => ReadCellRaw(_blob, DeviceTreeFormat.VersionOffset);

    /// <summary>True once a walk met an unknown token; the log line of <see cref="MarkWalkStopped"/> is written once. Any context.</summary>
    internal bool WalkStopped => _walkStopped;

    /// <summary>
    /// The root cursor, with the default cell counts the root's own reg
    /// would use and the interrupt cells of its interrupt parent. Built on
    /// each read; the phandle scan's answer is cached after the first.
    /// Thread context; allocation-free.
    /// </summary>
    internal DeviceTreeNode Root
    {
        get
        {
            uint interruptCells = 0;
            DeviceTreeNode probe = new(this, _structOffset, DeviceTreeFormat.DefaultAddressCells, DeviceTreeFormat.DefaultSizeCells, 0, 0);
            if (probe.TryGetProperty("interrupt-parent", out DeviceTreeProperty parent)
                && parent.TryReadCell(0, out uint phandle)
                && TryResolveInterruptCells(phandle, out uint cells))
            {
                interruptCells = cells;
            }

            return new DeviceTreeNode(this, _structOffset, DeviceTreeFormat.DefaultAddressCells, DeviceTreeFormat.DefaultSizeCells, interruptCells, interruptCells);
        }
    }

    /// <summary>Reads the big-endian cell at <paramref name="offset"/>. Any context; allocation-free.</summary>
    /// <param name="offset">The cell's offset in the blob.</param>
    /// <param name="value">The cell, or 0 when it runs past the blob.</param>
    internal bool TryReadCell(uint offset, out uint value)
    {
        if ((ulong)offset + (uint)DeviceTreeFormat.CellBytes > _totalSize)
        {
            value = 0;
            return false;
        }

        value = ReadCellRaw(_blob, offset);
        return true;
    }

    /// <summary>Reads the byte at <paramref name="offset"/>. Any context; allocation-free.</summary>
    /// <param name="offset">The byte's offset in the blob.</param>
    /// <param name="value">The byte, or 0 when it lies past the blob.</param>
    internal bool TryReadByte(uint offset, out byte value)
    {
        if (offset >= _totalSize)
        {
            value = 0;
            return false;
        }

        value = _blob[offset];
        return true;
    }

    /// <summary>
    /// Reads the token at <paramref name="offset"/>: the cell when it lies
    /// inside the structure block, <see cref="DeviceTreeFormat.TokenEnd"/>
    /// when the offset is at or past the block's end (a truncated block
    /// ends a walk the way a well-formed one does), false only when the
    /// offset overflows. Any context; allocation-free.
    /// </summary>
    /// <param name="offset">The token's offset in the blob.</param>
    /// <param name="token">The token.</param>
    internal bool TryReadToken(uint offset, out uint token)
    {
        if (offset > uint.MaxValue - (uint)DeviceTreeFormat.TokenBytes)
        {
            token = 0;
            return false;
        }

        if (offset + (uint)DeviceTreeFormat.TokenBytes > _structEnd)
        {
            token = DeviceTreeFormat.TokenEnd;
            return true;
        }

        token = ReadCellRaw(_blob, offset);
        return true;
    }

    /// <summary>
    /// The offset of the first token after the NUL-terminated name that
    /// starts at <paramref name="nameOffset"/>, padded to a cell; the
    /// structure block's end when the name runs past it, so the next token
    /// read ends the walk. Any context; allocation-free.
    /// </summary>
    /// <param name="nameOffset">The name's first byte, right after a begin-node token.</param>
    internal uint SkipName(uint nameOffset)
    {
        uint length = 0;
        while (true)
        {
            ulong position = (ulong)nameOffset + length;
            if (position >= _structEnd)
            {
                return _structEnd;
            }

            if (_blob[position] == 0)
            {
                break;
            }

            length++;
        }

        ulong body = nameOffset + (ulong)DeviceTreeFormat.AlignUp(length + 1);
        return body > _structEnd ? _structEnd : (uint)body;
    }

    /// <summary>Whether the NUL-terminated bytes at <paramref name="offset"/> equal <paramref name="name"/> ordinally; false when they run past the blob. Any context; allocation-free.</summary>
    /// <param name="offset">The first byte of the name in the blob.</param>
    /// <param name="name">The name to compare with.</param>
    internal bool NameAtEquals(uint offset, string name)
    {
        if (!BytesEqual(offset, (uint)name.Length, name))
        {
            return false;
        }

        return TryReadByte(offset + (uint)name.Length, out byte terminator) && terminator == 0;
    }

    /// <summary>Whether exactly <paramref name="length"/> bytes at <paramref name="offset"/> equal <paramref name="value"/> ordinally; false when they run past the blob. Any context; allocation-free.</summary>
    /// <param name="offset">The first byte in the blob.</param>
    /// <param name="length">How many bytes to compare.</param>
    /// <param name="value">The string to compare with.</param>
    internal bool BytesEqual(uint offset, uint length, string value)
    {
        if (length != (uint)value.Length || (ulong)offset + length > _totalSize)
        {
            return false;
        }

        for (uint i = 0; i < length; i++)
        {
            char c = value[(int)i];
            if (c > 0xFF || _blob[offset + i] != (byte)c)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>Whether the strings block entry at <paramref name="nameOffset"/> equals <paramref name="name"/>. Any context; allocation-free.</summary>
    /// <param name="nameOffset">The offset into the strings block.</param>
    /// <param name="name">The property name to compare with.</param>
    internal bool StringsNameEquals(uint nameOffset, string name)
    {
        if (nameOffset >= _stringsSize)
        {
            return false;
        }

        return NameAtEquals(_stringsOffset + nameOffset, name);
    }

    /// <summary>The NUL-terminated string at <paramref name="offset"/> as a managed string, null when it runs past the blob. For diagnostics only, never on the publish path. Thread context; allocates.</summary>
    /// <param name="offset">The first byte in the blob.</param>
    internal string? ReadName(uint offset)
    {
        uint length = 0;
        while (true)
        {
            if (!TryReadByte(offset + length, out byte b))
            {
                return null;
            }

            if (b == 0)
            {
                break;
            }

            length++;
        }

        char[] chars = new char[length];
        for (uint i = 0; i < length; i++)
        {
            chars[i] = (char)_blob[offset + i];
        }

        return new string(chars);
    }

    /// <summary>The strings block entry at <paramref name="nameOffset"/> as a managed string, null when it runs past the block. For diagnostics only. Thread context; allocates.</summary>
    /// <param name="nameOffset">The offset into the strings block.</param>
    internal string? ReadStringsName(uint nameOffset)
    {
        if (nameOffset >= _stringsSize)
        {
            return null;
        }

        return ReadName(_stringsOffset + nameOffset);
    }

    /// <summary>
    /// The <c>#interrupt-cells</c> of the node whose <c>phandle</c> (or
    /// <c>linux,phandle</c>) equals <paramref name="phandle"/>: one linear
    /// scan of the structure block's tokens, its answer cached so the
    /// nodes that share an interrupt parent cost one scan. The scan never
    /// goes through cursors or <see cref="Root"/>, which resolve through
    /// this member. Thread context; allocation-free.
    /// </summary>
    /// <param name="phandle">The phandle to look for.</param>
    /// <param name="cells">The cells, or 0 when unresolved.</param>
    internal bool TryResolveInterruptCells(uint phandle, out uint cells)
    {
        if (_cacheValid && phandle == _cachedPhandle)
        {
            cells = _cachedInterruptCells;
            return _cachedResolved;
        }

        bool resolved = false;
        cells = 0;
        Span<uint> open = stackalloc uint[DeviceTreeFormat.MaxDepth];
        int depth = 0;
        uint offset = _structOffset;
        while (true)
        {
            if (!TryReadToken(offset, out uint token))
            {
                break;
            }

            if (token == DeviceTreeFormat.TokenBeginNode)
            {
                if (depth >= DeviceTreeFormat.MaxDepth)
                {
                    break;
                }

                open[depth] = offset;
                depth++;
                offset = SkipName(offset + (uint)DeviceTreeFormat.TokenBytes);
            }
            else if (token == DeviceTreeFormat.TokenEndNode)
            {
                if (depth == 0)
                {
                    break;
                }

                depth--;
                offset += (uint)DeviceTreeFormat.TokenBytes;
            }
            else if (token == DeviceTreeFormat.TokenNop)
            {
                offset += (uint)DeviceTreeFormat.TokenBytes;
            }
            else if (token == DeviceTreeFormat.TokenProperty)
            {
                if (!TryReadCell(offset + (uint)DeviceTreeFormat.TokenBytes, out uint length)
                    || !TryReadCell(offset + (uint)DeviceTreeFormat.TokenBytes + (uint)DeviceTreeFormat.CellBytes, out uint nameOffset))
                {
                    break;
                }

                uint valueOffset = offset + (uint)DeviceTreeFormat.TokenBytes + (uint)DeviceTreeFormat.PropertyHeaderBytes;
                if (depth > 0
                    && length >= (uint)DeviceTreeFormat.CellBytes
                    && (StringsNameEquals(nameOffset, "phandle") || StringsNameEquals(nameOffset, "linux,phandle"))
                    && TryReadCell(valueOffset, out uint candidate)
                    && candidate == phandle)
                {
                    DeviceTreeNode match = new(this, open[depth - 1], 0, 0, 0, 0);
                    resolved = match.TryGetProperty("#interrupt-cells", out DeviceTreeProperty property) && property.TryReadCell(0, out cells);
                    break;
                }

                ulong next = valueOffset + DeviceTreeFormat.AlignUp((ulong)length);
                if (next > uint.MaxValue)
                {
                    break;
                }

                offset = (uint)next;
            }
            else if (token == DeviceTreeFormat.TokenEnd)
            {
                break;
            }
            else
            {
                MarkWalkStopped(token, offset);
                break;
            }
        }

        if (!resolved)
        {
            cells = 0;
        }

        _cachedPhandle = phandle;
        _cacheValid = true;
        _cachedResolved = resolved;
        _cachedInterruptCells = cells;
        return resolved;
    }

    /// <summary>
    /// Records that a walk met an unknown token: every cursor read returns
    /// false from here, the walk never faults, and the machine description
    /// goes on with what it has. The line is written once, number by
    /// number, so the walks that call this stay allocation-free. Any
    /// context; allocation-free.
    /// </summary>
    /// <param name="token">The token read.</param>
    /// <param name="offset">Where it was read.</param>
    internal void MarkWalkStopped(uint token, uint offset)
    {
        if (_walkStopped)
        {
            return;
        }

        _walkStopped = true;
        Serial.WriteString("[Firmware] Device tree walk stopped: unknown token 0x");
        Serial.WriteHex(token);
        Serial.WriteString(" at offset 0x");
        Serial.WriteHex(offset);
        Serial.WriteString("\n");
    }

    /// <summary>
    /// The big-endian cell at <paramref name="offset"/>, unbounded: for the
    /// header, whose forty bytes <see cref="TryOpen"/> reads before any size
    /// is known and before the tree object exists (hence the blob
    /// parameter), and for the bounded readers once their own check passed.
    /// Any context; allocation-free.
    /// </summary>
    /// <param name="blob">The blob's first byte.</param>
    /// <param name="offset">The cell's offset in the blob.</param>
    private static uint ReadCellRaw(byte* blob, uint offset)
    {
        byte* cell = blob + offset;
        return ((uint)cell[0] << 24) | ((uint)cell[1] << 16) | ((uint)cell[2] << 8) | cell[3];
    }
}
