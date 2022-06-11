using IL2CPU.API.Attribs;
using System;
using System.IO;

namespace Cosmos.System_Plugs.Interop
{
    [Plug("Interop+Sys", IsOptional = true)]
    internal class SysImpl
    {
        [PlugMethod(Signature = "System_Void__Interop_Sys_GetNonCryptographicallySecureRandomBytes_System_Byte#__System_Int32_")]
        internal static unsafe void GetNonCryptographicallySecureRandomBytes(byte* buffer, int length)
        {
            throw new NotImplementedException();
        }
    }
}
