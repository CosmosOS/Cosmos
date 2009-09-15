using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Cosmos.IL2CPU;
using CPUx86 = Cosmos.IL2CPU.X86;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.X86.Plugs.CustomImplementations.System.Runtime.CompilerServices {
	[Plug(Target = typeof(RuntimeHelpers))]
	public static class RuntimeHelpersImpl {
//		[PlugMethod(Signature = "System_Void__System_Runtime_CompilerServices_RuntimeHelpers__cctor__")]
		public static void cctor() {
			//todo: do something
		}

		[PlugMethod(Assembler = typeof(InitializeArrayAssembler))]
		public static void InitializeArray(Array array, RuntimeFieldHandle fldHandle) {
		}
	}

	public class InitializeArrayAssembler: AssemblerMethod {
		public override void AssembleNew(object aAssembler) {
			// Arguments:
			//    Array aArray, RuntimeFieldHandle aFieldHandle
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 0xC }; // array
            new CPUx86.Move { DestinationReg = CPUx86.Registers.ESI, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 8 };// aFieldHandle
            new CPUx86.Add { DestinationReg = CPUx86.Registers.EDI, SourceValue = 8 };
			new CPUx86.Push{DestinationReg=CPUx86.Registers.EDI, DestinationIsIndirect=true};
            new CPUx86.Add { DestinationReg = CPUx86.Registers.EDI, SourceValue = 4 };
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EDI, SourceIsIndirect = true };
			new CPUx86.Multiply{DestinationReg=CPUx86.Registers.ESP, DestinationIsIndirect=true, Size=32};
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
            new CPUx86.Move { DestinationReg = CPUx86.Registers.ECX, SourceReg = CPUx86.Registers.EAX };
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
            new CPUx86.Add { DestinationReg = CPUx86.Registers.EDI, SourceValue = 4 };

			new Label(".StartLoop");
			new CPUx86.Move { DestinationReg = CPUx86.Registers.DL, SourceReg = CPUx86.Registers.ESI, SourceIsIndirect = true };
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EDI, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.DL };
			new CPUx86.Add{DestinationReg = CPUx86.Registers.EAX, SourceValue=1};
            new CPUx86.Add { DestinationReg = CPUx86.Registers.ESI, SourceValue = 1 };
            new CPUx86.Add { DestinationReg = CPUx86.Registers.EDI, SourceValue = 1 };
			new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ECX };
            new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Equal, DestinationLabel = ".EndLoop" };
            new CPUx86.Jump { DestinationLabel = ".StartLoop" };

			new Label(".EndLoop");
		}

    public override void Assemble(Indy.IL2CPU.Assembler.Assembler aAssembler) {
      throw new NotImplementedException();
    }
	}
}