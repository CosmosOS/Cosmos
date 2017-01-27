using System;
using System.Linq;

using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;

using SysReflection = System.Reflection;

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
        XS.Set(ECX, ESP, sourceDisplacement: (int)xRoundedSize + 4);
      }
      else
      {
        XS.Set(ECX, ESP, sourceDisplacement: (int)xRoundedSize);
      }

      if (xActualOffset != 0)
      {
        XS.Add(ECX, (uint)(xActualOffset));
      }

      //TODO: Can't we use an x86 op to do a byte copy instead and be faster?
      for (int i = 0; i < (xSize / 4); i++) {
        XS.Pop(EAX);
        XS.Set(ECX, EAX, destinationDisplacement: (int)((i * 4)));
      }

      switch (xSize % 4) {
        case 0: {
            break;
          }
        case 1: {
            XS.Pop(EAX);
            XS.Set(ECX, AL, destinationDisplacement: (int)((xSize / 4) * 4));
            //new CPUx86.Mov { DestinationReg = ECX, DestinationIsIndirect = true, DestinationDisplacement = (int)((xSize / 4) * 4), SourceReg = AL };
            break;
          }
        case 2: {
            XS.Pop(EAX);
            XS.Set(ECX, AX, destinationDisplacement: (int)((xSize / 4) * 4));
            //new CPUx86.Mov { DestinationReg = ECX, DestinationIsIndirect = true, DestinationDisplacement = (int)((xSize / 4) * 4), SourceReg = AX };
            break;
          }
		    case 3: {
				    XS.Pop(EAX);
            // move 2 lower bytes
            XS.Set(ECX, AX, destinationDisplacement: (int)((xSize / 4) * 4));
            //new CPUx86.Mov { DestinationReg = ECX, DestinationIsIndirect = true, DestinationDisplacement = (int)((xSize / 4) * 4), SourceReg = AX };
				    // shift third byte to lowest
				    XS.ShiftRight(EAX, 16);
            XS.Set(ECX, AL, destinationDisplacement: (int)((xSize / 4) * 4 + 2));
            //new CPUx86.Mov { DestinationReg = ECX, DestinationIsIndirect = true, DestinationDisplacement = (int)((xSize / 4) * 4) + 2, SourceReg = AL };
				    break;
			    }
        default:
          throw new Exception("Remainder size " + (xSize % 4) + " not supported!");
      }
      XS.Add(ESP, 4);
      if (aNeedsGC)
      {
        XS.Add(ESP, 4);
      }
    }

    public static void DoExecute(Cosmos.Assembler.Assembler aAssembler, MethodInfo aMethod, SysReflection.FieldInfo aField, bool debugEnabled, bool aNeedsGC)
    {
      DoExecute(aAssembler, aMethod, aField.GetFullName(), aField.DeclaringType, aNeedsGC, debugEnabled);
    }

  }
}
