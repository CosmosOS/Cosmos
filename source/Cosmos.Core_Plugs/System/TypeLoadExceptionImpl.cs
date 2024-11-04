using Cosmos.Core;
using Cosmos.Debug.Kernel;
using IL2CPU.API.Attribs;
using System;
using System.Runtime.CompilerServices;

namespace Cosmos.Core_Plugs.System
{
    [Plug(Target = typeof(TypeLoadException))]
    public unsafe class TypeLoadExceptionImpl
    {
        [PlugMethod(Signature = "System_Void__System_TypeLoadException_GetTypeLoadExceptionMessage_System_Int32__System_Runtime_CompilerServices_StringHandleOnStack_")]
        public static void GetTypeLoadExceptionMessage(int resourceId, object retString)
        {
            throw new NotImplementedException();
        }
    }
}