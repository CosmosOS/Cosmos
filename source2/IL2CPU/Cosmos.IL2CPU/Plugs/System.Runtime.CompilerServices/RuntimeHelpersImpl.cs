using System;
using System.Runtime.CompilerServices;
using Cosmos.Assembler;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.IL2CPU.X86.Plugs.CustomImplementations.System.Runtime.CompilerServices {
	[Plug(Target = typeof(RuntimeHelpers))]
	public static class RuntimeHelpersImpl {

		public static void cctor() {
			//TODO: do something
		}

		[InlineAttribute(TargetPlatform = TargetPlatform.x86)]
		public static void InitializeArray(Array array, RuntimeFieldHandle fldHandle) {
			// Arguments:
			//    Array aArray, RuntimeFieldHandle aFieldHandle
            new CPUx86.Mov { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 0xC }; // array
            new CPUx86.Mov { DestinationReg = CPUx86.Registers.ESI, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 8 };// aFieldHandle
            new CPUx86.Add { DestinationReg = CPUx86.Registers.EDI, SourceValue = 8 };
			new CPUx86.Push{DestinationReg=CPUx86.Registers.EDI, DestinationIsIndirect=true};
            new CPUx86.Add { DestinationReg = CPUx86.Registers.EDI, SourceValue = 4 };
            new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EDI, SourceIsIndirect = true };
			new CPUx86.Multiply{DestinationReg=CPUx86.Registers.ESP, DestinationIsIndirect=true, Size=32};
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
            new CPUx86.Mov { DestinationReg = CPUx86.Registers.ECX, SourceReg = CPUx86.Registers.EAX };
            new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
            new CPUx86.Add { DestinationReg = CPUx86.Registers.EDI, SourceValue = 4 };

			new Label(".StartLoop");
			new CPUx86.Mov { DestinationReg = CPUx86.Registers.DL, SourceReg = CPUx86.Registers.ESI, SourceIsIndirect = true };
            new CPUx86.Mov { DestinationReg = CPUx86.Registers.EDI, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.DL };
			new CPUx86.Add{DestinationReg = CPUx86.Registers.EAX, SourceValue=1};
            new CPUx86.Add { DestinationReg = CPUx86.Registers.ESI, SourceValue = 1 };
            new CPUx86.Add { DestinationReg = CPUx86.Registers.EDI, SourceValue = 1 };
			new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ECX };
            new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Equal, DestinationLabel = ".EndLoop" };
            new CPUx86.Jump { DestinationLabel = ".StartLoop" };

			new Label(".EndLoop");
		}
	}
}