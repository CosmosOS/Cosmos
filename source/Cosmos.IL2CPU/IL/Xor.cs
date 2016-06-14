using System;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.IL2CPU.X86;
using XSharp.Compiler;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Xor )]
    public class Xor : ILOp
    {
        public Xor( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xSize = Math.Max(SizeOfType(aOpCode.StackPopTypes[0]), SizeOfType(aOpCode.StackPopTypes[1]));
            new CPUx86.Pop { DestinationReg = CPUx86.RegistersEnum.EAX };
            new CPUx86.Pop { DestinationReg = CPUx86.RegistersEnum.EDX };
            XS.Xor(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDX));
            new CPUx86.Push { DestinationReg = CPUx86.RegistersEnum.EAX };
        }
    }
}
