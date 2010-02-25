using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPU = Cosmos.IL2CPU.X86;
using Cosmos.IL2CPU.ILOpCodes;
using System.Reflection;
using Cosmos.IL2CPU.X86.IL;
using Cosmos.IL2CPU.IL;
using CPUx86 = Cosmos.IL2CPU.X86;
using Cosmos.IL2CPU.Plugs;
using System.Runtime.InteropServices;

namespace Cosmos.IL2CPU.X86 {
  public abstract class ILOp: Cosmos.IL2CPU.ILOp {
    protected new readonly Assembler Assembler;

    protected ILOp(Cosmos.IL2CPU.Assembler aAsmblr)
      : base(aAsmblr) {
      Assembler = (Assembler)aAsmblr;
    }

    protected static void Jump_Exception(MethodInfo aMethod) {
      // todo: port to numeric labels
      new CPU.Jump { DestinationLabel = GetMethodLabel(aMethod) + AssemblerNasm.EndOfMethodLabelNameException };
    }

    protected static void Jump_End(MethodInfo aMethod) {
      new CPU.Jump { DestinationLabel = GetMethodLabel(aMethod) + AssemblerNasm.EndOfMethodLabelNameNormal };
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
          if (i == aOp.Value)
          {
              break;
          }
          var xField = xBody.LocalVariables[i];
        xOffset += GetStackCountForLocal(aMethod, xField) * 4;
      }
      return xOffset;
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
      var xFields = (from item in aType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                orderby item.Name, item.DeclaringType.ToString()
                                select item).ToArray();
      for(int i = 0; i < xFields.Length;i++){
        var xField = xFields[i];
        // todo: should be possible to have GetFields only return fields from a given type, thus removing need of next statement
        if (xField.DeclaringType != aType) {
          continue;
        }

        var xId = xField.GetFullName();
        var xInfo = new IL.FieldInfo(xId, SizeOfType(xField.FieldType), aType, xField.FieldType);
        var xFieldOffsetAttrib = xField.GetCustomAttributes(typeof(FieldOffsetAttribute), true).FirstOrDefault() as FieldOffsetAttribute;
        if (xFieldOffsetAttrib != null)
        {
            xInfo.Offset = (uint)xFieldOffsetAttrib.Value;
        }
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
          if (!xInfo.IsOffsetSet)
          {
              xInfo.Offset = xOffset;
          }
        xOffset += xInfo.Size;
      }
      return xResult;
    }

    protected static uint GetStorageSize(Type aType) {
      return (from item in GetFieldsInfo(aType)
              orderby item.Offset descending
              select item.Offset + item.Size).FirstOrDefault();
    }

    public static void EmitExceptionLogic(Assembler aAssembler, MethodInfo aMethodInfo, ILOpCode aCurrentOpCode, bool aDoTest, Action aCleanup)
    {
        string xJumpTo = null;
        if (aCurrentOpCode != null && aCurrentOpCode.CurrentExceptionHandler != null)
        {
            // todo add support for nested handlers, see comment in Engine.cs
            //if (!((aMethodInfo.CurrentHandler.HandlerOffset < aCurrentOpOffset) || (aMethodInfo.CurrentHandler.HandlerLength + aMethodInfo.CurrentHandler.HandlerOffset) <= aCurrentOpOffset)) {
            new Comment(String.Format("CurrentOffset = {0}, HandlerStartOffset = {1}", aCurrentOpCode.Position, aCurrentOpCode.CurrentExceptionHandler.HandlerOffset));
            if (aCurrentOpCode.CurrentExceptionHandler.HandlerOffset > aCurrentOpCode.Position)
            {
                switch (aCurrentOpCode.CurrentExceptionHandler.Flags)
                {
                    case ExceptionHandlingClauseOptions.Clause:
                        {
                            xJumpTo = ILOp.GetLabel(aMethodInfo, aCurrentOpCode.CurrentExceptionHandler.HandlerOffset);
                            break;
                        }
                    case ExceptionHandlingClauseOptions.Finally:
                        {
                            xJumpTo = ILOp.GetLabel(aMethodInfo, aCurrentOpCode.CurrentExceptionHandler.HandlerOffset);
                            break;
                        }
                    default:
                        {
                            throw new Exception("ExceptionHandlerType '" + aCurrentOpCode.CurrentExceptionHandler.Flags.ToString() + "' not supported yet!");
                        }
                }
            }
        }
        if (!aDoTest)
        {
            //new CPUx86.Call("_CODE_REQUESTED_BREAK_");
            if (xJumpTo == null)
            {
                Jump_End(aMethodInfo);
            }
            else
            {
                new CPUx86.Jump { DestinationLabel = xJumpTo };
            }

        }
        else
        {
            new CPUx86.Test { DestinationReg = CPUx86.Registers.ECX, SourceValue = 2 };

            if (aCleanup != null)
            {
                new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Equal, DestinationLabel = ILOp.GetLabel(aMethodInfo, aCurrentOpCode.NextPosition) };
                aCleanup();
                if (xJumpTo == null)
                {
                    Jump_End(aMethodInfo);
                }
                else
                { new CPUx86.Jump { DestinationLabel = xJumpTo }; }
            }
            else
            {
                if (xJumpTo == null)
                {
                    Jump_End(aMethodInfo);
                }
                else
                { new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.NotEqual, DestinationLabel = xJumpTo }; }
            }
        }
    }
  }
}