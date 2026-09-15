using Cosmos.Kernel.System.Network.IPv4;

namespace Cosmos.Kernel.System.Network;

/// <summary>
/// An address and port pair, of either IP version.
/// </summary>
public sealed class EndPoint : IComparable<EndPoint>, IEquatable<EndPoint>
{
    /// <summary>
    /// The address of the end-point.
    /// </summary>
    public Address Address { get; set; }

    /// <summary>
    /// The port of the end-point.
    /// </summary>
    public ushort Port { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EndPoint"/> class.
    /// </summary>
    /// <param name="addr">The address.</param>
    /// <param name="port">The port.</param>
    public EndPoint(Address addr, ushort port)
    {
        Address = addr;
        Port = port;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EndPoint"/> class.
    /// </summary>
    /// <param name="addr">The IPv4 address as a packed 32-bit value.</param>
    /// <param name="port">The port.</param>
    public EndPoint(uint addr, ushort port)
    {
        Address = new Address4(addr);
        Port = port;
    }

    /// <summary>
    /// Formats the end point as <c>address:port</c>.
    /// </summary>
    public override string ToString()
    {
        return $"{Address}:{Port}";
    }

    /// <summary>
    /// Orders end points by <see cref="Address"/> first, then by <see cref="Port"/>.
    /// A <see langword="null"/> end point sorts first.
    /// </summary>
    /// <param name="other">The end point to compare with.</param>
    /// <returns>A negative value when this end point sorts first, zero when both match, a positive value otherwise.</returns>
    public int CompareTo(EndPoint? other)
    {
        if (other is null)
        {
            return 1;
        }

        int order = Address.CompareTo(other.Address);
        if (order != 0)
        {
            return order;
        }

        return Port.CompareTo(other.Port);
    }

    /// <summary>
    /// Whether another end point carries the same address and port.
    /// </summary>
    /// <param name="other">The end point to compare with.</param>
    /// <returns>True when both the address and the port match.</returns>
    public bool Equals(EndPoint? other)
    {
        return other is not null && Port == other.Port && Address.Equals(other.Address);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return Equals(obj as EndPoint);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(Address, Port);
    }
}
