using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.IL.X86.Native {
	public partial class RuntimeEngine {
		public static void Initialize() {
			RemapIRQs();
		}

		private static void RemapIRQs() {
//			WriteToPort(0x20, 0x11);
//			WriteToPort(0xA0, 0x11);
//			WriteToPort(0x21, 0x20);
//			WriteToPort(0xA1, 0x28);
//			WriteToPort(0x21, 0x04);
//			WriteToPort(0xA1, 0x02);
//			WriteToPort(0x21, 0x01);
//			WriteToPort(0xA1, 0x01);
//			WriteToPort(0x21, 0x0);
//			WriteToPort(0xA1, 0x0);
		}
	}
}