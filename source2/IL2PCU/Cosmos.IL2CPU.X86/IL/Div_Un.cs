using System;
using CPUx86 = Cosmos.IL2CPU.X86;
namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Div_Un )]
    public class Div_Un : ILOp
    {
        public Div_Un( Cosmos.IL2CPU.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xStackItem = Assembler.Stack.Pop();
            if( xStackItem.IsFloat )
            {
                throw new NotImplementedException( "Floats not yet supported!" );
            }
            if( xStackItem.Size == 8 )
            {
                //TODO: implement proper div support for 8byte values!
                new CPUx86.Xor { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EDX };
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
                new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
                new CPUx86.Divide { DestinationReg = CPUx86.Registers.ECX };
                new CPUx86.Push { DestinationValue = 0 };
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };

            }
            else
            {
                new CPUx86.Xor { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EDX };
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                new CPUx86.Divide { DestinationReg = CPUx86.Registers.ECX };
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
            }
        }


        // using System;
        // 
        // using CPUx86 = Cosmos.IL2CPU.X86;
        // 
        // namespace Cosmos.IL2CPU.IL.X86 {
        // 	[OpCode(OpCodeEnum.Div_Un)]
        // 	public class Div_Un: Op {
        // 		public Div_Un(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 		}
        // 		public override void DoAssemble() {
        // 			var xStackItem= Assembler.Stack.Pop();
        // 			if (xStackItem.IsFloat) {
        // 				throw new Exception("Floats not yet supported!");
        // 			}
        // 			if (xStackItem.Size == 8) {
        // 				//TODO: implement proper div support for 8byte values!
        //                 new CPUx86.Xor { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EDX };
        // 				new CPUx86.Pop{DestinationReg = CPUx86.Registers.ECX};
        //                 new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
        //                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
        //                 new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
        //                 new CPUx86.Divide { DestinationReg = CPUx86.Registers.ECX };
        //                 new CPUx86.Push { DestinationValue = 0 };
        //                 new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
        // 
        // 			} else {
        //                 new CPUx86.Xor { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EDX };
        //                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
        // 				new CPUx86.Pop{DestinationReg = CPUx86.Registers.EAX};
        // 				new CPUx86.Divide{DestinationReg=CPUx86.Registers.ECX};
        //                 new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
        // 			}
        // 		}
        // 	}
        // }

    }
}
