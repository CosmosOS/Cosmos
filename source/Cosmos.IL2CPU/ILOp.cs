using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Cosmos.IL2CPU.Plugs;
using Cosmos.Assembler;
using CPU = Cosmos.Assembler.x86;
using Cosmos.IL2CPU.ILOpCodes;
using Cosmos.Debug.Common;
using Cosmos.IL2CPU.X86.IL;
using System.Runtime.InteropServices;
using FieldInfo = Cosmos.IL2CPU.X86.IL.FieldInfo;
using Label = Cosmos.Assembler.Label;

namespace Cosmos.IL2CPU {
  public abstract class ILOp {
    public static PlugManager mPlugManager;
    protected readonly Cosmos.Assembler.Assembler Assembler;

    protected ILOp(Cosmos.Assembler.Assembler aAsmblr) {
      Assembler = aAsmblr;
    }

      public bool DebugEnabled;

    // This is called execute and not assemble, as the scanner
    // could be used for other things, profiling, analysis, reporting, etc
    public abstract void Execute(MethodInfo aMethod, ILOpCode aOpCode);

    public static string GetTypeIDLabel(Type aType) {
      return "VMT__TYPE_ID_HOLDER__" + DataMember.FilterStringForIncorrectChars(LabelName.GetFullName(aType) + " ASM_IS__" + aType.Assembly.GetName().Name);
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
      return LabelName.Get(aMethod);
    }

    public static string GetMethodLabel(MethodInfo aMethod) {
      if (aMethod.PluggedMethod != null) {
        return "PLUG_FOR___" + GetMethodLabel(aMethod.PluggedMethod.MethodBase);
      } else {
        return GetMethodLabel(aMethod.MethodBase);
      }
    }

    public static string GetLabel(MethodInfo aMethod, int aPos) {
      return LabelName.Get(GetMethodLabel(aMethod), aPos);
    }

    public override string ToString() {
      return GetType().Name;
    }

    protected static void Jump_Exception(MethodInfo aMethod) {
      // todo: port to numeric labels
      new CPU.Jump { DestinationLabel = GetMethodLabel(aMethod) + AppAssembler.EndOfMethodLabelNameException };
    }

    protected static void Jump_End(MethodInfo aMethod) {
      new CPU.Jump { DestinationLabel = GetMethodLabel(aMethod) + AppAssembler.EndOfMethodLabelNameNormal };
    }

    public static uint GetStackCountForLocal(MethodInfo aMethod, LocalVariableInfo aField) {
      var xSize = SizeOfType(aField.LocalType);
      var xResult = xSize / 4;
      if (xSize % 4 != 0) {
        xResult++;
      }
      return xResult;
    }

    public static uint GetEBPOffsetForLocal(MethodInfo aMethod, int localIndex) {
      var xBody = aMethod.MethodBase.GetMethodBody();
      uint xOffset = 4;
      for (int i = 0; i < xBody.LocalVariables.Count; i++) {
        if (i == localIndex) {
          break;
        }
        var xField = xBody.LocalVariables[i];
        xOffset += GetStackCountForLocal(aMethod, xField) * 4;
      }
      return xOffset;
    }

    public static uint SizeOfType(Type aType) {
      if (aType.FullName == "System.Void") {
        return 0;
      } else if ((!aType.IsValueType && aType.IsClass) || aType.IsInterface) {
        return 4;
      }
      if (aType.IsByRef) {
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
        return (uint)(from item in GetFieldsInfo(aType)
                      select (int)item.Size).Sum();
      }
      return 4;
    }

    public static bool TypeIsFloat(Type type)
    {
      return type == typeof(Single)
             || type == typeof(Double);
    }

    public static uint GetEBPOffsetForLocalForDebugger(MethodInfo aMethod, int localIndex) {
      // because the memory is readed in positiv direction, we need to add additional size if greater than 4
      uint xOffset = GetEBPOffsetForLocal(aMethod, localIndex);
      var xBody = aMethod.MethodBase.GetMethodBody();
      var xField = xBody.LocalVariables[localIndex];
      xOffset += GetStackCountForLocal(aMethod, xField) * 4 - 4;
      return xOffset;
    }

