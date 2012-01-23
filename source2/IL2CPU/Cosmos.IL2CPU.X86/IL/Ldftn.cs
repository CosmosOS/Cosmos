using System;
using Cosmos.IL2CPU.ILOpCodes;
using CPUx86 = Cosmos.Compiler.Assembler.X86;
using CPU = Cosmos.Assembler;
using Cosmos.Assembler;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldftn )]
    public class Ldftn : ILOp
    {
        public Ldftn( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            new CPUx86.Push { DestinationRef = Cosmos.Assembler.ElementReference.New( MethodInfoLabelGenerator.GenerateLabelName(((OpMethod)aOpCode).Value ) ) };
			Assembler.Stack.Push(new StackContents.Item(4, typeof(uint)));
        }
    }
}