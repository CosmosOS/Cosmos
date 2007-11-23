using System;
using System.Collections.Generic;
using System.Text;
using Indy.IL2CPU.Plugs;
using HW = Cosmos.Hardware;

namespace Cosmos.Kernel.Plugs {
	[Plug(Target = typeof(HW.CPU))]
	public static class CPU {
		[PlugMethod(MethodAssembler=typeof(Assemblers.CreateGDT))]
		public static void CreateGDT() { }

        [PlugMethod(MethodAssembler = typeof(Assemblers.CreateIDT))]
        public static void CreateIDT() { }
	}
}
