using IL2CPU.API.Attribs;
using System;
using System.IO;

namespace Cosmos.System_Plugs.Interop
{
    [Plug("Interop+Sys, System.Private.CoreLib")]
    class SysImpl
    {
        [PlugMethod(Signature = "System_Void__Interop_Sys_GetNonCryptographicallySecureRandomBytes_System_Byte___System_Int32_")]
        public static unsafe void GetNonCryptographicallySecureRandomBytes(byte* buffer, int length)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Int32__Interop_Sys_LChflagsCanSetHiddenFlag__")]
        public static int LChflagsCanSetHiddenFlag()
        {
            throw new NotImplementedException();
        }
    }
}
