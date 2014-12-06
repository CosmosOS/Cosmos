using System;
using System.Linq;
using CPUx86 = Cosmos.Assembler.x86;

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
            var xOpCode = (ILOpCodes.OpField)aOpCode;
            DoExecute(Assembler, aMethod, xOpCode.Value.DeclaringType, xOpCode.Value.GetFullName(), true);
        }

        public static void DoExecute(Cosmos.Assembler.Assembler Assembler, MethodInfo aMethod, Type aDeclaringType, string aField, bool aDerefValue)
        {
            var xFields = GetFieldsInfo(aDeclaringType);
            var xFieldInfo = (from item in xFields
                              where item.Id == aField
                              select item).Single();

            int xExtraOffset = 0;
            var xType = aMethod.MethodBase.DeclaringType;
            bool xNeedsGC = aDeclaringType.IsClass && !aDeclaringType.IsValueType;

            if (xNeedsGC)
            {
                xExtraOffset = 12;
            }

            var xActualOffset = xFieldInfo.Offset + xExtraOffset;
            var xSize = xFieldInfo.Size;

            if (aDerefValue && xFieldInfo.IsExternalValue)
            {
                new CPUx86.Mov
                {
                    DestinationReg = CPUx86.Registers.EAX,
                    SourceReg = CPUx86.Registers.ESP,
                    SourceIsIndirect = true,
                    SourceDisplacement = (int)xActualOffset
                };
                new CPUx86.Mov { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.EAX };
            }
            else
                new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, SourceValue = (uint)(xActualOffset) };
        }
    }
}