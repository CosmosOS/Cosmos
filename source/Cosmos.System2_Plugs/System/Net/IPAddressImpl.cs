using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Net
{
    [Plug(Target = typeof(IPAddress))]
    public static class IPAddressImpl
    {
        public static void Ctor(IPAddress aThis, ReadOnlySpan<byte> address, [FieldAccess(Name = "System.Net.IPAddress System.Net.IPAddress.Any")] ref IPAddress aAny)
        {
            Cosmos.HAL.Global.debugger.Send("IPAddress - ctor.");

            

            Cosmos.HAL.Global.debugger.Send("IPAddress - aAny set.");
        }
    }
}
