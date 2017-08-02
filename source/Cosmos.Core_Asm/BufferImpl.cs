using System;
using Cosmos.IL2CPU.API;

namespace Cosmos.Core_Asm
{
    [Plug(Target = typeof(Buffer))]
    public class BufferImpl
    {
        [PlugMethod(Assembler = typeof(BufferBlockCopyAsm))]
        public static void BlockCopy(Array src, int srcOffset, Array dst, int dstOffset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
