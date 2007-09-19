using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Indy.IL2CPU {
	public static class PInvokes {
		[DllImport("kernel32.dll", EntryPoint = "ExitProcess")]
		public static extern void Kernel32_ExitProcess(uint uExitCode);
		[DllImport("kernel32.dll", EntryPoint = "GetLastError")]
		public static extern uint Kernel32_GetLastError();

		[DllImport("kernel32.dll")]
		public static extern void DebugBreak();

		#region Heap functions
		[DllImport("kernel32.dll", EntryPoint = "HeapCreate")]
		public static extern IntPtr Kernel32_HeapCreate(uint flOptions, uint dwInitialSize, uint dwMaximumSize);

		[DllImport("kernel32.dll", EntryPoint = "HeapDestroy")]
		public static extern bool Kernel32_HeapDestroy(IntPtr aHeap);

		[DllImport("kernel32.dll", EntryPoint = "HeapAlloc")]
		public static extern IntPtr Kernel32_HeapAlloc(IntPtr hHeap, uint dwFlags, uint dwBytes);
		#endregion

	}
}
