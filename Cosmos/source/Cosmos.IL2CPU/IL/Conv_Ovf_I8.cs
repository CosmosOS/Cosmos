using System;

using XSharp.Common;
using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Conv_Ovf_I8 )]
    public class Conv_Ovf_I8 : ILOp
    {
        public Conv_Ovf_I8( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute(_MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xSource = aOpCode.StackPopTypes[0];
            var xSourceSize = SizeOfType(xSource);
            var xSourceIsFloat = TypeIsFloat(xSource);
            switch( xSourceSize )
            {
                case 1:
                case 2:
                case 4:
                    XS.Pop(XSRegisters.EAX);
                    XS.SignExtendAX(XSRegisters.RegisterSize.Int32);
                    XS.Push(XSRegisters.EDX);
                    XS.Push(XSRegisters.EAX);
                    break;
                case 8:
                    XS.Noop();
                    break;
                default:
                    //EmitNotImplementedException( Assembler, GetServiceProvider(), "Conv_Ovf_I8: SourceSize " + xSource + " not supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel );
                    throw new NotImplementedException();
            }
        }
    }
}
