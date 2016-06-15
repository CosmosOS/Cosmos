using System;

using Cosmos.Assembler;
using XSharp.Compiler;
using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.IL2CPU.Plugs.System.Runtime.CompilerServices {
	[Plug(Target = typeof(global::System.Runtime.CompilerServices.RuntimeHelpers))]
	public static class RuntimeHelpersImpl {

		public static void cctor() {
			//TODO: do something
		}

		[Inline(TargetPlatform = TargetPlatform.x86)]
		public static void InitializeArray(Array array, RuntimeFieldHandle fldHandle) {
			// Arguments:
			//    Array aArray, RuntimeFieldHandle aFieldHandle
            new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.EDI, SourceReg = CPUx86.RegistersEnum.EBP, SourceIsIndirect = true, SourceDisplacement = 0xC }; // array
		    new CPUx86.Mov {DestinationReg = CPUx86.RegistersEnum.EDI, SourceReg = CPUx86.RegistersEnum.EDI, SourceIsIndirect = true};
            new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.ESI, SourceReg = CPUx86.RegistersEnum.EBP, SourceIsIndirect = true, SourceDisplacement = 8 };// aFieldHandle
            XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDI), 8);
		    new CPUx86.Push {DestinationReg = CPUx86.RegistersEnum.EDI, DestinationIsIndirect = true};
            XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDI), 4);
            new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.EAX, SourceReg = CPUx86.RegistersEnum.EDI, SourceIsIndirect = true };
		    new CPUx86.Multiply {DestinationReg = CPUx86.RegistersEnum.ESP, DestinationIsIndirect = true, Size = 32};
            XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX));
            XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
            XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), 0);
            XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDI), 4);

			new Label(".StartLoop");
			new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.DL, SourceReg = CPUx86.RegistersEnum.ESI, SourceIsIndirect = true };
            new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.EDI, DestinationIsIndirect = true, SourceReg = CPUx86.RegistersEnum.DL };
			XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), 1);
            XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESI), 1);
            XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDI), 1);
			XS.Compare(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX));
            new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Equal, DestinationLabel = ".EndLoop" };
            new CPUx86.Jump { DestinationLabel = ".StartLoop" };

			new Label(".EndLoop");
		}
	}
}
