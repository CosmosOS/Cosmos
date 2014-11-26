using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.IL2CPU.X86.Plugs.CustomImplementations.System {
	[Plug(Target = typeof(global::System.Buffer))]
	public class Buffer {

        public static unsafe void __Memcpy(byte* src, byte* dest, int count)
        {
            global::System.Buffer.BlockCopy((Array)(object)*src, 0, (Array)(object)*dest, 0, count);
        }

		[PlugMethod(Assembler = typeof(Assemblers.Buffer_BlockCopy))]
		public static void BlockCopy(Array src, int srcOffset, Array dst, int dstOffset, int count) {
		}

		public static void InternalBlockCopy(Array src, int srcOffset, Array dst, int dstOffset, int count) {
			global::System.Buffer.BlockCopy(src, srcOffset, dst, dstOffset, count);
		}
	}
}