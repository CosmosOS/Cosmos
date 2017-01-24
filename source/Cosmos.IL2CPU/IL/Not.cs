using System;

using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Not)]
    public class Not : ILOp
    {
        public Not(Cosmos.Assembler.Assembler aAsmblr)
            : base(aAsmblr)
        {
        }

        public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
        {
            var xStackContent = aOpCode.StackPopTypes[0];
            var xSize = SizeOfType(xStackContent);

            if (xSize > 8)
            {
                throw new NotSupportedException("Cosmos.IL2CPU.x86->IL->And.cs->Error: Operands have different size!");
            }

            if (xSize <= 4)
            {
                XS.Not(ESP, isIndirect: true, size: RegisterSize.Int32);
            }
            else if (xSize <= 8)
            {
                XS.Not(ESP, isIndirect: true, size: RegisterSize.Int32);
                XS.Not(ESP, displacement: 4, size: RegisterSize.Int32);
            }
        }
    }
}
