using System;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.Assembler.x86;
using Cosmos.Assembler;
using Cosmos.Assembler.x86.SSE;
using XSharp.Compiler;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Add_Ovf_Un )]
    public class Add_Ovf_Un : ILOp
    {
		public Add_Ovf_Un(Cosmos.Assembler.Assembler aAsmblr)
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
			// TODO overflow check for float
            var xType = aOpCode.StackPopTypes[0];
            var xSize = SizeOfType(xType);
            var xIsFloat = TypeIsFloat(xType);
            if (xSize > 8)
            {
                //EmitNotImplementedException( Assembler, aServiceProvider, "Size '" + xSize.Size + "' not supported (add)", aCurrentLabel, aCurrentMethodInfo, aCurrentOffset, aNextLabel );
                throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Add_Ovf_Un.cs->Error: StackSize > 8 not supported");
            }
            else
            {
				var xBaseLabel = GetLabel(aMethod, aOpCode) + ".";
				var xSuccessLabel = xBaseLabel + "Success";
                if (xSize > 4)
                {
                    if (xIsFloat)
                    {
						//TODO overflow check
                        new CPUx86.x87.FloatLoad { DestinationReg=RegistersEnum.ESP,Size=64, DestinationIsIndirect=true };
                        XS.Add(XSRegisters.ESP, 8);
                        new CPUx86.x87.FloatAdd { DestinationReg = CPUx86.RegistersEnum.ESP, DestinationIsIndirect=true, Size=64 };
                        new CPUx86.x87.FloatStoreAndPop { DestinationReg = RegistersEnum.ESP, Size = 64, DestinationIsIndirect = true };
                    }
                    else
                    {
                        XS.Pop(XSRegisters.OldToNewRegister(RegistersEnum.EDX)); // low part
                        XS.Pop(XSRegisters.OldToNewRegister(RegistersEnum.EAX)); // high part
                        XS.Add(XSRegisters.ESP, XSRegisters.EDX, destinationIsIndirect: true);
						new AddWithCarry { DestinationReg = RegistersEnum.ESP, DestinationIsIndirect = true, DestinationDisplacement = 4, SourceReg = RegistersEnum.EAX };
                    }
                }
                else
                {
                    if (xIsFloat) //float
                    {
						//TODO overflow check
                        XS.SSE.MoveSS(XSRegisters.XMM0, XSRegisters.ESP, sourceIsIndirect: true);
                        XS.Add(XSRegisters.OldToNewRegister(RegistersEnum.ESP), 4);
                        XS.SSE.MoveSS(XSRegisters.XMM1, XSRegisters.ESP, sourceIsIndirect: true);
                        XS.SSE.AddSS(XSRegisters.XMM0, XSRegisters.XMM1);
                        XS.SSE.MoveSS(XSRegisters.XMM1, XSRegisters.ESP, sourceIsIndirect: true);
                    }
                    else //integer
                    {
                        XS.Pop(XSRegisters.OldToNewRegister(RegistersEnum.EAX));
                        XS.Add(XSRegisters.ESP, XSRegisters.EAX, destinationIsIndirect: true);
                    }
                }
				if (false == xIsFloat)
				{
					XS.Jump(ConditionalTestEnum.NotCarry, xSuccessLabel);
					ThrowOverflowException();
				}
				XS.Label(xSuccessLabel);
            }
        }
    }
}
