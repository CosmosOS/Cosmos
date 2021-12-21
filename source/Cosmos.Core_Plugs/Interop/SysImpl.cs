using IL2CPU.API.Attribs;
using System;

namespace Cosmos.Core_Plugs.Interop
{
    [Plug("Interop+Sys, System.Private.CoreLib", IsOptional = true)]
    class SysImpl
    {
        [PlugMethod(Signature = "System_IntPtr__Interop_Sys_GetUnixNamePrivate__")]
        public static IntPtr GetUnixNamePrivate()
        {
            throw new NotImplementedException();
        }
    }
}