    protected void ThrowNotImplementedException(string aMessage) {
      new CPU.Push {
        DestinationRef = Cosmos.Assembler.ElementReference.New(LdStr.GetContentsArrayName(aMessage))
      };
      new CPU.Call {
        DestinationLabel = LabelName.Get(typeof(ExceptionHelper).GetMethod("ThrowNotImplemented", BindingFlags.Static | BindingFlags.Public))
      };
    }

    protected void ThrowOverflowException() {
      new CPU.Call {
        DestinationLabel = LabelName.Get(
            typeof(ExceptionHelper).GetMethod("ThrowOverflow", BindingFlags.Static | BindingFlags.Public, null, new Type[] { }, null))
      };
    }

    private static void DoGetFieldsInfo(Type aType, List<X86.IL.FieldInfo> aFields) {
      var xCurList = new Dictionary<string, X86.IL.FieldInfo>();
      var xFields = (from item in aType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                     orderby item.Name, item.DeclaringType.ToString()
                     select item).ToArray();
      for (int i = 0; i < xFields.Length; i++) {
        var xField = xFields[i];
        // todo: should be possible to have GetFields only return fields from a given type, thus removing need of next statement
        if (xField.DeclaringType != aType) {
          continue;
        }

        string xId = xField.GetFullName();

        var xInfo = new X86.IL.FieldInfo(xId, SizeOfType(xField.FieldType), aType, xField.FieldType);
        xInfo.IsStatic = xField.IsStatic;
        xInfo.Field = xField;

        var xFieldOffsetAttrib = xField.GetCustomAttributes(typeof(FieldOffsetAttribute), true).FirstOrDefault() as FieldOffsetAttribute;
        if (xFieldOffsetAttrib != null) {
          xInfo.Offset = (uint)xFieldOffsetAttrib.Value;
        }

        aFields.Add(xInfo);
        xCurList.Add(xId, xInfo);
      }

      // now check plugs
      IDictionary<string, PlugFieldAttribute> xPlugFields;
      if (mPlugManager.PlugFields.TryGetValue(aType, out xPlugFields)) {
        foreach (var xPlugField in xPlugFields) {
          X86.IL.FieldInfo xPluggedField = null;
          if (xCurList.TryGetValue(xPlugField.Key, out xPluggedField)) {
            // plugfield modifies an already existing field

            // TODO: improve.
            if (xPlugField.Value.IsExternalValue) {
              xPluggedField.IsExternalValue = true;
              xPluggedField.FieldType = xPluggedField.FieldType.MakePointerType();
              xPluggedField.Size = 4;
            }
          } else {
            xPluggedField = new X86.IL.FieldInfo(xPlugField.Value.FieldId, SizeOfType(xPlugField.Value.FieldType), aType, xPlugField.Value.FieldType);
            aFields.Add(xPluggedField);
          }
        }
      }

      if (aType.BaseType != null) {
        DoGetFieldsInfo(aType.BaseType, aFields);
      }
    }

    public static List<X86.IL.FieldInfo> GetFieldsInfo(Type aType) {
      var xResult = new List<X86.IL.FieldInfo>(16);
      DoGetFieldsInfo(aType, xResult);
      xResult.Reverse();
      uint xOffset = 0;
      foreach (var xInfo in xResult) {
        if (!xInfo.IsOffsetSet && !xInfo.IsStatic) {
          xInfo.Offset = xOffset;
          xOffset += xInfo.Size;
        }
      }
      var xDebugInfs = new List<FIELD_INFO>();
      foreach (var xInfo in xResult) {
        if (!xInfo.IsStatic)
        {
          xDebugInfs.Add(new FIELD_INFO()
                         {
                           TYPE = xInfo.FieldType.AssemblyQualifiedName,
                           OFFSET = (int)xInfo.Offset,
                           NAME = GetNameForField(xInfo),
                         });
        }
      }
      DebugInfo.CurrentInstance.WriteFieldInfoToFile(xDebugInfs);
      List<DebugInfo.Field_Map> xFieldMapping = new List<DebugInfo.Field_Map>();
      GetFieldMapping(xResult, xFieldMapping, aType);
      DebugInfo.CurrentInstance.WriteFieldMappingToFile(xFieldMapping);
      return xResult;
    }

