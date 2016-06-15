using System;
using XSharp.Compiler;
using CPUx86 = Cosmos.Assembler.x86;

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
							new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.RegistersEnum.XMM0, SourceReg = CPUx86.RegistersEnum.ESP, SourceIsIndirect = true };
                            new CPUx86.SSE.ConvertSS2SIAndTruncate { DestinationReg = CPUx86.RegistersEnum.EAX, SourceReg = CPUx86.RegistersEnum.XMM0 };
							new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.ESP, SourceReg = CPUx86.RegistersEnum.EAX, DestinationIsIndirect = true };
						}
                        break;
                    }
                case 8:
                    {
						if (xSourceIsFloat)
						{
							new CPUx86.SSE.MoveDoubleAndDupplicate { DestinationReg = CPUx86.RegistersEnum.XMM0, SourceReg = CPUx86.RegistersEnum.ESP, SourceIsIndirect = true };
                            new CPUx86.SSE.ConvertSD2SIAndTruncate { DestinationReg = CPUx86.RegistersEnum.EAX, SourceReg = CPUx86.RegistersEnum.XMM0 };
							new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.ESP, SourceReg = CPUx86.RegistersEnum.EAX, DestinationIsIndirect = true };
						}

                        XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
                        XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), 4);
                        XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
                        break;

                    }
                default:
                    //EmitNotImplementedException( Assembler, GetServiceProvider(), "Conv_I4: SourceSize " + xSource + " not yet supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel );
                    throw new NotImplementedException();
            }
        }
    }
}
