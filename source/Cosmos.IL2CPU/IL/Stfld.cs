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
      DoExecute(Assembler, aMethod, xField, DebugEnabled);
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
      DoNullReferenceCheck(aAssembler, debugEnabled, xRoundedSize);
      XS.Comment("After Nullref check");
      XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), sourceDisplacement: (int)xRoundedSize);
      // ECX contains the object pointer now
      if (aNeedsGC)
      {
        // for reference types (or boxed types), ECX actually contains the handle now, so we need to convert it to a memory address
        XS.Comment("Dereference memory handle now");
        new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.ECX, SourceReg = CPUx86.RegistersEnum.ECX, SourceIsIndirect = true };
      }
      if (debugEnabled)
      {
        XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX));
        XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX));
      }
      XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX), (uint)(xActualOffset));
      //TODO: Can't we use an x86 op to do a byte copy instead and be faster?
      for (int i = 0; i < (xSize / 4); i++) {
        XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
        new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.ECX, DestinationIsIndirect = true, DestinationDisplacement = (int)((i * 4)), SourceReg = CPUx86.RegistersEnum.EAX };
      }

      switch (xSize % 4) {
        case 1: {
            XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
            new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.ECX, DestinationIsIndirect = true, DestinationDisplacement = (int)((xSize / 4) * 4), SourceReg = CPUx86.RegistersEnum.AL };
            break;
          }
        case 2: {
            XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
            new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.ECX, DestinationIsIndirect = true, DestinationDisplacement = (int)((xSize / 4) * 4), SourceReg = CPUx86.RegistersEnum.AX };
            break;
          }
		case 3: {
				XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
				// move 2 lower bytes
				new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.ECX, DestinationIsIndirect = true, DestinationDisplacement = (int)((xSize / 4) * 4), SourceReg = CPUx86.RegistersEnum.AX };
				// shift third byte to lowest
				XS.ShiftRight(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), 16);
				new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.ECX, DestinationIsIndirect = true, DestinationDisplacement = (int)((xSize / 4) * 4) + 2, SourceReg = CPUx86.RegistersEnum.AL };
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
            XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX));
            XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
            new CPUx86.Call { DestinationLabel = LabelName.Get(GCImplementationRefs.DecRefCountRef) };
            new CPUx86.Call { DestinationLabel = LabelName.Get(GCImplementationRefs.DecRefCountRef) };
          }
#endif
      XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), 4);
    }

    public static void DoExecute(Cosmos.Assembler.Assembler aAssembler, MethodInfo aMethod, SysReflection.FieldInfo aField, bool debugEnabled)
    {
      bool xNeedsGC = aField.DeclaringType.IsClass && !aField.DeclaringType.IsValueType;

      DoExecute(aAssembler, aMethod, aField.GetFullName(), aField.DeclaringType, xNeedsGC, debugEnabled);
    }

  }
}
