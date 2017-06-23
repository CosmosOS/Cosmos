using System;

using XSharp.Common;
using static XSharp.Common.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Xor )]
    public class Xor : ILOp
    {
        public Xor( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute(_MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xSize = Math.Max(SizeOfType(aOpCode.StackPopTypes[0]), SizeOfType(aOpCode.StackPopTypes[1]));

            if (xSize > 8)
            {
                throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Xor.cs->Error: StackSize > 8 not supported");
            }

            if (xSize <= 4)
            {
                XS.Pop(EAX);
                XS.Xor(ESP, EAX, destinationIsIndirect: true);
            }
            else if (xSize <= 8)
            {
                XS.Pop(EAX);
                XS.Pop(EDX);
                XS.Xor(ESP, EAX, destinationIsIndirect: true);
                XS.Xor(ESP, EDX, destinationDisplacement: 4);
            }
        }
    }
}
