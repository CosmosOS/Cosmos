using System;
using Cosmos.Core;
using Cosmos.Debug.Kernel;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System
{
    [Plug("System.Signature, System.Private.CoreLib")]
    public class SignatureImpl
    {
        [PlugMethod(Signature = "System_Void__System_Signature_GetSignature_System_Void#__System_Int32__System_RuntimeFieldHandleInternal__System_IRuntimeMethodInfo__System_RuntimeType_")]
        public static unsafe void GetSignature(object aThis, 
            void* pCorSig, int cCorSig,
            object fieldHandle, object methodHandle, object declaringType)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Boolean__System_Signature_CompareSig_System_Signature__System_Signature_")]
        public static unsafe bool CompareSig(object sig1, object sig2)
        {
            throw new NotImplementedException();
        }
    }
}