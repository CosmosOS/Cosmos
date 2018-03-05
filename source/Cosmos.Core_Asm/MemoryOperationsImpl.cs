#define COSMOSDEBUG
using System;
using Cosmos.Core_Asm.MemoryOperations;
using IL2CPU.API;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.MemoryOperations
{
    [Plug(Target = typeof(Cosmos.Core.MemoryOperations))]
    public unsafe class MemoryOperationsImpl
    {
        [PlugMethod(Assembler = typeof(MemoryOperationsFill16BlocksAsm))]
        public static unsafe void Fill16Blocks(byte* dest, int value, int BlocksNum)
        {
            throw new NotImplementedException();
        }
    }
}