    private static void GetFieldMapping(List<X86.IL.FieldInfo> aFieldInfs, List<DebugInfo.Field_Map> aFieldMapping, Type aType) {
      DebugInfo.Field_Map xFMap = new DebugInfo.Field_Map();
      xFMap.TypeName = aType.AssemblyQualifiedName;
      foreach (var xInfo in aFieldInfs) {
        xFMap.FieldNames.Add(GetNameForField(xInfo));
      }
      aFieldMapping.Add(xFMap);
    }

    private static string GetNameForField(X86.IL.FieldInfo inf) {
      // First we need to separate out the
      // actual name of field from the type of the field.
      int loc = inf.Id.IndexOf(' ');
      if (loc >= 0) {
        string fName = inf.Id.Substring(loc, inf.Id.Length - loc);
        return inf.DeclaringType.AssemblyQualifiedName + fName;
      } else {
        return inf.Id;
      }
    }

    protected static uint GetStorageSize(Type aType) {
      return (from item in GetFieldsInfo(aType)
              where !item.IsStatic
              orderby item.Offset descending
              select item.Offset + item.Size).FirstOrDefault();
    }

    public static void EmitExceptionLogic(Cosmos.Assembler.Assembler aAssembler, MethodInfo aMethodInfo, ILOpCode aCurrentOpCode, bool aDoTest, Action aCleanup) {
      EmitExceptionLogic(aAssembler, aMethodInfo, aCurrentOpCode, aDoTest, aCleanup, ILOp.GetLabel(aMethodInfo, aCurrentOpCode.NextPosition));
    }
    public static void EmitExceptionLogic(Cosmos.Assembler.Assembler aAssembler, MethodInfo aMethodInfo, ILOpCode aCurrentOpCode, bool aDoTest, Action aCleanup, string aJumpTargetNoException) {
      string xJumpTo = null;
      if (aCurrentOpCode != null && aCurrentOpCode.CurrentExceptionHandler != null) {
        // todo add support for nested handlers, see comment in Engine.cs
        //if (!((aMethodInfo.CurrentHandler.HandlerOffset < aCurrentOpOffset) || (aMethodInfo.CurrentHandler.HandlerLength + aMethodInfo.CurrentHandler.HandlerOffset) <= aCurrentOpOffset)) {
        new Comment(String.Format("CurrentOffset = {0}, HandlerStartOffset = {1}", aCurrentOpCode.Position, aCurrentOpCode.CurrentExceptionHandler.HandlerOffset));
        if (aCurrentOpCode.CurrentExceptionHandler.HandlerOffset > aCurrentOpCode.Position) {
          switch (aCurrentOpCode.CurrentExceptionHandler.Flags) {
            case ExceptionHandlingClauseOptions.Clause: {
                xJumpTo = ILOp.GetLabel(aMethodInfo, aCurrentOpCode.CurrentExceptionHandler.HandlerOffset);
                break;
              }
            case ExceptionHandlingClauseOptions.Finally: {
                xJumpTo = ILOp.GetLabel(aMethodInfo, aCurrentOpCode.CurrentExceptionHandler.HandlerOffset);
                break;
              }
            default: {
                throw new Exception("ExceptionHandlerType '" + aCurrentOpCode.CurrentExceptionHandler.Flags.ToString() + "' not supported yet!");
              }
          }
        }
      }
      // if aDoTest is true, we check ECX for exception flags
      if (!aDoTest) {
        //new CPU.Call("_CODE_REQUESTED_BREAK_");
        if (xJumpTo == null) {
          Jump_Exception(aMethodInfo);
        } else {
          new CPU.Jump { DestinationLabel = xJumpTo };
        }

      } else {
        new CPU.Test { DestinationReg = CPU.Registers.ECX, SourceValue = 2 };

        if (aCleanup != null) {
          new CPU.ConditionalJump { Condition = CPU.ConditionalTestEnum.Equal, DestinationLabel = aJumpTargetNoException };
          aCleanup();
          if (xJumpTo == null) {
            new CPU.ConditionalJump { Condition = CPU.ConditionalTestEnum.NotEqual, DestinationLabel = GetMethodLabel(aMethodInfo) + AppAssembler.EndOfMethodLabelNameException };
          } else {
            new CPU.ConditionalJump { Condition = CPU.ConditionalTestEnum.NotEqual, DestinationLabel = xJumpTo };
          }
        } else {
          if (xJumpTo == null) {
            new CPU.ConditionalJump { Condition = CPU.ConditionalTestEnum.NotEqual, DestinationLabel = GetMethodLabel(aMethodInfo) + AppAssembler.EndOfMethodLabelNameException };
          } else {
            new CPU.ConditionalJump { Condition = CPU.ConditionalTestEnum.NotEqual, DestinationLabel = xJumpTo };
          }
        }
      }
    }

