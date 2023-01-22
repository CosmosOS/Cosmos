using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.Interop
{
    [Plug("Interop+BCrypt, System.Private.CoreLib", IsOptional = true)]
    class BCryptImpl
    {
        [PlugMethod(Signature = "Interop_BCrypt_NTSTATUS__Interop_BCrypt_BCryptGenRandom_System_IntPtr__System_Byte___System_Int32__System_Int32_")]
        public static unsafe int BCryptGenRandom(IntPtr hAlgorithm, byte* pbBuffer, int cbBuffer, int dwFlags)
        {
            throw new NotImplementedException();
        }
    }
}
