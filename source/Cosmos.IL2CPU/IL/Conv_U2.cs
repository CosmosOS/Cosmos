using System;
using Cosmos.Assembler.x86.SSE;
using XSharp.Compiler;
using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.IL2CPU.X86.IL
{
	/// <summary>
	/// Convert top Stack element to UInt16 and extends it to Int32.
	/// </summary>
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Conv_U2 )]
    public class Conv_U2 : ILOp
    {
        public Conv_U2( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xSource = aOpCode.StackPopTypes[0];
            var xSourceSize = SizeOfType(xSource);
            if (TypeIsFloat(xSource))
            {
                if (xSourceSize == 4)
                {
                    new CPUx86.SSE.MoveSS { SourceReg = CPUx86.RegistersEnum.ESP, DestinationReg = CPUx86.RegistersEnum.XMM0, SourceIsIndirect = true };
                    XS.SSE.ConvertSS2SIAndTruncate(XSRegisters.EAX, XSRegisters.XMM0);
                    XS.Set(XSRegisters.ESP, XSRegisters.EAX, destinationIsIndirect: true);
                }
                else if (xSourceSize == 8)
                {
                    XS.SSE3.MoveDoubleAndDuplicate(XSRegisters.XMM0, XSRegisters.ESP, sourceIsIndirect: true);
                    XS.SSE2.ConvertSD2SIAndTruncate(XSRegisters.EAX, XSRegisters.XMM0);
                    XS.Set(XSRegisters.ESP, XSRegisters.EAX, destinationIsIndirect: true);
                }
                else
                {
                    throw new Exception("Cosmos.IL2CPU.x86->IL->Conv_U2.cs->Unknown size of floating point value.");
                }
            }
            else {
                switch( xSourceSize )
                {
                    case 2:
                        break;
                    case 1:
                    case 4:
						    XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
						    new CPUx86.MoveZeroExtend { DestinationReg = CPUx86.RegistersEnum.EAX, SourceReg = CPUx86.RegistersEnum.AX, Size = 16 };
                            XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
						    break;
                    case 8:
                        {
                            XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
                            XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX));
                            new CPUx86.MoveZeroExtend { DestinationReg = CPUx86.RegistersEnum.EAX, SourceReg = CPUx86.RegistersEnum.AX, Size = 16 };
                            XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
                            break;
                        }
                    default:
                        //EmitNotImplementedException( Assembler, GetServiceProvider(), "Conv_U2: SourceSize " + xSource + " not yet supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel );
                        throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Conv_U2.cs->Unknown size of variable on the top of the stack.");
                }
            }
        }
    }
}
