using System;
using CPUx86 = Cosmos.IL2CPU.X86;
namespace Cosmos.IL2CPU.X86.IL
{
    /// <summary>
    /// Divides two unsigned values and pushes the remainder onto the evaluation stack.
    /// </summary>
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Rem_Un )]
    public class Rem_Un : ILOp
    {
        public Rem_Un( Cosmos.IL2CPU.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            if( Assembler.Stack.Peek().IsFloat )
            {
                //EmitNotImplementedException( Assembler, GetServiceProvider(), "Rem: Float support not yet implemented!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel );
                throw new NotImplementedException();
            }
            var xStackItem = Assembler.Stack.Peek();
            int xSize = Math.Max( Assembler.Stack.Pop().Size, Assembler.Stack.Pop().Size );
            if( xSize > 4 )
            {
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
                new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX }; // gets devised by ecx
                new CPUx86.Xor { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EDX };

                new CPUx86.Divide { DestinationReg = CPUx86.Registers.ECX }; // => EAX / ECX 
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EDX };

            }
            else
            {
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX }; // gets devised by ecx
                new CPUx86.Xor { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EDX };

                new CPUx86.Divide { DestinationReg = CPUx86.Registers.ECX }; // => EAX / ECX 
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EDX };
            }
            Assembler.Stack.Push( xStackItem );
        }


        // using System;
        // using System.IO;
        // 
        // 
        // using CPU = Cosmos.IL2CPU.X86;
        // 
        // namespace Indy.IL2CPU.IL.X86 {
        // 	[OpCode(OpCodeEnum.Rem_Un)]
        // 	public class Rem_Un: Rem {
        // 		public Rem_Un(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 		}
        // 	}
        // }

    }
}
