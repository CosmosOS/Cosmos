using System;

using XSharp.Common;
using static XSharp.Common.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Not )]
    public class Not : ILOp
    {
        public Not( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute(_MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xSource = aOpCode.StackPopTypes[0];
            var xSourceSize = SizeOfType(xSource);
            var xSourceIsFloat = TypeIsFloat(xSource);

            if (xSourceSize > 8)
            {
                throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Not.cs->Error: StackSize > 8 not supported");
            }

            if (xSourceSize <= 4)
            {
                XS.Not(ESP, isIndirect: true, size: RegisterSize.Int32);
            }
            else if (xSourceSize <= 8)
            {
                XS.Not(ESP, isIndirect: true, size: RegisterSize.Int32);
                XS.Not(ESP, displacement: 4, size: RegisterSize.Int32);
            }
        }
    }
}
