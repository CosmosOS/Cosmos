using System;
using System.Linq;

using CPUx86 = Cosmos.Compiler.Assembler.X86;
using Cosmos.Compiler.Assembler;

namespace Cosmos.IL2CPU.X86.IL {
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Stfld)]
  public class Stfld : ILOp {
    public Stfld(Cosmos.Compiler.Assembler.Assembler aAsmblr) : base(aAsmblr) {
    }

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode) {
      var xOpCode = (ILOpCodes.OpField)aOpCode;
      var xField = xOpCode.Value;
      DoExecute(Assembler, aMethod, xField);
    }

    public static void DoExecute(Cosmos.Compiler.Assembler.Assembler aAssembler,  MethodInfo aMethod, string aFieldId, Type aDeclaringObject, bool aNeedsGC) {
      var xType = aMethod.MethodBase.DeclaringType;
      int xExtraOffset = aNeedsGC ? 12 : 0;

      var xFields = GetFieldsInfo(aDeclaringObject);
      var xFieldInfo = (from item in xFields
                        where item.Id == aFieldId
                        select item).Single();
      var xActualOffset = xFieldInfo.Offset + xExtraOffset;
      var xSize = xFieldInfo.Size;
      new Comment("Field: " + xFieldInfo.Id);
      new Comment("Type: " + xFieldInfo.FieldType.ToString());
      new Comment("Size: " + xFieldInfo.Size);

      aAssembler.Stack.Pop();

      uint xRoundedSize = Align(xSize, 4);

      new CPUx86.Mov { DestinationReg = CPUx86.Registers.ECX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = (int)xRoundedSize };
      new CPUx86.Add { DestinationReg = CPUx86.Registers.ECX, SourceValue = (uint)(xActualOffset) };
      //TODO: Can't we use an x86 op to do a byte copy instead and be faster?
      for (int i = 0; i < (xSize / 4); i++) {
        new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
        new CPUx86.Mov { DestinationReg = CPUx86.Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = (int)((i * 4)), SourceReg = CPUx86.Registers.EAX };
      }

      switch (xSize % 4) {
        case 1: {
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
            new CPUx86.Mov { DestinationReg = CPUx86.Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = (int)((xSize / 4) * 4), SourceReg = CPUx86.Registers.AL };
            break;
          }
        case 2: {
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
            new CPUx86.Mov { DestinationReg = CPUx86.Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = (int)((xSize / 4) * 4), SourceReg = CPUx86.Registers.AX };
            break;
          }
		case 3: {
				new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
				// move 2 lower bytes
				new CPUx86.Mov { DestinationReg = CPUx86.Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = (int)((xSize / 4) * 4), SourceReg = CPUx86.Registers.AX };
				// shift third byte to lowest
				new CPUx86.ShiftRight { DestinationReg = CPUx86.Registers.EAX, SourceValue = 16 };
				new CPUx86.Mov { DestinationReg = CPUx86.Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = (int)((xSize / 4) * 4) + 2, SourceReg = CPUx86.Registers.AL };
				break;
			}
        case 0: {
            break;
          }
        default:
          throw new Exception("Remainder size " + (xSize % 4) + " not supported!");
      }

#if! SKIP_GC_CODE
          if (aNeedsGC) {
            new CPUx86.Push { DestinationReg = CPUx86.Registers.ECX };
            new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
            new CPUx86.Call { DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName(GCImplementationRefs.DecRefCountRef) };
            new CPUx86.Call { DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName(GCImplementationRefs.DecRefCountRef) };
          }
#endif
      new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
      aAssembler.Stack.Pop();
    }

    public static void DoExecute(Cosmos.Compiler.Assembler.Assembler aAssembler,  MethodInfo aMethod, System.Reflection.FieldInfo aField) {
      bool xNeedsGC = aField.DeclaringType.IsClass && !aField.DeclaringType.IsValueType;

      DoExecute(aAssembler, aMethod, aField.GetFullName(), aField.DeclaringType, xNeedsGC);
    }

  }
}