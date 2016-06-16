using System;
using System.Linq;
using System.Reflection;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.Assembler;
using XSharp.Compiler;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ldflda)]
    public class Ldflda : ILOp
    {
        public Ldflda(Cosmos.Assembler.Assembler aAsmblr)
            : base(aAsmblr)
        {
        }

        public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
        {
            var xOpCode = (ILOpCodes.OpField) aOpCode;
            DoExecute(Assembler, aMethod, xOpCode.Value.DeclaringType, xOpCode.Value.GetFullName(), true, DebugEnabled);
        }

        public static void DoExecute(Cosmos.Assembler.Assembler Assembler, MethodInfo aMethod, Type aDeclaringType, string aField, bool aDerefValue, bool aDebugEnabled)
        {
          var xFieldInfo = ResolveField(aDeclaringType, aField, true);
          DoExecute(Assembler, aMethod, aDeclaringType, xFieldInfo, aDerefValue, aDebugEnabled);
        }

        public static void DoExecute(Cosmos.Assembler.Assembler Assembler, MethodInfo aMethod, Type aDeclaringType, FieldInfo aField, bool aDerefValue, bool aDebugEnabled)
        {
            XS.Comment("Field: " + aField.Id);
            int xExtraOffset = 0;
            var xType = aMethod.MethodBase.DeclaringType;
            bool xNeedsGC = aDeclaringType.IsClass && !aDeclaringType.IsValueType;

            if (xNeedsGC)
            {
                xExtraOffset = 12;
            }

            var xActualOffset = aField.Offset + xExtraOffset;
            var xSize = aField.Size;
            DoNullReferenceCheck(Assembler, aDebugEnabled, 0);

            if (aDerefValue && aField.IsExternalValue)
            {
                if (xNeedsGC)
                {
                    // eax contains the handle now, lets convert it to the real memory address
                    XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), sourceDisplacement: (int)xActualOffset);
                }

                new CPUx86.Mov {DestinationReg = CPUx86.RegistersEnum.ESP, DestinationIsIndirect = true, SourceReg = CPUx86.RegistersEnum.EAX};
            }
            else
            {
                XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
                if (xNeedsGC)
                {
                    // eax contains the handle now, lets convert it to the real memory address
                    XS.Set(XSRegisters.EAX, XSRegisters.EAX, sourceIsIndirect: true);
                }
                XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), (uint)(xActualOffset));
                XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
            }
        }
    }
}
