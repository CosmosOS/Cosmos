using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.IL2CPU {
  public abstract class ILOp {
    public static IDictionary<Type, IDictionary<string, PlugFieldAttribute>> mPlugFields;

    protected readonly Assembler Assembler;
    protected ILOp(Assembler aAsmblr) {
      Assembler = aAsmblr;
    }

    // This is called execute and not assemble, as the scanner
    // could be used for other things, profiling, analysis, reporting, etc
    public abstract void Execute(MethodInfo aMethod, ILOpCode aOpCode);

    public static string GetTypeIDLabel(Type aType) {
      return "VMT__TYPE_ID_HOLDER__" + DataMember.FilterStringForIncorrectChars(MethodInfoLabelGenerator.GetFullName(aType) + " ASM_IS__" + aType.Assembly.GetName().Name);
    }

    public static uint Align(uint aSize, uint aAlign) {
      var xSize = aSize;
      if ((xSize % aAlign) != 0) {
        xSize += aAlign - (xSize % aAlign);
      }
      return xSize;
    }

    public static string GetLabel(MethodInfo aMethod, ILOpCode aOpCode) {
      return GetLabel(aMethod, aOpCode.Position);
    }

    public static string GetMethodLabel(MethodBase aMethod) {
      return MethodInfoLabelGenerator.GenerateLabelName(aMethod);
    }

    public static string GetMethodLabel(MethodInfo aMethod) {
      if (aMethod.PluggedMethod != null) {
        return "PLUG_FOR___" + GetMethodLabel(aMethod.PluggedMethod.MethodBase);
      } else {
        return GetMethodLabel(aMethod.MethodBase);
      }
    }

    public static string GetLabel(MethodInfo aMethod, int aPos) {
      return GetMethodLabel(aMethod) + "__DOT__" + aPos.ToString("X8").ToUpper();
    }

    public static uint SizeOfType(Type aType) {
      if (aType.FullName == "System.Void") {
        return 0;
      } else if ((!aType.IsValueType && aType.IsClass) || aType.IsInterface) {
        return 4;
      }
      switch (aType.FullName) {
        case "System.Char":
          return 2;
        case "System.Byte":
        case "System.SByte":
          return 1;
        case "System.UInt16":
        case "System.Int16":
          return 2;
        case "System.UInt32":
        case "System.Int32":
          return 4;
        case "System.UInt64":
        case "System.Int64":
          return 8;
        //TODO: for now hardcode IntPtr and UIntPtr to be 32-bit
        case "System.UIntPtr":
        case "System.IntPtr":
          return 4;
        case "System.Boolean":
          return 1;
        case "System.Single":
          return 4;
        case "System.Double":
          return 8;
        case "System.Decimal":
          return 16;
        case "System.Guid":
          return 16;
        case "System.Enum":
          return 4;
        case "System.DateTime":
          return 8;
      }
      if (aType.FullName != null && aType.FullName.EndsWith("*")) {
        // pointer
        return 4;
      }
      // array
      //TypeSpecification xTypeSpec = aType as TypeSpecification;
      //if (xTypeSpec != null) {
      //    return 4;
      //}
      if (aType.IsEnum) {
        return SizeOfType(aType.GetField("value__").FieldType);
      }
      if (aType.IsValueType) {
        var xSla = aType.StructLayoutAttribute;
        if (xSla != null) {
          if (xSla.Size > 0) {
            return (uint)xSla.Size;
          }
        }
      }
      return 4;
    }
  }
}
