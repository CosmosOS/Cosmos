using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System
{
    [Plug(typeof(HashCode))]
    class HashCodeImpl
    {
        public static uint GenerateGlobalSeed()
        {
            return 0;
        }
    }
}
