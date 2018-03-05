using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.System.Network.IPv4
{
    public class EndPoint : IComparable
    {
        internal Address address;
        internal UInt16 port;

        public EndPoint(Address addr, UInt16 port)
        {
            this.address = addr;
            this.port = port;
        }

        public override string ToString()
        {
            return this.address.ToString() + ":" + this.port.ToString();
        }

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
