using System;

namespace Cosmos.Compiler.IL.X86
{
	[OpCode(OpCodeEnum.Rem)]
	public class Rem: Op
	{



		#region Old code
		// using System;
		// 
		// using CPUx86 = Indy.IL2CPU.Assembler.X86;
		// using Indy.IL2CPU.Assembler;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[OpCode(OpCodeEnum.Rem)]
		// 	public class Rem: Op {
		//         private string mNextLabel;
		// 	    private string mCurLabel;
		// 	    private uint mCurOffset;
		// 	    private MethodInformation mMethodInformation;
		// 		public Rem(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		//              mMethodInformation = aMethodInfo;
		// 		    mCurOffset = aReader.Position;
		// 		    mCurLabel = IL.Op.GetInstructionLabel(aReader);
		//             mNextLabel = IL.Op.GetInstructionLabel(aReader.NextPosition);
		// 		}
		// 		public override void DoAssemble() {
		// 			if (Assembler.StackContents.Peek().IsFloat) {
		//                 EmitNotImplementedException(Assembler, GetServiceProvider(), "Rem: Float support not yet implemented!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
		//                 return;
		// 			}
		// 			StackContent xStackItem = Assembler.StackContents.Peek();
		// 			int xSize = Math.Max(Assembler.StackContents.Pop().Size, Assembler.StackContents.Pop().Size);
		// 			if (xSize > 4) {
		//                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
		//                 new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
		//                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX }; // gets devised by ecx
		//                 new CPUx86.Xor { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EDX };
		// 
		//                 new CPUx86.Divide { DestinationReg = CPUx86.Registers.ECX }; // => EAX / ECX 
		//                 new CPUx86.Push { DestinationReg = CPUx86.Registers.EDX };
		// 
		// 			} else {
		// 				new CPUx86.Pop{DestinationReg = CPUx86.Registers.ECX};
		//                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX }; // gets devised by ecx
		//                 new CPUx86.Xor { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EDX };
		// 
		//                 new CPUx86.Divide { DestinationReg = CPUx86.Registers.ECX }; // => EAX / ECX 
		//                 new CPUx86.Push { DestinationReg = CPUx86.Registers.EDX };
		// 			}
		// 			Assembler.StackContents.Push(xStackItem);
		// 		}
		// 	}
		// }
		#endregion
	}
}
