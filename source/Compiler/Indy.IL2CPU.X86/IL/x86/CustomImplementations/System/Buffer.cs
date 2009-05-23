using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.IL.X86.CustomImplementations.System {
	[Plug(Target = typeof(global::System.Buffer))]
	public class Buffer {

		[PlugMethod(Assembler = typeof(Assemblers.Buffer_BlockCopy))]
		public static void BlockCopy(Array src, int srcOffset, Array dst, int dstOffset, int count) {
		}

		public static void InternalBlockCopy(Array src, int srcOffset, Array dst, int dstOffset, int count) {
			global::System.Buffer.BlockCopy(src, srcOffset, dst, dstOffset, count);
		}
	}
}