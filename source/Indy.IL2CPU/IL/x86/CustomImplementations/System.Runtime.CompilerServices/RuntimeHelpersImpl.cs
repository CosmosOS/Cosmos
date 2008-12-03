using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.IL.X86.CustomImplementations.System.Runtime.CompilerServices {
	[Plug(Target = typeof(RuntimeHelpers))]
	public static class RuntimeHelpersImpl {
		[PlugMethod(Signature = "System_Void__System_Runtime_CompilerServices_RuntimeHelpers__cctor__")]
		public static void CCtor() {
			//todo: do something
		}

		[PlugMethod(Assembler = typeof(InitializeArrayAssembler))]
		public static void InitializeArray(Array array, RuntimeFieldHandle fldHandle) {
		}
	}

	public class InitializeArrayAssembler: AssemblerMethod {
		public override void Assemble(Indy.IL2CPU.Assembler.Assembler aAssembler) {
			// Arguments:
			//    Array aArray, RuntimeFieldHandle aFieldHandle
            new Assembler.X86.Move { DestinationReg = Assembler.X86.Registers.EDI, SourceReg = Assembler.X86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 0xC }; // array
            new Assembler.X86.Move { DestinationReg = Assembler.X86.Registers.ESI, SourceReg = Assembler.X86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 8 };// aFieldHandle
            new Assembler.X86.Add { DestinationReg = Assembler.X86.Registers.EDI, SourceValue = 8 };
			new Assembler.X86.Push{DestinationReg=Assembler.X86.Registers.EDI, DestinationIsIndirect=true};
            new Assembler.X86.Add { DestinationReg = Assembler.X86.Registers.EDI, SourceValue = 4 };
            new Assembler.X86.Move { DestinationReg = Assembler.X86.Registers.EAX, SourceReg = Assembler.X86.Registers.EDI, SourceIsIndirect = true };
			new Assembler.X86.Multiply{DestinationReg=Assembler.X86.Registers.ESP, DestinationIsIndirect=true, Size=32};
            new Assembler.X86.Pop { DestinationReg = Assembler.X86.Registers.ECX };
            new Assembler.X86.Move { DestinationReg = Assembler.X86.Registers.ECX, SourceReg = Assembler.X86.Registers.EAX };
            new Assembler.X86.Move { DestinationReg = Assembler.X86.Registers.EAX, SourceValue = 0 };
            new Assembler.X86.Add { DestinationReg = Assembler.X86.Registers.EDI, SourceValue = 4 };

			new Assembler.Label(".StartLoop");
			new Assembler.X86.Move { DestinationReg = Assembler.X86.Registers.DL, SourceReg = Assembler.X86.Registers.ESI, SourceIsIndirect = true };
            new Assembler.X86.Move { DestinationReg = Assembler.X86.Registers.EDI, DestinationIsIndirect = true, SourceReg = Assembler.X86.Registers.DL };
			new Assembler.X86.Add{DestinationReg = Assembler.X86.Registers.EAX, SourceValue=1};
            new Assembler.X86.Add { DestinationReg = Assembler.X86.Registers.ESI, SourceValue = 1 };
            new Assembler.X86.Add { DestinationReg = Assembler.X86.Registers.EDI, SourceValue = 1 };
			new Assembler.X86.Compare { DestinationReg = Assembler.X86.Registers.EAX, SourceReg = Assembler.X86.Registers.ECX };
            new Assembler.X86.ConditionalJump { Condition = Assembler.X86.ConditionalTestEnum.Equal, DestinationLabel = ".EndLoop" };
            new Assembler.X86.Jump { DestinationLabel = ".StartLoop" };

			new Assembler.Label(".EndLoop");
		}
	}
}