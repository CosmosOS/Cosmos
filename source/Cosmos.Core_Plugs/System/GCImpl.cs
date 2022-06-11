#define COSMOSDEBUG
using System;
using Cosmos.Core;
using Cosmos.Debug.Kernel;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System;

[Plug(Target = typeof(GC))]
public static class GCImpl
{
    public static void _SuppressFinalize(object o)
    {
        // not implemented yet
    }

    [PlugMethod(Signature =
        "System_Array__System_GC_AllocateNewArray_System_IntPtr__System_Int32__System_GC_GC_ALLOC_FLAGS_")]
    public static unsafe Array AllocateNewArray(int* aTypeHandle, int aLength, uint aGCFlags)
    {
        if (aGCFlags != 0 && aGCFlags != 16) // 16 means that zeroing is optional
        {
            var debugger = new Debugger("Plug", "GC");
            debugger.Send($"-- AllocateNewArray -- Invalid aGCFlags: {aGCFlags}");
            Debugger.DoBochsBreak();
            throw new NotImplementedException();
        }

        var aSize = (int)VTablesImpl.GetSize((uint)*aTypeHandle);
        return CreateNewArray(aSize, aLength);
    }

    [PlugMethod(Assembler = typeof(GCImplCreateNewArrayAsm))]
    public static Array CreateNewArray(int aSize, int aLength) =>
        throw new NotImplementedException(); // Implemented directly in ILReader.cs

    public static void _ReRegisterForFinalize(object aObject) => throw new NotImplementedException();
}
