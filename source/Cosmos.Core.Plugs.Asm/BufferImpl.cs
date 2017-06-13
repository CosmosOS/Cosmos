using System;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.Core.Plugs.Asm
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
