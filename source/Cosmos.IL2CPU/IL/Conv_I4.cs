using System;
using Cosmos.Assembler.x86.SSE;
using XSharp.Compiler;
using CPUx86 = Cosmos.Assembler.x86;
using static XSharp.Compiler.XSRegisters;
using static Cosmos.Assembler.x86.SSE.ComparePseudoOpcodes;

namespace Cosmos.IL2CPU.X86.IL
{
	/// <summary>
	/// Convert top Stack element to Int32.
    /// </summary>
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Conv_I)] // x86 is a 32-bit system, so this is the op-code that we should be using, for an x64 target, use Conv_I8 instead.
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Conv_I4 )]
    public class Conv_I4 : ILOp
    {
        public Conv_I4( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xSource = aOpCode.StackPopTypes[0];
            var xSourceSize = SizeOfType(xSource);
            var xSourceIsFloat = TypeIsFloat(xSource);
            switch (xSourceSize)
            {
                case 1:
                    throw new Exception("Cosmos.IL2CPU.x86->IL->Conv_I4.cs->The size 1 could not exist, because always is pushed Int32 or Int64!");
                case 2:
                    throw new Exception("Cosmos.IL2CPU.x86->IL->Conv_I4.cs->The size 2 could not exist, because always is pushed Int32 or Int64!");
                case 4:
                    {
						if (xSourceIsFloat)
						{
                            XS.SSE.MoveSS(XMM0, ESP, sourceIsIndirect: true);
                            XS.SSE.ConvertSS2SIAndTruncate(EAX, XMM0);
                            XS.Set(ESP, EAX, destinationIsIndirect: true);
                        }
                        break;
                    }
                case 8:
                    {
						if (xSourceIsFloat)
						{
                            XS.SSE2.MoveSD(XMM0, ESP, sourceIsIndirect: true);
                            XS.SSE2.ConvertSD2SIAndTruncate(EAX, XMM0);
                            XS.Set(ESP, EAX, destinationIsIndirect: true);
                        }

                        XS.Pop(XSRegisters.EAX);
                        XS.Add(XSRegisters.ESP, 4);
                        XS.Push(XSRegisters.EAX);
                        break;

                    }
                default:
                    //EmitNotImplementedException( Assembler, GetServiceProvider(), "Conv_I4: SourceSize " + xSource + " not yet supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel );
                    throw new NotImplementedException();
            }
        }
    }
}
