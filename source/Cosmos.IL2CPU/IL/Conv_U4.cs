using System;
using Cosmos.Assembler.x86.SSE;
using XSharp.Compiler;
using CPUx86 = Cosmos.Assembler.x86;

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
						XS.SSE.MoveSS(XSRegisters.XMM0, XSRegisters.ESP, sourceIsIndirect: true);
						XS.SSE.ConvertSS2SIAndTruncate(XSRegisters.EAX, XSRegisters.XMM0);
						new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.ESP, SourceReg = CPUx86.RegistersEnum.EAX, DestinationIsIndirect = true };
					}
					break;
                case 8:
					if (TypeIsFloat(xSource))
					{
                        XS.SSE3.MoveDoubleAndDuplicate(XSRegisters.XMM0, XSRegisters.ESP, sourceIsIndirect: true);
						            XS.SSE2.ConvertSD2SIAndTruncate(XSRegisters.EAX, XSRegisters.XMM0);
                        new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.ESP, SourceReg = CPUx86.RegistersEnum.EAX, DestinationIsIndirect = true };
                        XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
                        XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), 4);
                        XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
                        break;
					}
					else
                    {
                        XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
                        XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX));
                        XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
                        break;
                    }
                default:
                    //EmitNotImplementedException( Assembler, GetServiceProvider(), "Conv_U4: SourceSize " + xStackItem.Size + " not supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel );
                    throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Conv_U4.cs->Unknown size of variable on the top of the stack.");
            }
        }
    }
}
