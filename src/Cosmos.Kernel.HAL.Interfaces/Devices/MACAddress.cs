namespace Cosmos.Kernel.HAL.Interfaces.Devices;

/// <summary>
/// A 48-bit Ethernet MAC address.
/// </summary>
public sealed class MACAddress : IComparable<MACAddress>, IEquatable<MACAddress>
{
    private static MACAddress? s_broadcast;
    private static MACAddress? s_none;

    /// <summary>
    /// The broadcast address (FF:FF:FF:FF:FF:FF).
    /// </summary>
    public static MACAddress Broadcast
    {
        get
        {
            if (s_broadcast is null)
            {
                s_broadcast = new MACAddress([0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF]);
            }
            return s_broadcast;
        }
    }

    /// <summary>
    /// The all-zero address (00:00:00:00:00:00), used when no address is assigned.
    /// </summary>
    public static MACAddress None
    {
        get
        {
            if (s_none is null)
            {
                s_none = new MACAddress([0x00, 0x00, 0x00, 0x00, 0x00, 0x00]);
            }
            return s_none;
        }
    }

    /// <summary>
    /// The six address bytes, most significant first. Internal because the
    /// array is mutable: handing it out would let a caller rewrite an
    /// address in place and desynchronize the maps keyed on it.
    /// </summary>
    internal readonly byte[] _bytes = new byte[6];

    /// <summary>
    /// Create a MAC address from a 6-byte array.
    /// </summary>
    /// <param name="address">The six address bytes, most significant first.</param>
    public MACAddress(byte[] address)
    {
        if (address is null || address.Length != 6)
        {
            throw new ArgumentException("MACAddress is null or has wrong length", nameof(address));
        }

        _bytes[0] = address[0];
        _bytes[1] = address[1];
        _bytes[2] = address[2];
        _bytes[3] = address[3];
        _bytes[4] = address[4];
        _bytes[5] = address[5];

    }

    /// <summary>
    /// Create a MAC address from a byte buffer starting at the specified offset
    /// </summary>
    /// <param name="buffer">byte buffer</param>
    /// <param name="offset">offset in buffer to start from</param>
    public MACAddress(byte[] buffer, int offset)
    {
        if (buffer is null || buffer.Length < offset + 6)
        {
            throw new ArgumentException("buffer does not contain enough data starting at offset", nameof(buffer));
        }

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
    public MACAddress(MACAddress m)
        : this(m._bytes)
    {
    }


    /// <summary>
    /// Compare this address to another MAC address, byte by byte from the
    /// most significant byte.
    /// </summary>
    /// <param name="other">MAC address to compare against, or null.</param>
    /// <returns>Negative, zero, or positive following the ordering of the first differing byte. Null orders before any address.</returns>
    public int CompareTo(MACAddress? other)
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
    public bool Equals(MACAddress? other)
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
    /// <returns>True when <paramref name="obj"/> is a <see cref="MACAddress"/> with the same six bytes, false for anything else including null.</returns>
    public override bool Equals(object? obj)
    {
        return Equals(obj as MACAddress);
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

    private static void PutByte(char[] aChars, int aIndex, byte aByte)
    {
        string xChars = "0123456789ABCDEF";
        aChars[aIndex + 0] = xChars[(aByte >> 4) & 0xF];
        aChars[aIndex + 1] = xChars[aByte & 0xF];
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

    private uint _hash;
    /// <summary>
    /// Hash value for this mac. Used to uniquely identify each mac
    /// </summary>
    internal uint Hash
    {
        get
        {
            if (_hash == 0)
            {
                _hash = To32BitNumber();
            }

            return _hash;
        }
    }

    /// <summary>
    /// Format the address as six colon-separated hex byte pairs
    /// (e.g. "52:54:00:12:34:56").
    /// </summary>
    /// <returns>The address in colon-separated hex notation.</returns>
    public override string ToString()
    {
        // mac address consists of 6 2chars pairs, delimited by :
        char[] xChars = new char[17];
        PutByte(xChars, 0, _bytes[0]);
        xChars[2] = ':';
        PutByte(xChars, 3, _bytes[1]);
        xChars[5] = ':';
        PutByte(xChars, 6, _bytes[2]);
        xChars[8] = ':';
        PutByte(xChars, 9, _bytes[3]);
        xChars[11] = ':';
        PutByte(xChars, 12, _bytes[4]);
        xChars[14] = ':';
        PutByte(xChars, 15, _bytes[5]);
        return new string(xChars);
    }
}
