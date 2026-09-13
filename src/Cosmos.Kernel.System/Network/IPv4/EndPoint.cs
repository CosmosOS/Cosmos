namespace Cosmos.Kernel.System.Network.IPv4;

/// <summary>
/// Represents an IPv4 end-point.
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
    /// <param name="addr">The IPv4 address.</param>
    /// <param name="port">The port.</param>
    public EndPoint(Address addr, ushort port)
    {
        Address = addr;
        Port = port;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EndPoint"/> class.
    /// </summary>
    /// <param name="addr">The IPv4 address.</param>
    /// <param name="port">The port.</param>
    public EndPoint(uint addr, ushort port)
    {
        Address = new Address(addr);
        Port = port;
    }

    /// <summary>
    /// Formats the end point as <c>address:port</c>.
    /// </summary>
    public override string ToString()
    {
        return Address.ToString() + ":" + Port.ToString();
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
