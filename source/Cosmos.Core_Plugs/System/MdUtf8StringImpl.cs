using System;
using Cosmos.Core;
using Cosmos.Debug.Kernel;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System
{
    [Plug("System.MdUtf8String, System.Private.CoreLib")]
    public class MdUtf8StringImpl
    {
        [PlugMethod(Signature = "System_Boolean__System_MdUtf8String__EqualsCaseInsensitive_System_Void#__System_Void#__System_Int32_")]
        public static unsafe bool EqualsCaseInsensitive(void* szLhs, void* szRhs, int cSz)
        {
            throw new NotImplementedException();
        }
    }
}