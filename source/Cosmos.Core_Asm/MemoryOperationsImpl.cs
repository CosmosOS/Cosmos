#define COSMOSDEBUG
using System;

using Cosmos.Core.Plugs_Asm.MemoryOperations;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.Core.Plugs.MemoryOperations
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
