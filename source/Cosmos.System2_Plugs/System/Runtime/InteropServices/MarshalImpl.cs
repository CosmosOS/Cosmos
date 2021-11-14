using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Runtime.InteropServices
{
    [Plug(typeof(global::System.Runtime.InteropServices.Marshal))]
    public static class MarshalImpl
    {
        public static void SetLastWin32Error(int aInt)
        {
            throw new NotImplementedException();
        }
    }
}
