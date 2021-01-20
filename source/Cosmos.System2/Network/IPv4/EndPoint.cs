/*
* PROJECT:          Aura Operating System Development
* CONTENT:          End point
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

using System;

namespace Cosmos.System.Network.IPv4
{
    /// <summary>
    /// EndPoint class.
    /// </summary>
    public class EndPoint : IComparable
    {
        /// <summary>
        /// Address.
        /// </summary>
        public Address address;
        /// <summary>
        /// Port.
        /// </summary>
        public UInt16 port;

        /// <summary>
        /// Create new instance of the <see cref="EndPoint"/> class.
        /// </summary>
        /// <param name="addr">Adress.</param>
        /// <param name="port">Port.</param>
        public EndPoint(Address addr, UInt16 port)
        {
            this.address = addr;
            this.port = port;
        }

        /// <summary>
        /// To string.
        /// </summary>
        /// <returns>string value.</returns>
        public override string ToString()
        {
            return this.address.ToString() + ":" + this.port.ToString();
        }

        /// <summary>
        /// Compare end points.
        /// </summary>
        /// <param name="obj">Other end point to compare to.</param>
        /// <returns>-1 if end points are diffrent, 0 if equal.</returns>
        /// <exception cref="ArgumentException">Thrown if obj is not a EndPoint.</exception>
        public int CompareTo(object obj)
        {
            if (obj is EndPoint)
            {
                EndPoint other = (EndPoint)obj;
                if ((other.address.CompareTo(this.address) != 0) ||
                    (other.port != this.port))
                {
                    return -1;
                }

                return 0;
            }
            else
                throw new ArgumentException("obj is not a IPv4EndPoint", "obj");
        }
    }
}
