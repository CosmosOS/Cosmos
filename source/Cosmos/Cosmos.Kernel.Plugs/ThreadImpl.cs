using System;
using System.Collections.Generic;
using Indy.IL2CPU.Plugs;
using System.Threading;

namespace Cosmos.Kernel.Plugs {
	[Plug(Target=typeof(System.Threading.Thread))]
	public static class ThreadImpl {
		public static IntPtr InternalGetCurrentThread() {
			return IntPtr.Zero;
		}
	}
}