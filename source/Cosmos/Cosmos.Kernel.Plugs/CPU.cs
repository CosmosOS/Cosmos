using System;
using System.Collections.Generic;
using System.Text;
using Indy.IL2CPU.Plugs;
using HW = Cosmos.Hardware;

namespace Cosmos.Kernel.Plugs {
    [Plug(Target = typeof(HW.CPUOld))]
    public static class CPUOld {
		[PlugMethod(MethodAssembler = typeof(Assemblers.CreateGDT))]
		public static void CreateGDT() {
		}

		[PlugMethod(MethodAssembler = typeof(Assemblers.CreateIDT))]
		public static void CreateIDT() {
		}

		[PlugMethod(MethodAssembler = typeof(Assemblers.ZeroFill))]
		// TODO: implement this using REP STOSB and REPO STOSD
		public static unsafe void ZeroFill(uint aStartAddress, uint aLength) {
			Console.Write("Clearing ");
			Cosmos.Hardware.Storage.ATAOld.WriteNumber(aLength, 32);
			Console.Write(" bytes at ");
			Cosmos.Hardware.Storage.ATAOld.WriteNumber(aStartAddress, 32);
			Console.WriteLine("");
			uint* xPtr = (uint*)aStartAddress;
			for (uint i = 0; i < (aLength / 4); i++) {
				xPtr[i] = 0;
				if (i % (256 * 1024) == 0) {
					Console.Write("Cleared Megabyte ");
					Cosmos.Hardware.Storage.ATAOld.WriteNumber(i / (256 * 1024), 16);
					Console.WriteLine();
				}
			}
		}
	}
}