    public static bool IsIntegerSigned(Type aType) {
      switch (aType.FullName) {
        case "System.SByte":
        case "System.Int16":
        case "System.Int32":
        case "System.Int64":
          //TODO not sure about this case "System.IntPtr":
          //TODO not sure aobut this case "System.Enum":
          return true;
      }
      return false;
    }

    public static void DoNullReferenceCheck(Assembler.Assembler assembler, bool debugEnabled, uint stackOffsetToCheck)
    {
      if (stackOffsetToCheck != Align(stackOffsetToCheck, 4))
      {
        throw new Exception("Stack offset not aligned!");
      }
      if (debugEnabled)
      {
        new CPU.Compare {DestinationReg = CPU.RegistersEnum.ESP, DestinationDisplacement = (int) stackOffsetToCheck, DestinationIsIndirect = true, SourceValue = 0};
        new CPU.ConditionalJump {DestinationLabel = ".AfterNullCheck", Condition = CPU.ConditionalTestEnum.NotEqual};
        new CPU.ClrInterruptFlag();
        // don't remove the call. It seems pointless, but we need it to retrieve the EIP value
        new CPU.Call {DestinationLabel = ".NullCheck_GetCurrAddress"};
        new Assembler.Label(".NullCheck_GetCurrAddress");
        new CPU.Pop {DestinationReg = CPU.RegistersEnum.EAX};
        new CPU.Mov {DestinationRef = ElementReference.New("DebugStub_CallerEIP"), DestinationIsIndirect = true, SourceReg = CPU.RegistersEnum.EAX};
        new CPU.Call {DestinationLabel = "DebugStub_SendNullReferenceOccurred"};
        new CPU.Halt();
        new Label(".AfterNullCheck");
      }
    }

    public static FieldInfo ResolveField(Type aDeclaringType, string aField, bool aOnlyInstance)
    {
      var xFields = GetFieldsInfo(aDeclaringType);
      var xFieldInfo = (from item in xFields
                        where item.Id == aField
                        && (!aOnlyInstance || item.IsStatic == false)
                        select item).SingleOrDefault();
      if (xFieldInfo == null)
      {
        Console.WriteLine("Following fields have been found on '{0}'", aDeclaringType.FullName);
        foreach (var xField in xFields)
        {
          Console.WriteLine("\t'{0}'", xField.Id);
        }
        throw new Exception(string.Format("Field '{0}' not found on type '{1}'", aField, aDeclaringType.FullName));
      }
      return xFieldInfo;
    }


  }
}
