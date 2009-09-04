using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU {
  public abstract class ILOp {
    protected readonly Assembler Assembler;
    protected ILOp(Assembler aAsmblr) {
      Assembler = aAsmblr;
    }

    // This is called execute and not assemble, as the scanner
    // could be used for other things, profiling, analysis, reporting, etc
    public abstract void Execute(MethodInfo aMethod, ILOpCode aOpCode);

    protected static uint SizeOfType(Type aType) {
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
