using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldind_I1 )]
    public class Ldind_I1 : ILOp
    {
        public Ldind_I1( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            DoNullReferenceCheck(Assembler, DebugEnabled, 0);
            XS.Pop(EAX);
            XS.MoveSignExtend(EAX, EAX, sourceIsIndirect: true, size: RegisterSize.Byte8);
            XS.Push(EAX);
        }
    }
}
