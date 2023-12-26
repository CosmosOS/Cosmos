using System;

namespace Cosmos.System.Network.IPv4
{
    /// <summary>
    /// Represents an IPv4 end-point.
    /// </summary>
    public class EndPoint : IComparable
    {
        /// <summary>
        /// The address of the end-point.
        /// </summary>
        public Address Address;

        /// <summary>
        /// The port of the end-point.
        /// </summary>
        public ushort Port;

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

        public override string ToString()
        {
            return Address.ToString() + ":" + Port.ToString();
        }

        public int CompareTo(object obj)
        {
            if (obj is EndPoint other)
            {
                if (other.Address.CompareTo(Address) != 0 || other.Port != Port)
                {
                    return -1;
                }

                return 0;
            }
            else
            {
                throw new ArgumentException("'obj' is not an EndPoint instance", nameof(obj));
            }
        }
    }
}
