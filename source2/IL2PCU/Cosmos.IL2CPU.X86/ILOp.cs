using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPU = Cosmos.IL2CPU.X86;
using Cosmos.IL2CPU.ILOpCodes;
using System.Reflection;
using Cosmos.IL2CPU.X86.IL;
using Indy.IL2CPU.IL;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Plugs;

namespace Cosmos.IL2CPU.X86 {
  public abstract class ILOp: Cosmos.IL2CPU.ILOp {
    protected new readonly Assembler Assembler;

    protected ILOp(Cosmos.IL2CPU.Assembler aAsmblr)
      : base(aAsmblr) {
      Assembler = (Assembler)aAsmblr;
    }

    protected void Jump_Exception(MethodInfo aMethod) {
      // todo: port to numeric labels
      new CPU.Jump { DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName(aMethod.MethodBase) + AssemblerNasm.EndOfMethodLabelNameException };
    }

    protected void Jump_End(MethodInfo aMethod) {
      new CPU.Jump { DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName(aMethod.MethodBase) + AssemblerNasm.EndOfMethodLabelNameNormal };
    }

    protected uint GetStackCountForLocal(MethodInfo aMethod, LocalVariableInfo aField) {
      var xSize = SizeOfType(aField.LocalType);
      var xResult = xSize / 4;
      if (xSize % 4 != 0) {
        xResult++;
      }
      return xResult;
    }

    protected uint GetEBPOffsetForLocal(MethodInfo aMethod, OpVar aOp) {
      var xBody = aMethod.MethodBase.GetMethodBody();
      uint xOffset = 4;
      for (int i = 0; i < xBody.LocalVariables.Count; i++) {
        if (i == aOp.Value) {
          break;
        }
        var xField = xBody.LocalVariables[i];
        xOffset += GetStackCountForLocal(aMethod, xField);
      }
      return xOffset;
    }


    public static uint Align(uint aSize, uint aAlign) {
      var xSize = aSize;
      if ((xSize % aAlign) != 0) {
        xSize += aAlign - (xSize % aAlign);
      }
      return xSize;
    }

    protected void ThrowNotImplementedException(string aMessage) {
      new CPU.Push {
        DestinationRef = ElementReference.New(LdStr.GetContentsArrayName(aMessage))
      };
      new CPU.Call {
        DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName(typeof(ExceptionHelper).GetMethod("ThrowNotImplemented", BindingFlags.Static | BindingFlags.Public))
      };
    }

    private static void DoGetFieldsInfo(Type aType, List<IL.FieldInfo> aFields) {
      var xCurList = new Dictionary<string, IL.FieldInfo>();
      foreach (var xField in aType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
        // todo: should be possible to have GetFields only return fields from a given type, thus removing need of next statement
        if (xField.DeclaringType != aType) {
          continue;
        }

        var xId = xField.GetFullName();
        var xInfo = new IL.FieldInfo(xId, SizeOfType(xField.FieldType), aType, xField.FieldType);
        aFields.Add(xInfo);
        xCurList.Add(xId, xInfo);
      }

      // now check plugs
      IDictionary<string, PlugFieldAttribute> xPlugFields;
      if (mPlugFields.TryGetValue(aType, out xPlugFields)) {
        foreach (var xPlugField in xPlugFields) {
          IL.FieldInfo xPluggedField = null;
          if (xCurList.TryGetValue(xPlugField.Key, out xPluggedField)) {
            // plugfield modifies an already existing field
            
            // TODO: improve.
            if (xPlugField.Value.IsExternalValue) {
              xPluggedField.IsExternalValue = true;
              xPluggedField.FieldType = xPluggedField.FieldType.MakePointerType();
              xPluggedField.Size = 4;
            }
          } else {
            xPluggedField = new IL.FieldInfo(xPlugField.Value.FieldId, SizeOfType(xPlugField.Value.FieldType), aType, xPlugField.Value.FieldType);
            aFields.Add(xPluggedField);
          }
        }
      }

      if (aType.BaseType != null) {
        DoGetFieldsInfo(aType.BaseType, aFields);
      }
    }
    protected static List<IL.FieldInfo> GetFieldsInfo(Type aType) {
      var xResult = new List<IL.FieldInfo>();
      DoGetFieldsInfo(aType,xResult);
      xResult.Reverse();
      uint xOffset = 0;
      foreach (var xInfo in xResult) {
        xInfo.Offset = xOffset;
        xOffset += xInfo.Size;
      }
      return xResult;
    }
  }
}