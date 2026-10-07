// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Devices.Network;

/// <summary>
/// A 48-bit Ethernet MAC address.
/// </summary>
public sealed class MacAddress : IComparable<MacAddress>, IEquatable<MacAddress>
{
    // Filled on first read, not by initializers: an initializer would give this type a
    // class constructor, and the type is reachable from any device bring-up path, before
    // the scheduler has a current thread for the class-constructor lock to use. No current
    // caller reads these before the scheduler exists; the lazy fill keeps it that way.
    private static MacAddress? s_broadcast;
    private static MacAddress? s_none;

    /// <summary>
    /// The broadcast address (FF:FF:FF:FF:FF:FF).
    /// </summary>
    public static MacAddress Broadcast => s_broadcast ??= new([0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF]);

    /// <summary>
    /// The all-zero address (00:00:00:00:00:00), used when no address is assigned.
    /// </summary>
    public static MacAddress None => s_none ??= new([0x00, 0x00, 0x00, 0x00, 0x00, 0x00]);

    /// <summary>
    /// The six address bytes, most significant first. Internal because the
    /// array is mutable: handing it out would let a caller rewrite an
    /// address in place and desynchronize the maps keyed on it.
    /// </summary>
    internal readonly byte[] _bytes = new byte[6];

    /// <summary>
    /// The address folded to 32 bits by <see cref="To32BitNumber"/>, kept after
    /// the first read. Not unique: two addresses can fold to the same value.
    /// </summary>
    internal uint Hash
    {
        get
        {
            if (field == 0)
            {
                field = To32BitNumber();
            }

            return field;
        }
    }

    /// <summary>
    /// Create a MAC address from a 6-byte array.
    /// </summary>
    /// <param name="address">The six address bytes, most significant first.</param>
    public MacAddress(byte[] address)
    {
        ArgumentNullException.ThrowIfNull(address);
        ArgumentOutOfRangeException.ThrowIfNotEqual(address.Length, 6, nameof(address));

        _bytes[0] = address[0];
        _bytes[1] = address[1];
        _bytes[2] = address[2];
        _bytes[3] = address[3];
        _bytes[4] = address[4];
        _bytes[5] = address[5];
    }

    /// <summary>
    /// Create a MAC address from a byte buffer starting at the specified offset.
    /// </summary>
    /// <param name="buffer">Byte buffer holding the six address bytes.</param>
    /// <param name="offset">Offset in <paramref name="buffer"/> of the most significant byte.</param>
    public MacAddress(byte[] buffer, int offset)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        ArgumentOutOfRangeException.ThrowIfLessThan(buffer.Length, offset + 6, nameof(buffer));

        _bytes[0] = buffer[offset];
        _bytes[1] = buffer[offset + 1];
        _bytes[2] = buffer[offset + 2];
        _bytes[3] = buffer[offset + 3];
        _bytes[4] = buffer[offset + 4];
        _bytes[5] = buffer[offset + 5];
    }

    /// <summary>
    /// Create a copy of an existing MAC address.
    /// </summary>
    /// <param name="m">MAC address to copy.</param>
    public MacAddress(MacAddress m)
        : this(m._bytes)
    {
    }

    /// <summary>
    /// Compare this address to another MAC address, byte by byte from the
    /// most significant byte.
    /// </summary>
    /// <param name="other">MAC address to compare against, or null.</param>
    /// <returns>Negative, zero, or positive following the ordering of the first differing byte. Null orders before any address.</returns>
    public int CompareTo(MacAddress? other)
    {
        if (other is null)
        {
            return 1;
        }

        for (int i = 0; i < 6; i++)
        {
            int order = _bytes[i].CompareTo(other._bytes[i]);
            if (order != 0)
            {
                return order;
            }
        }

        return 0;
    }

    /// <summary>
    /// Check whether another MAC address holds the same six bytes.
    /// </summary>
    /// <param name="other">MAC address to compare against, or null.</param>
    /// <returns>True when <paramref name="other"/> holds the same six bytes, false otherwise and for null.</returns>
    public bool Equals(MacAddress? other)
    {
        if (other is null)
        {
            return false;
        }

        for (int i = 0; i < 6; i++)
        {
            if (_bytes[i] != other._bytes[i])
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Check whether another object is a MAC address holding the same six bytes.
    /// </summary>
    /// <param name="obj">Object to compare against.</param>
    /// <returns>True when <paramref name="obj"/> is a <see cref="MacAddress"/> with the same six bytes, false for anything else including null.</returns>
    public override bool Equals(object? obj)
    {
        return Equals(obj as MacAddress);
    }

    /// <summary>
    /// Get a hash code derived from the six address bytes, consistent with
    /// <see cref="Equals(object?)"/>.
    /// </summary>
    /// <returns>Hash code for this address.</returns>
    public override int GetHashCode()
    {
        return (int)To32BitNumber();
    }

    /// <summary>
    /// Combine the address bytes into a single unsigned number,
    /// most significant byte first.
    /// </summary>
    /// <returns>The address as a number.</returns>
    private ulong ToNumber()
    {
        return ((ulong)_bytes[0] << 40) | ((ulong)_bytes[1] << 32) | ((ulong)_bytes[2] << 24) |
            ((ulong)_bytes[3] << 16) | ((ulong)_bytes[4] << 8) | _bytes[5];
    }

    private static void PutByte(Span<char> chars, int index, byte value)
    {
        string hexDigits = "0123456789ABCDEF";
        chars[index] = hexDigits[(value >> 4) & 0xF];
        chars[index + 1] = hexDigits[value & 0xF];
    }

    /// <summary>
    /// Fold all six address bytes into a 32-bit unsigned number. Used as the
    /// <see cref="Hash"/> value; it is a hash, not a truncation, so it does
    /// not round-trip back to an address.
    /// </summary>
    /// <returns>The folded address.</returns>
    private uint To32BitNumber()
    {
        ulong value = ToNumber();
        return (uint)value ^ (uint)(value >> 32);
    }

    /// <summary>
    /// Format the address as six colon-separated hex byte pairs
    /// (e.g. "52:54:00:12:34:56").
    /// </summary>
    /// <returns>The address in colon-separated hex notation.</returns>
    public override string ToString()
    {
        Span<char> chars = stackalloc char[17];
        PutByte(chars, 0, _bytes[0]);
        chars[2] = ':';
        PutByte(chars, 3, _bytes[1]);
        chars[5] = ':';
        PutByte(chars, 6, _bytes[2]);
        chars[8] = ':';
        PutByte(chars, 9, _bytes[3]);
        chars[11] = ':';
        PutByte(chars, 12, _bytes[4]);
        chars[14] = ':';
        PutByte(chars, 15, _bytes[5]);
        return new string(chars);
    }
}
