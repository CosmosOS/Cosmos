using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Cosmos.IL2CPU.Plugs;
using Cosmos.Compiler.Assembler;

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
      return GetMethodLabel(aMethod) + "." + aPos.ToString("X8").ToUpper();
    }

    public override string ToString() {
      return GetType().Name;
    }

  }
}
