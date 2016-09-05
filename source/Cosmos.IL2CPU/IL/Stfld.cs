using System;
using System.Linq;
using SysReflection = System.Reflection;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.Assembler;
using XSharp.Compiler;

namespace Cosmos.IL2CPU.X86.IL {
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Stfld)]
  public class Stfld : ILOp {
    public Stfld(Cosmos.Assembler.Assembler aAsmblr) : base(aAsmblr) {
    }

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode) {
      var xOpCode = (ILOpCodes.OpField)aOpCode;
      var xField = xOpCode.Value;
      XS.Comment("Operand type: " + aOpCode.StackPopTypes[1].ToString());
      DoExecute(Assembler, aMethod, xField, DebugEnabled, TypeIsReferenceType(aOpCode.StackPopTypes[1]));
    }

    public static void DoExecute(Cosmos.Assembler.Assembler aAssembler,  MethodInfo aMethod, string aFieldId, Type aDeclaringObject, bool aNeedsGC, bool debugEnabled) {
      var xType = aMethod.MethodBase.DeclaringType;

      var xFields = GetFieldsInfo(aDeclaringObject, false);
      var xFieldInfo = (from item in xFields
                        where item.Id == aFieldId
                        select item).Single();
      var xActualOffset = Ldfld.GetFieldOffset(aDeclaringObject, aFieldId);
      var xSize = xFieldInfo.Size;
      XS.Comment("Field: " + xFieldInfo.Id);
      XS.Comment("Type: " + xFieldInfo.FieldType.ToString());
      XS.Comment("Size: " + xFieldInfo.Size);
      XS.Comment("Offset: " + xActualOffset + " (includes object header)");

      uint xRoundedSize = Align(xSize, 4);
      if (aNeedsGC)
      {
        DoNullReferenceCheck(aAssembler, debugEnabled, (int)xRoundedSize + 4);
      }
      else
      {
        DoNullReferenceCheck(aAssembler, debugEnabled, (int)xRoundedSize);
      }

      XS.Comment("After Nullref check");

      if (aNeedsGC)
      {
        XS.Set(XSRegisters.ECX, XSRegisters.ESP, sourceDisplacement: (int)xRoundedSize + 4);
      }
      else
      {
        XS.Set(XSRegisters.ECX, XSRegisters.ESP, sourceDisplacement: (int)xRoundedSize);
      }

      if (xActualOffset != 0)
      {
        XS.Add(XSRegisters.ECX, (uint)(xActualOffset));
      }

      //TODO: Can't we use an x86 op to do a byte copy instead and be faster?
      for (int i = 0; i < (xSize / 4); i++) {
        XS.Pop(XSRegisters.EAX);
        XS.Set(XSRegisters.ECX, XSRegisters.EAX, destinationDisplacement: (int)((i * 4)));
      }

      switch (xSize % 4) {
        case 1: {
            XS.Pop(XSRegisters.EAX);
            new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.ECX, DestinationIsIndirect = true, DestinationDisplacement = (int)((xSize / 4) * 4), SourceReg = CPUx86.RegistersEnum.AL };
            break;
          }
        case 2: {
            XS.Pop(XSRegisters.EAX);
            new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.ECX, DestinationIsIndirect = true, DestinationDisplacement = (int)((xSize / 4) * 4), SourceReg = CPUx86.RegistersEnum.AX };
            break;
          }
		    case 3: {
				    XS.Pop(XSRegisters.EAX);
				    // move 2 lower bytes
				    new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.ECX, DestinationIsIndirect = true, DestinationDisplacement = (int)((xSize / 4) * 4), SourceReg = CPUx86.RegistersEnum.AX };
				    // shift third byte to lowest
				    XS.ShiftRight(XSRegisters.EAX, 16);
				    new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.ECX, DestinationIsIndirect = true, DestinationDisplacement = (int)((xSize / 4) * 4) + 2, SourceReg = CPUx86.RegistersEnum.AL };
				    break;
			    }
        case 0: {
            break;
          }
        default:
          throw new Exception("Remainder size " + (xSize % 4) + " not supported!");
      }
      XS.Add(XSRegisters.ESP, 4);
      if (aNeedsGC)
      {
        XS.Add(XSRegisters.ESP, 4);
      }
    }

    public static void DoExecute(Cosmos.Assembler.Assembler aAssembler, MethodInfo aMethod, SysReflection.FieldInfo aField, bool debugEnabled, bool aNeedsGC)
    {
      DoExecute(aAssembler, aMethod, aField.GetFullName(), aField.DeclaringType, aNeedsGC, debugEnabled);
    }

  }
}
