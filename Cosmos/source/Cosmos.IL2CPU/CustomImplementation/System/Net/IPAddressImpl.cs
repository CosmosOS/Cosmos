﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.IL2CPU.IL.CustomImplementations.System.Net {
    [Plug(Target=typeof(IPAddress))]
    public static class IPAddressImpl {
        public static string ToString(IPAddress aThis) {
            return "<IPAddress.ToString() not yet implemented!>";
        }

       // [PlugMethod(Signature = "System_Void__System_Net_IPAddress__cctor__")]
        public static void CCtor() {
            // todo: implement
        }

        public static int GetHashCode(IPAddress aThis) {
          throw new Exception("IPAddress.GetHashCode Not implemented");
        }
    }
}