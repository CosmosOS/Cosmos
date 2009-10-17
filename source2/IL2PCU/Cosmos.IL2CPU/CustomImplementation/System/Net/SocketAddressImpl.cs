using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.IL2CPU.IL.CustomImplementations.System.Net {
    [Plug(Target=typeof(SocketAddress))]
    public static class SocketAddressImpl {
        public static string ToString(SocketAddress aThis) {
            return "<SocketAddress.ToString() not yet plugged correctly!>";
        }
    }
}