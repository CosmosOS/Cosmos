using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace SimplePInvokeTest {
	class Program {
		static void Main() {
			bool result = MessageBeep(0xFFFFFFFF);
			uint error = GetLastError();
		}

		[DllImport("user32.dll")]
		private static extern bool MessageBeep(uint aType);

		[DllImport("kernel32.dll")]
		private static extern uint GetLastError();
	}
}
