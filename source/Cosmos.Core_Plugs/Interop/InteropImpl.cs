using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.Interop
{
    [Plug("Interop, System.Private.CoreLib")]
    class InteropImpl
    {
        public static unsafe void GetRandomBytes(byte* aBuffer, int aLength)
        {
            throw new NotImplementedException();
        }
    }
}
