using System;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System
{
    [Plug(Target=typeof(GC))]
    public static class GCImpl
    {
        public static void _SuppressFinalize(object o) {
        // not implemented yet
        }

        [PlugMethod(Signature = "System_Array__System_GC_AllocateNewArray_System_IntPtr__System_Int32__System_GC_GC_ALLOC_FLAGS_")]
        public static unsafe Array AllocateNewArray(int* aTypeHandle, int aLength, uint aGCFlags)
        {
            throw new NotImplementedException();
        }

        public static void _ReRegisterForFinalize(object aObject)
        {
            throw new NotImplementedException();
        }
    }
}
