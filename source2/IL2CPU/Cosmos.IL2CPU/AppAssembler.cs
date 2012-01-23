using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using Cosmos.Assembler;
using Cosmos.Debug.Common;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.IL2CPU {
  public abstract class AppAssembler {
    protected ILOp[] mILOpsLo = new ILOp[256];
    protected ILOp[] mILOpsHi = new ILOp[256];

    private System.IO.TextWriter mLog;

    protected Cosmos.Assembler.Assembler mAssembler;
    protected AppAssembler(Cosmos.Assembler.Assembler assembler) {
      mAssembler = assembler;
      mLog = new System.IO.StreamWriter("Cosmos.Assembler.Log");
      InitILOps();
    }

    public bool ShouldOptimize = false;

    public Cosmos.Assembler.Assembler Assembler {
      get { return mAssembler; }
        set { mAssembler = value; }
    }

    public DebugInfo DebugInfo {
      get;
      set;
    }

    protected virtual void MethodBegin(MethodInfo aMethod) {
      new Comment("---------------------------------------------------------");
      new Comment("Assembly: " + aMethod.MethodBase.DeclaringType.Assembly.FullName);
      new Comment("Type: " + aMethod.MethodBase.DeclaringType.ToString());
      new Comment("Name: " + aMethod.MethodBase.Name);
      new Comment("Plugged: " + (aMethod.PlugMethod == null ? "No" : "Yes"));
    }

    protected virtual void MethodBegin(string aMethodName) {
      new Comment("---------------------------------------------------------");
      new Comment("Name: " + aMethodName);
    }

    protected virtual void MethodEnd(string aMethodName) {
      new Comment("End Method: " + aMethodName);
    }

    protected virtual void MethodEnd(MethodInfo aMethod) {
      new Comment("End Method: " + aMethod.MethodBase.Name);
    }

    public void ProcessMethod(MethodInfo aMethod, List<ILOpCode> aOpCodes) {
      // We check this here and not scanner as when scanner makes these
      // plugs may still have not yet been scanned that it will depend on.
      // But by the time we make it here, they have to be resolved.
      if (aMethod.Type == MethodInfo.TypeEnum.NeedsPlug && aMethod.PlugMethod == null) {
        throw new Exception("Method needs plug, but no plug was assigned.");
      }

      // todo: MtW: how to do this? we need some extra space.
      //		see ConstructLabel for extra info
      if (aMethod.UID > 0x00FFFFFF) {
        throw new Exception("Too many methods.");
      }

      MethodBegin(aMethod);
      mAssembler.Stack.Clear();
      mLog.WriteLine("Method '{0}'", aMethod.MethodBase.GetFullName());
      mLog.Flush();
      if (aMethod.MethodAssembler != null) {
        mLog.WriteLine("Emitted using MethodAssembler", aMethod.MethodBase.GetFullName());
        mLog.Flush();
        var xAssembler = (AssemblerMethod)Activator.CreateInstance(aMethod.MethodAssembler);
        xAssembler.AssembleNew(mAssembler, aMethod.PluggedMethod);
      } else if (aMethod.IsInlineAssembler) {
        mLog.WriteLine("Emitted using Inline MethodAssembler", aMethod.MethodBase.GetFullName());
        mLog.Flush();
        aMethod.MethodBase.Invoke("", new object[aMethod.MethodBase.GetParameters().Length]);
      } else {
        string xMethodLabelPrefix = ILOp.GetMethodLabel(aMethod.MethodBase);
        int xMethodID = DebugInfo.AddMethod(xMethodLabelPrefix);
        Cosmos.Assembler.Assembler.CurrentInstance.CurrentMethodID = xMethodID;

        foreach (var xOpCode in aOpCodes) {
          ushort xOpCodeVal = (ushort)xOpCode.OpCode;
          ILOp xILOp;
          if (xOpCodeVal <= 0xFF) {
            xILOp = mILOpsLo[xOpCodeVal];
          } else {
            xILOp = mILOpsHi[xOpCodeVal & 0xFF];
          }
          mLog.WriteLine("\t{0} {1}", mAssembler.Stack.Count, xILOp.GetType().Name);
          mLog.Flush();

          BeforeOp(aMethod, xOpCode);
          new Comment(xILOp.ToString());
          var xNextPosition = xOpCode.Position + 1;
          #region Exception handling support code
          ExceptionHandlingClause xCurrentHandler = null;
          var xBody = aMethod.MethodBase.GetMethodBody();
          // todo: add support for nested handlers using a stack or so..
          foreach (ExceptionHandlingClause xHandler in xBody.ExceptionHandlingClauses) {
            if (xHandler.TryOffset > 0) {
              if (xHandler.TryOffset <= xNextPosition && (xHandler.TryLength + xHandler.TryOffset) > xNextPosition) {
                if (xCurrentHandler == null) {
                  xCurrentHandler = xHandler;
                  continue;
                } else if (xHandler.TryOffset > xCurrentHandler.TryOffset && (xHandler.TryLength + xHandler.TryOffset) < (xCurrentHandler.TryLength + xCurrentHandler.TryOffset)) {
                  // only replace if the current found handler is narrower
                  xCurrentHandler = xHandler;
                  continue;
                }
              }
            }
            if (xHandler.HandlerOffset > 0) {
              if (xHandler.HandlerOffset <= xNextPosition && (xHandler.HandlerOffset + xHandler.HandlerLength) > xNextPosition) {
                if (xCurrentHandler == null) {
                  xCurrentHandler = xHandler;
                  continue;
                } else if (xHandler.HandlerOffset > xCurrentHandler.HandlerOffset && (xHandler.HandlerOffset + xHandler.HandlerLength) < (xCurrentHandler.HandlerOffset + xCurrentHandler.HandlerLength)) {
                  // only replace if the current found handler is narrower
                  xCurrentHandler = xHandler;
                  continue;
                }
              }
            }
            if ((xHandler.Flags & ExceptionHandlingClauseOptions.Filter) > 0) {
              if (xHandler.FilterOffset > 0) {
                if (xHandler.FilterOffset <= xNextPosition) {
                  if (xCurrentHandler == null) {
                    xCurrentHandler = xHandler;
                    continue;
                  } else if (xHandler.FilterOffset > xCurrentHandler.FilterOffset) {
                    // only replace if the current found handler is narrower
                    xCurrentHandler = xHandler;
                    continue;
                  }
                }
              }
            }
          }
          #endregion
          var xNeedsExceptionPush = (xCurrentHandler != null) && (((xCurrentHandler.HandlerOffset > 0 && xCurrentHandler.HandlerOffset == xOpCode.Position) || ((xCurrentHandler.Flags & ExceptionHandlingClauseOptions.Filter) > 0 && xCurrentHandler.FilterOffset > 0 && xCurrentHandler.FilterOffset == xOpCode.Position)) && (xCurrentHandler.Flags == ExceptionHandlingClauseOptions.Clause));
          if (xNeedsExceptionPush) {
            Push(DataMember.GetStaticFieldName(ExceptionHelperRefs.CurrentExceptionRef), true);
            mAssembler.Stack.Push(4, typeof(Exception));
          }

          xILOp.Execute(aMethod, xOpCode);

          AfterOp(aMethod, xOpCode);
          //mLog.WriteLine( " end: " + Stack.Count.ToString() );
        }
      }
      MethodEnd(aMethod);
    }

    protected virtual void BeforeOp(MethodInfo aMethod, ILOpCode aOpCode) { }
    protected virtual void AfterOp(MethodInfo aMethod, ILOpCode aOpCode) { }
    protected abstract void InitILOps();

    protected virtual void InitILOps(Type aAssemblerBaseOp) {
      foreach (var xType in aAssemblerBaseOp.Assembly.GetExportedTypes()) {
        if (xType.IsSubclassOf(aAssemblerBaseOp)) {
          var xAttribs = (OpCodeAttribute[])xType.GetCustomAttributes(typeof(OpCodeAttribute), false);
          foreach (var xAttrib in xAttribs) {
            var xOpCode = (ushort)xAttrib.OpCode;
            var xCtor = xType.GetConstructor(new Type[] { typeof(Cosmos.Assembler.Assembler) });
            var xILOp = (ILOp)xCtor.Invoke(new Object[] { Assembler });
            if (xOpCode <= 0xFF) {
              mILOpsLo[xOpCode] = xILOp;
            } else {
              mILOpsHi[xOpCode & 0xFF] = xILOp;
            }
          }
        }
      }
    }

    protected abstract void Push(uint aValue);
    protected abstract void Push(string aLabelName, bool isIndirect = false);
    protected abstract void Call(MethodBase aMethod);
    protected abstract void Move(string aDestLabelName, int aValue);
    protected abstract void Jump(string aLabelName);
    protected abstract int GetVTableEntrySize();

    public const string InitVMTCodeLabel = "___INIT__VMT__CODE____";
    public virtual void GenerateVMTCode(HashSet<Type> aTypesSet, HashSet<MethodBase> aMethodsSet, Func<Type, uint> aGetTypeID, Func<MethodBase, uint> aGetMethodUID) {
      // initialization
      MethodBegin(InitVMTCodeLabel);
      {
        var xSetTypeInfoRef = VTablesImplRefs.SetTypeInfoRef;
        var xSetMethodInfoRef = VTablesImplRefs.SetMethodInfoRef;
        var xLoadTypeTableRef = VTablesImplRefs.LoadTypeTableRef;
        var xTypesFieldRef = VTablesImplRefs.VTablesImplDef.GetField("mTypes",
                                                                           BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
        string xTheName = DataMember.GetStaticFieldName(xTypesFieldRef);
        DataMember xDataMember = (from item in Cosmos.Assembler.Assembler.CurrentInstance.DataMembers
                                  where item.Name == xTheName
                                  select item).FirstOrDefault();
        if (xDataMember != null) {
          Cosmos.Assembler.Assembler.CurrentInstance.DataMembers.Remove((from item in Cosmos.Assembler.Assembler.CurrentInstance.DataMembers
                                                        where item == xDataMember
                                                        select item).First());
        }
        var xData = new byte[16 + (aTypesSet.Count * GetVTableEntrySize())];
        var xTemp = BitConverter.GetBytes(aGetTypeID(typeof(Array)));
        xTemp = BitConverter.GetBytes(0x80000002);
        Array.Copy(xTemp, 0, xData, 4, 4);
        xTemp = BitConverter.GetBytes(aTypesSet.Count);
        Array.Copy(xTemp, 0, xData, 8, 4);
        xTemp = BitConverter.GetBytes(GetVTableEntrySize());
        Array.Copy(xTemp, 0, xData, 12, 4);
        Cosmos.Assembler.Assembler.CurrentInstance.DataMembers.Add(new DataMember(xTheName + "__Contents", xData));
        Cosmos.Assembler.Assembler.CurrentInstance.DataMembers.Add(new DataMember(xTheName, Cosmos.Assembler.ElementReference.New(xTheName + "__Contents")));
#if VMT_DEBUG
        using (var xVmtDebugOutput = XmlWriter.Create(@"e:\vmt_debug.xml"))
        {
            xVmtDebugOutput.WriteStartDocument();
            xVmtDebugOutput.WriteStartElement("VMT");
#endif
        //Push((uint)aTypesSet.Count);
        //Call(xLoadTypeTableRef);
        foreach (var xType in aTypesSet) {
#if VMT_DEBUG
                xVmtDebugOutput.WriteStartElement("Type");
                xVmtDebugOutput.WriteAttributeString("TypeId", aGetTypeID(xType).ToString());
                if (xType.BaseType != null)
                {
                    xVmtDebugOutput.WriteAttributeString("BaseTypeId", aGetTypeID(xType.BaseType).ToString());
                }
                xVmtDebugOutput.WriteAttributeString("Name", xType.FullName);
#endif
          // value contains true if the method is an interface method definition
          SortedList<MethodBase, bool> xEmittedMethods = new SortedList<MethodBase, bool>(new MethodBaseComparer());
          foreach (MethodBase xMethod in xType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
            if (aMethodsSet.Contains(xMethod))//) && !xMethod.IsAbstract)
                        {
              xEmittedMethods.Add(xMethod, false);
            }
          }
          foreach (MethodBase xCtor in xType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
            if (aMethodsSet.Contains(xCtor))// && !xCtor.IsAbstract)
                        {
              xEmittedMethods.Add(xCtor, false);
            }
          }
          foreach (var xIntf in xType.GetInterfaces()) {
            foreach (var xMethodIntf in xIntf.GetMethods()) {
              var xActualMethod = xType.GetMethod(xIntf.FullName + "." + xMethodIntf.Name,
                                                  (from xParam in xMethodIntf.GetParameters()
                                                   select xParam.ParameterType).ToArray());

              if (xActualMethod == null) {
                // get private implemenation
                xActualMethod = xType.GetMethod(xMethodIntf.Name,
                                                (from xParam in xMethodIntf.GetParameters()
                                                 select xParam.ParameterType).ToArray());
              }
              if (xActualMethod == null) {
                try {
                  var xMap = xType.GetInterfaceMap(xIntf);
                  for (int k = 0; k < xMap.InterfaceMethods.Length; k++) {
                    if (xMap.InterfaceMethods[k] == xMethodIntf) {
                      xActualMethod = xMap.TargetMethods[k];
                      break;
                    }
                  }
                } catch {
                }
              }
              if (aMethodsSet.Contains(xMethodIntf)) {
                if (!xEmittedMethods.ContainsKey(xMethodIntf)) {
                  xEmittedMethods.Add(xMethodIntf,
                                      true);
                }
              }

            }
          }
          if (!xType.IsInterface) {
            Push(aGetTypeID(xType));
          }
          int? xBaseIndex = null;
          if (xType.BaseType == null) {
            xBaseIndex = (int)aGetTypeID(xType);
          } else {
            for (int t = 0; t < aTypesSet.Count; t++) {
              // todo: optimize check
              var xItem = aTypesSet.Skip(t).First();
              if (xItem.ToString() == xType.BaseType.ToString()) {
                xBaseIndex = (int)aGetTypeID(xItem);
                break;
              }
            }
          }
          if (xBaseIndex == null) {
            throw new Exception("Base type not found!");
          }
          for (int x = xEmittedMethods.Count - 1; x >= 0; x--) {
            if (!aMethodsSet.Contains(xEmittedMethods.Keys[x])) {
              xEmittedMethods.RemoveAt(x);
            }
          }
          if (!xType.IsInterface) {
            Move("VMT__TYPE_ID_HOLDER__" + DataMember.FilterStringForIncorrectChars(MethodInfoLabelGenerator.GetFullName(xType) + " ASM_IS__" + xType.Assembly.GetName().Name), (int)aGetTypeID(xType));
            Cosmos.Assembler.Assembler.CurrentInstance.DataMembers.Add(
                new DataMember("VMT__TYPE_ID_HOLDER__" + DataMember.FilterStringForIncorrectChars(MethodInfoLabelGenerator.GetFullName(xType) + " ASM_IS__" + xType.Assembly.GetName().Name), new int[] { (int)aGetTypeID(xType) }));
            Push((uint)xBaseIndex.Value);
            xData = new byte[16 + (xEmittedMethods.Count * 4)];
            xTemp = BitConverter.GetBytes(aGetTypeID(typeof(Array)));
            Array.Copy(xTemp, 0, xData, 0, 4);
            xTemp = BitConverter.GetBytes(0x80000002); // embedded array
            Array.Copy(xTemp, 0, xData, 4, 4);
            xTemp = BitConverter.GetBytes(xEmittedMethods.Count); // embedded array
            Array.Copy(xTemp, 0, xData, 8, 4);
            xTemp = BitConverter.GetBytes(4); // embedded array
            Array.Copy(xTemp, 0, xData, 12, 4);
            string xDataName = "____SYSTEM____TYPE___" + DataMember.FilterStringForIncorrectChars(MethodInfoLabelGenerator.GetFullName(xType) + " ASM_IS__" + xType.Assembly.GetName().Name) + "__MethodIndexesArray";
            Cosmos.Assembler.Assembler.CurrentInstance.DataMembers.Add(new DataMember(xDataName, xData));
            Push(xDataName);
            xDataName = "____SYSTEM____TYPE___" + DataMember.FilterStringForIncorrectChars(MethodInfoLabelGenerator.GetFullName(xType) + " ASM_IS__" + xType.Assembly.GetName().Name) + "__MethodAddressesArray";
            Cosmos.Assembler.Assembler.CurrentInstance.DataMembers.Add(new DataMember(xDataName, xData));
            Push(xDataName);
            xData = new byte[16 + Encoding.Unicode.GetByteCount(xType.FullName + ", " + xType.Module.Assembly.GetName().FullName)];
            xTemp = BitConverter.GetBytes(aGetTypeID(typeof(Array)));
            Array.Copy(xTemp, 0, xData, 0, 4);
            xTemp = BitConverter.GetBytes(0x80000002); // embedded array
            Array.Copy(xTemp, 0, xData, 4, 4);
            xTemp = BitConverter.GetBytes((xType.FullName + ", " + xType.Module.Assembly.GetName().FullName).Length);
            Array.Copy(xTemp, 0, xData, 8, 4);
            xTemp = BitConverter.GetBytes(2); // embedded array
            Array.Copy(xTemp, 0, xData, 12, 4);
            xDataName = "____SYSTEM____TYPE___" + DataMember.FilterStringForIncorrectChars(MethodInfoLabelGenerator.GetFullName(xType) + " ASM_IS__" + xType.Assembly.GetName().Name);
            Cosmos.Assembler.Assembler.CurrentInstance.DataMembers.Add(new DataMember(xDataName, xData));
            Push("0" + xEmittedMethods.Count.ToString("X") + "h");
            Call(xSetTypeInfoRef);
          }
          for (int j = 0; j < xEmittedMethods.Count; j++) {
            MethodBase xMethod = xEmittedMethods.Keys[j];
#if VMT_DEBUG
                    xVmtDebugOutput.WriteStartElement("Method");
                    xVmtDebugOutput.WriteAttributeString("Id", aGetMethodUID(xMethod).ToString());
                    xVmtDebugOutput.WriteAttributeString("Name", xMethod.GetFullName());
                    xVmtDebugOutput.WriteEndElement();
#endif
            var xMethodId = aGetMethodUID(xMethod);
            if (!xType.IsInterface) {
              if (xEmittedMethods.Values[j]) {
                var xNewMethod = xType.GetMethod(xMethod.DeclaringType.FullName + "." + xMethod.Name,
                                                    (from xParam in xMethod.GetParameters()
                                                     select xParam.ParameterType).ToArray());

                if (xNewMethod == null) {
                  // get private implemenation
                  xNewMethod = xType.GetMethod(xMethod.Name,
                                                  (from xParam in xMethod.GetParameters()
                                                   select xParam.ParameterType).ToArray());
                }
                if (xNewMethod == null) {
                  try {
                    var xMap = xType.GetInterfaceMap(xMethod.DeclaringType);
                    for (int k = 0; k < xMap.InterfaceMethods.Length; k++) {
                      if (xMap.InterfaceMethods[k] == xMethod) {
                        xNewMethod = xMap.TargetMethods[k];
                        break;
                      }
                    }
                  } catch {
                  }
                }
                xMethod = xNewMethod;
              }

              Push((uint)aGetTypeID(xType));
              Push((uint)j);

              Push((uint)xMethodId);
              if (xMethod.IsAbstract) {
                // abstract methods dont have bodies, oiw, are not emitted
                Push(0);
              } else {
                Push(ILOp.GetMethodLabel(xMethod));
              }
              Push(0);
              Call(VTablesImplRefs.SetMethodInfoRef);
            }
          }
#if VMT_DEBUG
                xVmtDebugOutput.WriteEndElement(); // type
#endif
        }
#if VMT_DEBUG
                    xVmtDebugOutput.WriteEndElement(); // types
                    xVmtDebugOutput.WriteEndDocument();
        }
#endif
      }
      MethodEnd(InitVMTCodeLabel);
    }

    public void ProcessField(FieldInfo aField) {
      string xFieldName = MethodInfoLabelGenerator.GetFullName(aField);
      xFieldName = DataMember.GetStaticFieldName(aField);
      if (Cosmos.Assembler.Assembler.CurrentInstance.DataMembers.Count(x => x.Name == xFieldName) == 0) {
        var xItemList = (from item in aField.GetCustomAttributes(false)
                         where item.GetType().FullName == "ManifestResourceStreamAttribute"
                         select item).ToList();

        object xItem = null;
        if (xItemList.Count > 0)
          xItem = xItemList[0];
        string xManifestResourceName = null;
        if (xItem != null) {
          var xItemType = xItem.GetType();
          xManifestResourceName = (string)xItemType.GetField("ResourceName").GetValue(xItem);
        }
        if (xManifestResourceName != null) {
          // todo: add support for manifest streams again
          //RegisterType(xCurrentField.FieldType);
          //string xFileName = Path.Combine(mOutputDir,
          //                                (xCurrentField.DeclaringType.Assembly.FullName + "__" + xManifestResourceName).Replace(",",
          //                                                                                                                       "_") + ".res");
          //using (var xStream = xCurrentField.DeclaringType.Assembly.GetManifestResourceStream(xManifestResourceName)) {
          //    if (xStream == null) {
          //        throw new Exception("Resource '" + xManifestResourceName + "' not found!");
          //    }
          //    using (var xTarget = File.Create(xFileName)) {
          //        // todo: abstract this array code out.
          //        xTarget.Write(BitConverter.GetBytes(Engine.RegisterType(Engine.GetType("mscorlib",
          //                                                                               "System.Array"))),
          //                      0,
          //                      4);
          //        xTarget.Write(BitConverter.GetBytes((uint)InstanceTypeEnum.StaticEmbeddedArray),
          //                      0,
          //                      4);
          //        xTarget.Write(BitConverter.GetBytes((int)xStream.Length), 0, 4);
          //        xTarget.Write(BitConverter.GetBytes((int)1), 0, 4);
          //        var xBuff = new byte[128];
          //        while (xStream.Position < xStream.Length) {
          //            int xBytesRead = xStream.Read(xBuff, 0, 128);
          //            xTarget.Write(xBuff, 0, xBytesRead);
          //        }
          //    }
          //}
          //mAssembler.DataMembers.Add(new DataMember("___" + xFieldName + "___Contents",
          //                                          "incbin",
          //                                          "\"" + xFileName + "\""));
          //mAssembler.DataMembers.Add(new DataMember(xFieldName,
          //                                          "dd",
          //                                          "___" + xFieldName + "___Contents"));
          throw new NotImplementedException();
        } else {
          uint xTheSize;
          //string theType = "db";
          Type xFieldTypeDef = aField.FieldType;
          if (!xFieldTypeDef.IsClass || xFieldTypeDef.IsValueType) {
            xTheSize = GetSizeOfType(aField.FieldType);
          } else {
            xTheSize = 4;
          }
          byte[] xData = new byte[xTheSize];
          try {
            object xValue = aField.GetValue(null);
            if (xValue != null) {
              try {
                xData = new byte[xTheSize];
                if (xValue.GetType().IsValueType) {
                  for (int x = 0; x < xTheSize; x++) {
                    xData[x] = Marshal.ReadByte(xValue,
                                                x);
                  }
                }
              } catch {
              }
            }
          } catch {
          }
          Cosmos.Assembler.Assembler.CurrentInstance.DataMembers.Add(new DataMember(xFieldName, xData));
        }
      }
    }

    public abstract uint GetSizeOfType(Type aType);

    public abstract void EmitEntrypoint(MethodBase aEntrypoint, IEnumerable<MethodBase> aMethods);

    protected abstract void Ldarg(MethodInfo aMethod, int aIndex);
    protected abstract void Ldflda(MethodInfo aMethod, string aFieldId);
    protected abstract void Call(MethodInfo aMethod, MethodInfo aTargetMethod);
    protected abstract void Pop();

    internal void GenerateMethodForward(MethodInfo aFrom, MethodInfo aTo) {
      // todo: completely get rid of this kind of trampoline code
      MethodBegin(aFrom);
      {
        var xParams = aTo.MethodBase.GetParameters().ToArray();

        if (aTo.MethodAssembler != null) {
          xParams = aFrom.MethodBase.GetParameters();
        }

        //if (aFrom.MethodBase.GetParameters().Length > 0 || !aFrom.MethodBase.IsStatic) {
        //  Ldarg(aFrom, 0);
        //  Pop();
        //}

        int xCurParamIdx = 0;
        if (!aFrom.MethodBase.IsStatic) {
          Ldarg(aFrom, 0);
          xCurParamIdx++;
          if (aTo.MethodAssembler == null) {
            xParams = xParams.Skip(1).ToArray();
          }
        }
        foreach (var xParam in xParams) {
          FieldAccessAttribute xFieldAccessAttrib = null;
          foreach (var xAttrib in xParam.GetCustomAttributes(typeof(FieldAccessAttribute), true)) {
            xFieldAccessAttrib = xAttrib as FieldAccessAttribute;
          }

          if (xFieldAccessAttrib != null) {
            // field access                                                                                        
            new Comment("Loading address of field '" + xFieldAccessAttrib.Name + "'");
            Ldarg(aFrom, 0);
            Ldflda(aFrom, xFieldAccessAttrib.Name);
          } else {
            // normal field access
            new Comment("Loading parameter " + xCurParamIdx);
            Ldarg(aFrom, xCurParamIdx);
            xCurParamIdx++;
          }
        }
        Call(aFrom, aTo);
      }
      MethodEnd(aFrom);
    }

  }
}
