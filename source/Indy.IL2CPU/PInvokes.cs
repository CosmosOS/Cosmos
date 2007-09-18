using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Indy.IL2CPU {
	public class PInvokes {
		[DllImport("kernel32.dll", EntryPoint = "ExitProcess")]
		static extern void Kernel32_ExitProcess(uint uExitCode);
	}
}
