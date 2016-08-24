using System;
using Cosmos.Assembler.x86.SSE;
using XSharp.Compiler;
using CPUx86 = Cosmos.Assembler.x86;
using static XSharp.Compiler.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
	/// <summary>
	/// Convert top Stack element to UInt32 and change its type to Int32.
    /// </summary>
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Conv_U)] // x86 is a 32-bit system, so this is the op-code that we should be using, for an x64 target, use Conv_U8 instead.
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Conv_U4 )]
    public class Conv_U4 : ILOp
    {
        public Conv_U4( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xSource = aOpCode.StackPopTypes[0];
            var xSourceSize = SizeOfType(xSource);
            switch( xSourceSize )
            {
                case 1:
                    throw new Exception("Cosmos.IL2CPU.x86->IL->Conv_U4.cs->The size 1 could not exist, because always is pushed Int32 or Int64!");
                case 2:
                    throw new Exception("Cosmos.IL2CPU.x86->IL->Conv_U4.cs->The size 2 could not exist, because always is pushed Int32 or Int64!");
				case 4:
					if (TypeIsFloat(xSource))
					{
                        XS.SSE.MoveSS(XMM0, ESP, sourceIsIndirect: true);
                        XS.SSE.ConvertSS2SIAndTruncate(EAX, XMM0);
                        XS.Set(ESP, EAX, destinationIsIndirect: true);
                    }
					break;
                case 8:
					if (TypeIsFloat(xSource))
					{
                        XS.SSE2.MoveSD(XMM0, ESP, sourceIsIndirect: true);
                        XS.SSE2.ConvertSD2SIAndTruncate(EAX, XMM0);
                        // We need to move the stack pointer of 4 Byte to "eat" the second double that is yet in the stack or we get a corrupted stack!
                        XS.Add(ESP, 4);
                        XS.Set(ESP, EAX, destinationIsIndirect: true);
                        // Is this really needed? Conv.U2 and Conv.U1 did not this! In reality they should call the same code...
                        //XS.Push(EAX);
                        break;
					}
					else
                    {
                        XS.Pop(EAX);
                        XS.Pop(ECX);
                        XS.Push(EAX);
                        break;
                    }
                default:
                    //EmitNotImplementedException( Assembler, GetServiceProvider(), "Conv_U4: SourceSize " + xStackItem.Size + " not supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel );
                    throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Conv_U4.cs->Unknown size of variable on the top of the stack.");
            }
        }
    }
}
