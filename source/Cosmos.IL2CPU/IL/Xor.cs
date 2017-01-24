using System;

using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Xor)]
    public class Xor : ILOp
    {
        public Xor(Cosmos.Assembler.Assembler aAsmblr)
            : base(aAsmblr)
        {
        }

        public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
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
                // low part
                XS.Pop(EAX);
                // high part
                XS.Pop(EDX);

                // xor on low parts
                XS.Xor(ESP, EAX, destinationIsIndirect: true);
                // xor on high parts
                XS.Xor(ESP, EDX, destinationDisplacement: 4);
            }
        }
    }
}
