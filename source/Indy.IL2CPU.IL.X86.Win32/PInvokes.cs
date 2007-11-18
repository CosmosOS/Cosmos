using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Indy.IL2CPU.IL.X86.Win32 {
	public static  class PInvokes {
		#region Heap functions
		[DllImport("kernel32.dll", EntryPoint = "HeapCreate")]
		public static extern uint Kernel32_HeapCreate(uint flOptions, uint dwInitialSize, uint dwMaximumSize);

		[DllImport("kernel32.dll", EntryPoint = "HeapDestroy")]
		public static extern bool Kernel32_HeapDestroy(uint aHeap);

		[DllImport("kernel32.dll", EntryPoint = "HeapAlloc")]
		public static extern uint Kernel32_HeapAlloc(uint hHeap, uint dwFlags, uint dwBytes);

		[DllImport("kernel32.dll", EntryPoint="HeapFree")]
		public static extern bool Kernel32_HeapFree(uint hHeap, uint dwFlags, uint lpMem);
		#endregion

		[DllImport("kernel32.dll", EntryPoint = "ExitProcess")]
		public static extern void Kernel32_ExitProcess(int aExitCode);
	}
}