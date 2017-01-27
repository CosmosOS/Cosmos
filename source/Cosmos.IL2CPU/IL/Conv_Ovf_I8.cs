using System;

using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Conv_Ovf_I8 )]
    public class Conv_Ovf_I8 : ILOp
    {
        public Conv_Ovf_I8( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xSource = aOpCode.StackPopTypes[0];
            var xSourceSize = SizeOfType(xSource);
            var xSourceIsFloat = TypeIsFloat(xSource);
            switch( xSourceSize )
            {
                case 1:
                case 2:
                case 4:
                    XS.Pop(EAX);
                    XS.SignExtendAX(RegisterSize.Int32);
                    XS.Push(EDX);
                    XS.Push(EAX);
                    break;
                case 8:
                    XS.Noop();
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
