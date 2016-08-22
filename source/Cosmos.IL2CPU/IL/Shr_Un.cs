using System;
using CPUx86 = Cosmos.Assembler.x86;
using CPU = Cosmos.Assembler.x86;
using Cosmos.Assembler;
using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Shr_Un )]
    public class Shr_Un : ILOp
    {
        public Shr_Un( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
			var xStackItem_ShiftAmount = aOpCode.StackPopTypes[0];
			var xStackItem_Value = aOpCode.StackPopTypes[1];
            var xStackItem_Value_Size = SizeOfType(xStackItem_Value);

            XS.Pop(XSRegisters.ECX); // shift amount
#if DOTNETCOMPATIBLE
			if (xStackItem_Value.Size == 4)
#else
            if (xStackItem_Value_Size <= 4)
#endif
			{
				XS.ShiftRight(XSRegisters.ESP, XSRegisters.CL, destinationIsIndirect: true, size: RegisterSize.Int32);
			}
#if DOTNETCOMPATIBLE
			else if (xStackItem_Value_Size == 8)
#else
			else if (xStackItem_Value_Size <= 8)
#endif
			{
				string BaseLabel = GetLabel(aMethod, aOpCode) + ".";
				string HighPartIsZero = BaseLabel + "HighPartIsZero";
				string End_Shr = BaseLabel + "End_Shr";

				// [ESP] is low part
				// [ESP + 4] is high part

				// move high part in EAX
				XS.Set(XSRegisters.EAX, XSRegisters.ESP, sourceDisplacement: 4);

				XS.Compare(XSRegisters.CL, 32, size: RegisterSize.Byte8);
				XS.Jump(CPU.ConditionalTestEnum.AboveOrEqual, HighPartIsZero);

				// shift lower part
                XS.ShiftRightDouble(ESP, EAX, CL, destinationIsIndirect: true);
				// shift higher part
				XS.ShiftRight(ESP, CL, destinationDisplacement: 4, size: RegisterSize.Int32);
				XS.Jump(End_Shr);

				XS.Label(HighPartIsZero);
				// remove bits >= 32, so that CL max value could be only 31
				XS.And(XSRegisters.CL, 0x1f, size: RegisterSize.Byte8);

				// shift high part and move it in low part
				XS.ShiftRight(XSRegisters.EAX, XSRegisters.CL);
				XS.Set(ESP, EAX, destinationIsIndirect: true);
				// replace unknown high part with a zero
				new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.ESP, DestinationIsIndirect = true, DestinationDisplacement = 4, SourceValue = 0};

				XS.Label(End_Shr);
			}
			else
				throw new NotSupportedException("A size bigger 8 not supported at Shr!");
            /*string xLabelName = AppAssembler.TmpPosLabel(aMethod, aOpCode);
            var xStackItem_ShiftAmount = Assembler.Stack.Pop();
            var xStackItem_Value = Assembler.Stack.Peek();
            if( xStackItem_Value.Size <= 4 )
            {
                XS.Pop(XSRegisters.ECX); // shift amount
                XS.ShiftRight(XSRegisters.ESP, XSRegisters.CL, destinationIsIndirect: true);
            }
            else if( xStackItem_Value.Size <= 8 )
            {
				XS.Pop(XSRegisters.ECX); // shift amount
				// [ESP] is high part
				// [ESP + 4] is low part
				XS.Mov(XSRegisters.EAX, XSRegisters.ESP, sourceDisplacement: 4);
				// shift low part
				new CPUx86.ShiftRightDouble { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.EAX, ArgumentReg = CPUx86.Registers.CL };
				// shift high part
				XS.ShiftRight(XSRegisters.ESP, XSRegisters.CL, destinationIsIndirect: true, size: RegisterSize.Int32);
            }*/
        
#if false
            XS.Pop(XSRegisters.ECX); // shift amount
            string xBaseLabel = GetLabel( aMethod, aOpCode ) + ".";
            var xStackItem_ShiftAmount = aOpCode.StackPopTypes[0];
            var xStackItem_Value = aOpCode.StackPopTypes[1];

            if (TypeIsFloat(xStackItem_Value))
            {
                throw new NotImplementedException("Floats not yet supported!");
            }
            var xStackItem_Value_Size = SizeOfType(xStackItem_Value);
            if( xStackItem_Value_Size <= 4 )
            {
                XS.Pop(XSRegisters.EAX); // shift amount
                XS.Pop(XSRegisters.EBX); // value
                XS.Set(XSRegisters.CL, XSRegisters.AL);
                XS.ShiftRight(XSRegisters.EBX, CL);
                XS.Push(XSRegisters.EBX);
                return;
            }
            if( xStackItem_Value_Size <= 8 )
            {
                string BaseLabel = GetLabel(aMethod, aOpCode) + ".";
                string HighPartIsZero = BaseLabel + "HighPartIsZero";
                string End_Shr = BaseLabel + "End_Shr";

                // [ESP] is low part
                // [ESP + 4] is high part

                // move high part in EAX
                XS.Set(XSRegisters.EAX, XSRegisters.ESP, sourceDisplacement: 4);

                XS.Compare(XSRegisters.CL, 32, size: RegisterSize.Byte8);
                XS.Jump(CPU.ConditionalTestEnum.AboveOrEqual, HighPartIsZero);

                // shift lower part
                XS.ShiftRightDouble(ESP, EAX, CL, destinationIsIndirect: true);
                // shift higher part
                XS.ShiftRight(ESP, CL, destinationDisplacement: 4, size: RegisterSize.Int32);
                XS.Jump(End_Shr);

                XS.Label(HighPartIsZero);
                // remove bits >= 32, so that CL max value could be only 31
                XS.And(XSRegisters.CL, 0x1f, size: RegisterSize.Byte8);

                // shift high part and move it in low part
                XS.ShiftRight(XSRegisters.EAX, XSRegisters.CL);
                XS.Set(ESP, EAX, destinationIsIndirect: true);
                // replace unknown high part with a zero, if <= 32
                new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.ESP, DestinationIsIndirect = true, DestinationDisplacement = 4, SourceValue = 0 };

                XS.Label(End_Shr);

#if false
                XS.Pop(XSRegisters.EDX);
                XS.Set(XSRegisters.EAX, 0);
                XS.Label(xBaseLabel + "__StartLoop" );
                XS.Compare(XSRegisters.EDX, XSRegisters.EAX);
                XS.Jump(CPUx86.ConditionalTestEnum.Equal, xBaseLabel + "__EndLoop");
                XS.Set(EBX, ESP, sourceIsIndirect: true);
                XS.Set(XSRegisters.CL, 1);
                XS.ShiftRight(XSRegisters.EBX, CL);
                XS.Set(ESP, EBX, destinationIsIndirect: true);
                XS.Set(XSRegisters.CL, 1);
                XS.RotateThroughCarryRight(ESP, CL, destinationDisplacement: 4, size: RegisterSize.Int32);
                XS.Add(XSRegisters.EAX, 1);
                new CPUx86.Jump { DestinationLabel = xBaseLabel + "__StartLoop" };


                XS.Label(xBaseLabel + "__EndLoop" );
#endif
                return;
#endif
        }
    }
}
