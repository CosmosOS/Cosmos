using System;
using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;
using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.IL2CPU.X86.IL
{
	  /// <summary>
	  /// Convert top Stack element to Int64.
	  /// </summary>
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Conv_I8 )]
    public class Conv_I8 : ILOp
    {
        public Conv_I8( Cosmos.Assembler.Assembler aAsmblr )
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
                    XS.Pop(EAX);
                    XS.MoveSignExtend(EAX, AL);
                    XS.SignExtendAX(RegisterSize.Int32);
                    XS.Push(EDX);
                    XS.Push(EAX);
                    break;
                case 2:
                    XS.Pop(EAX);
                    XS.MoveSignExtend(EAX, AX);
                    XS.SignExtendAX(RegisterSize.Int32);
                    XS.Push(EDX);
                    XS.Push(EAX);
                    break;
                case 4:
					          if (xSourceIsFloat)
					          {
                        /*
                          * Sadly for x86 there is no way using SSE to convert a float to an Int64... in x64 we could use ConvertPD2DQAndTruncate with
                          * x64 register as a destination... so this one of the few cases in which we need the legacy FPU!
                          */
						            XS.FPU.FloatLoad(ESP, destinationIsIndirect: true, size: RegisterSize.Int32);
						            XS.Sub(ESP, 4);
						            XS.FPU.IntStoreWithTruncate(ESP, isIndirect: true, size: RegisterSize.Long64);
					          }
					          else
					          {
						            XS.Pop(EAX);
						            XS.SignExtendAX(RegisterSize.Int32);
						            XS.Push(EDX);
                        XS.Push(EAX);
					          }
                    break;
                case 8:
					          if (xSourceIsFloat)
					          {
                        /*
                         * Sadly for x86 there is no way using SSE to convert a double to an Int64... in x64 we could use ConvertPD2DQAndTruncate with
                         * x64 register as a destination... so only in this case we need the legacy FPU!
                         */
						            XS.FPU.FloatLoad(ESP, destinationIsIndirect: true, size: RegisterSize.Long64);
						            XS.FPU.IntStoreWithTruncate(ESP, isIndirect: true, size: RegisterSize.Long64);
					          }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
