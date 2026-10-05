// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Firmware;

/// <summary>
/// One property of a device tree node: the value's offset and length in
/// the blob, read through the tree. A default instance has no tree and
/// every reader returns false. Any context; allocation-free.
/// </summary>
internal readonly struct DeviceTreeProperty
{
    /// <summary>Builds the property over its value bytes.</summary>
    /// <param name="tree">The tree the value lives in.</param>
    /// <param name="valueOffset">The offset of the value's first byte in the blob.</param>
    /// <param name="length">The value's length in bytes.</param>
    internal DeviceTreeProperty(DeviceTree tree, uint valueOffset, uint length)
    {
        Tree = tree;
        ValueOffset = valueOffset;
        Length = length;
    }

    /// <summary>The tree the value lives in, or null for a default instance.</summary>
    internal DeviceTree? Tree { get; }

    /// <summary>The offset of the value's first byte in the blob.</summary>
    internal uint ValueOffset { get; }

    /// <summary>The value's length in bytes.</summary>
    internal uint Length { get; }

    /// <summary>Reads cell <paramref name="index"/> of the value, big-endian. Any context; allocation-free.</summary>
    /// <param name="index">The cell's index.</param>
    /// <param name="value">The cell, or 0 when the value has no such cell.</param>
    internal bool TryReadCell(int index, out uint value)
    {
        value = 0;
        if (Tree is null || index < 0)
        {
            return false;
        }

        ulong end = ((ulong)index + 1) * (ulong)DeviceTreeFormat.CellBytes;
        if (end > Length)
        {
            return false;
        }

        return Tree.TryReadCell(ValueOffset + (uint)index * (uint)DeviceTreeFormat.CellBytes, out value);
    }

    /// <summary>Whether the value is exactly <paramref name="value"/> followed by its NUL. Any context; allocation-free.</summary>
    /// <param name="value">The string to compare with.</param>
    internal bool ValueEquals(string value)
    {
        if (Tree is null)
        {
            return false;
        }

        return Length == (uint)value.Length + 1 && Tree.NameAtEquals(ValueOffset, value);
    }

    /// <summary>
    /// Whether one entry of the value, read as a list of NUL-terminated
    /// strings back to back, equals <paramref name="value"/> ordinally.
    /// Any context; allocation-free.
    /// </summary>
    /// <param name="value">The entry to look for.</param>
    internal bool ContainsString(string value)
    {
        if (Tree is null)
        {
            return false;
        }

        uint position = 0;
        while (position < Length)
        {
            // The entry runs to its NUL, or to the end of the value when
            // the last entry is unterminated.
            uint entryLength = 0;
            while (position + entryLength < Length)
            {
                if (!Tree.TryReadByte(ValueOffset + position + entryLength, out byte b))
                {
                    return false;
                }

                if (b == 0)
                {
                    break;
                }

                entryLength++;
            }

            if (entryLength == (uint)value.Length && Tree.BytesEqual(ValueOffset + position, entryLength, value))
            {
                return true;
            }

            position += entryLength + 1;
        }

        return false;
    }
}
