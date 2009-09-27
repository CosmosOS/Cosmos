using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.IO;
using System.Reflection;
using Indy.IL2CPU;
using Indy.IL2CPU.IL;
using System.Runtime.InteropServices;
using Indy.IL2CPU.Plugs;

namespace Cosmos.IL2CPU {

  public abstract class Assembler {
    protected ILOp[] mILOpsLo = new ILOp[256];
    protected ILOp[] mILOpsHi = new ILOp[256];
    public virtual void Initialize() {
    }
    // Contains info on the current stack structure. What type are on the stack, etc
    public readonly StackContents Stack = new StackContents();

    private static Assembler mCurrentInstance;
    protected internal List<Instruction> mInstructions = new List<Instruction>();
    private List<DataMember> mDataMembers = new List<DataMember>();
    private System.IO.TextWriter mLog;
    #region Properties
    public List<DataMember> DataMembers {
      get {
        return mDataMembers;
      }
    }

    public List<Instruction> Instructions {
      get {
        return mInstructions;
      }
    }
    public static Assembler CurrentInstance {
      get {
        return mCurrentInstance;
      }
    }

    internal int AllAssemblerElementCount {
      get {
        return mInstructions.Count + mDataMembers.Count;
      }
    }

    #endregion

    public Assembler() {
      mLog = new System.IO.StreamWriter("Cosmos.Assembler.Log");
      InitILOps();
      mCurrentInstance = this;
    }

    public static ulong ConstructLabel(uint aMethod, uint aOpCode, byte aSubLabel) {
      /* Explanation:
       * * This method generates labels. labels are 64bit:
       * * First 24 bits (high to low) is the method number
       * * then 32 bits is the opcode offset in the il
       * * then 8 bits for a sub label.
       */
      if (aMethod > 0x00FFFFFF) {
        throw new Exception("Error Method id too high!");
      }
      ulong xResult = aMethod << 40;
      xResult |= aOpCode << 8;
      xResult |= aSubLabel;
      return xResult;
    }

    public void Dispose() {
      // MtW: I know, IDisposable usage for this isn't really nice, but for now this should be fine.
      //		Anyhow, we need a way to clear the CurrentInstance property
      //mInstructions.Clear();
      //mDataMembers.Clear();
      //if (mAllAssemblerElements != null)
      //{
      //    mAllAssemblerElements.Clear();
      //}
    }

    public BaseAssemblerElement GetAssemblerElement(int aIndex) {
      if (aIndex >= mInstructions.Count) {
        return mDataMembers[aIndex - mInstructions.Count];
      }
      return mInstructions[aIndex];
    }

    public BaseAssemblerElement TryResolveReference(ElementReference aReference) {
      foreach (var xInstruction in mInstructions) {
        var xLabel = xInstruction as Label;
        if (xLabel != null) {
          if (xLabel.QualifiedName.Equals(aReference.Name, StringComparison.InvariantCultureIgnoreCase)) {
            return xLabel;
          }
        }
      }
      foreach (var xDataMember in mDataMembers) {
        if (xDataMember.Name.Equals(aReference.Name, StringComparison.InvariantCultureIgnoreCase)) {
          return xDataMember;
        }
      }
      return null;
    }

    public void Add(params Instruction[] aReaders) {
      foreach (Instruction xInstruction in aReaders) {
        mInstructions.Add(xInstruction);
      }
    }

    protected virtual void MethodBegin(MethodInfo aMethod) {
      new Comment(this, "---------------------------------------------------------");
      new Comment(this, "Type: " + aMethod.MethodBase.DeclaringType.ToString());
      new Comment(this, "Name: " + aMethod.MethodBase.Name);
      new Comment(this, "Plugged: " + (aMethod.PlugMethod == null ? "No" : "Yes"));
    }

    protected virtual void MethodEnd(MethodInfo aMethod) {
      new Comment(this, "End Method: " + aMethod.MethodBase.Name);
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
        throw new Exception("For now, too much methods");
      }

      MethodBegin(aMethod);
      Stack.Clear();
      mLog.WriteLine("Method '{0}'", aMethod.MethodBase.GetFullName());
      mLog.Flush();
      if (aMethod.MethodBase.DeclaringType.FullName == "Cosmos.Kernel.Plugs.CPU"
  && aMethod.MethodBase.Name == "GetAmountOfRAM") {
        Console.Write("");
      }
      if (aMethod.MethodAssembler != null) {
        mLog.WriteLine("Emitted using MethodAssembler", aMethod.MethodBase.GetFullName());
        mLog.Flush();
        var xAssembler = (AssemblerMethod)Activator.CreateInstance(aMethod.MethodAssembler);
        var xNeedsMethodInfo = xAssembler as INeedsMethodInfo;
        if (xNeedsMethodInfo != null) {
          throw new Exception("Plug cant work, because of INeedsMethodInfo");
        }
        xAssembler.AssembleNew(this, aMethod);
      } else {
        foreach (var xOpCode in aOpCodes) {
          uint xOpCodeVal = (uint)xOpCode.OpCode;
          ILOp xILOp;
          if (xOpCodeVal <= 0xFF) {
            xILOp = mILOpsLo[xOpCodeVal];
          } else {
            xILOp = mILOpsHi[xOpCodeVal & 0xFF];
          }
          //mLog.WriteLine ( "\t[" + xILOp.ToString() + "] \t Stack start: " + Stack.Count.ToString() );
          mLog.WriteLine("\t{0} {1}", Stack.Count, xILOp.GetType().Name);
          mLog.Flush();
          BeforeOp(aMethod, xOpCode);
          new Comment(this, "[" + xILOp.ToString() + "]");
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
            Push(0);
            Stack.Push(4, typeof(Exception));
          }
          xILOp.Execute(aMethod, xOpCode);
          AfterOp(aMethod, xOpCode);
          //mLog.WriteLine( " end: " + Stack.Count.ToString() );
        }
      }
      MethodEnd(aMethod);
    }

    //

    protected virtual void BeforeOp(MethodInfo aMethod, ILOpCode aOpCode) {
    }

    protected virtual void AfterOp(MethodInfo aMethod, ILOpCode aOpCode) {
    }

    /// <summary>
    /// allows to emit footers to the code and datamember sections
    /// </summary>
    protected virtual void OnBeforeFlush() {
    }
    private uint mDataMemberCounter = 0;
    public string GetIdentifier(string aPrefix) {
      mDataMemberCounter++;
      return aPrefix + mDataMemberCounter.ToString("X8").ToUpper();
    }
    private bool mFlushInitializationDone = false;
    protected void BeforeFlush() {
      if (mFlushInitializationDone) {
        return;
      }
      mFlushInitializationDone = true;
      OnBeforeFlush();
      //MergeAllElements();
    }

    public virtual void FlushBinary(Stream aOutput, ulong aBaseAddress) {
      BeforeFlush();
      var xMax = AllAssemblerElementCount;
      var xCurrentAddresss = aBaseAddress;
      for (int i = 0; i < xMax; i++) {
        GetAssemblerElement(i).UpdateAddress(this, ref xCurrentAddresss);
      }
      aOutput.SetLength(aOutput.Length + (long)(xCurrentAddresss - aBaseAddress));
      for (int i = 0; i < xMax; i++) {
        var xItem = GetAssemblerElement(i);
        if (!xItem.IsComplete(this)) {
          throw new Exception("Incomplete element encountered.");
        }
        //var xBuff = xItem.GetData(this);
        //aOutput.Write(xBuff, 0, xBuff.Length);
        xItem.WriteData(this, aOutput);
      }
    }

    public virtual void FlushText(TextWriter aOutput) {
      BeforeFlush();
      if (mDataMembers.Count > 0) {
        aOutput.WriteLine();
        foreach (DataMember xMember in mDataMembers) {
          aOutput.Write("\t");
          if (xMember.IsComment) {
            aOutput.Write(xMember.Name);
          } else {
            xMember.WriteText(this, aOutput);
          }
          aOutput.WriteLine();
        }
        aOutput.WriteLine();
      }
      if (mInstructions.Count > 0) {
        for (int i = 0; i < mInstructions.Count; i++) {
          //foreach (Instruction x in mInstructions) {
          var x = mInstructions[i];
          string prefix = "\t\t\t";
          Label xLabel = x as Label;
          if (xLabel != null) {
            aOutput.WriteLine();
            if (xLabel.Name[0] == '.') {
              prefix = "\t\t";
            } else {
              prefix = "\t";
            }
            //string xFullName;
            aOutput.Write(prefix);
            x.WriteText(this, aOutput);
            aOutput.WriteLine();
            //aOutput.WriteLine(prefix + Label.FilterStringForIncorrectChars(xFullName) + ":");
            continue;
          }
          aOutput.Write(prefix);
          x.WriteText(this, aOutput);
          aOutput.WriteLine();
        }
      }
    }

    protected abstract void InitILOps();

    protected virtual void InitILOps(Type aAssemblerBaseOp) {
      foreach (var xType in aAssemblerBaseOp.Assembly.GetExportedTypes()) {
        if (xType.IsSubclassOf(aAssemblerBaseOp)) {
          var xAttribs = (OpCodeAttribute[])xType.GetCustomAttributes(typeof(OpCodeAttribute), false);
          foreach (var xAttrib in xAttribs) {
            var xOpCode = (ushort)xAttrib.OpCode;
            var xCtor = xType.GetConstructor(new Type[] { typeof(Assembler) });
            var xILOp = (ILOp)xCtor.Invoke(new Object[] { this });
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
    protected abstract void Push(string aLabelName);
    protected abstract void Call(MethodBase aMethod);
    protected abstract void Move(string aDestLabelName, int aValue);
    protected abstract void Jump(string aLabelName);
    protected abstract int GetVTableEntrySize();

    public void GenerateVMTCode(HashSet<Type> aTypesSet, HashSet<MethodBase> aMethodsSet, Func<Type, uint> aGetTypeID, Func<MethodBase, uint> aGetMethodUID) {
      // initialization
      var xSetTypeInfoRef = VTablesImplRefs.SetTypeInfoRef;
      var xSetMethodInfoRef = VTablesImplRefs.SetMethodInfoRef;
      var xLoadTypeTableRef = VTablesImplRefs.LoadTypeTableRef;
      var xTypesFieldRef = VTablesImplRefs.VTablesImplDef.GetField("mTypes",
                                                                         BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
      // data we need but dont have:
      
      // end initialization
      string xTheName = DataMember.GetStaticFieldName(xTypesFieldRef);
      DataMember xDataMember = (from item in Assembler.mCurrentInstance.DataMembers
                                where item.Name == xTheName
                                select item).FirstOrDefault();
      if (xDataMember != null) {
        Assembler.mCurrentInstance.DataMembers.Remove((from item in Assembler.mCurrentInstance.DataMembers
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
      Assembler.mCurrentInstance.DataMembers.Add(new DataMember(xTheName + "__Contents", xData));
      Assembler.mCurrentInstance.DataMembers.Add(new DataMember(xTheName, ElementReference.New(xTheName + "__Contents")));
      Push((uint)aTypesSet.Count);
      Call(xLoadTypeTableRef);
      foreach(var xType in aTypesSet){
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
          Assembler.mCurrentInstance.DataMembers.Add(
              new DataMember("VMT__TYPE_ID_HOLDER__" + DataMember.FilterStringForIncorrectChars(MethodInfoLabelGenerator.GetFullName(xType) + " ASM_IS__" + xType.Assembly.GetName().Name), new int[] { (int)aGetTypeID(xType) }));
          Push((uint)xBaseIndex.Value);
          Push("0" + xEmittedMethods.Count.ToString("X") + "h");
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
          Assembler.mCurrentInstance.DataMembers.Add(new DataMember(xDataName, xData));
          Push(xDataName);
          xDataName = "____SYSTEM____TYPE___" + DataMember.FilterStringForIncorrectChars(MethodInfoLabelGenerator.GetFullName(xType) + " ASM_IS__" + xType.Assembly.GetName().Name) + "__MethodAddressesArray";
          Assembler.mCurrentInstance.DataMembers.Add(new DataMember(xDataName, xData));
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
          Assembler.CurrentInstance.DataMembers.Add(new DataMember(xDataName, xData));
          Push((uint)xEmittedMethods.Count);
          //Push("0");
          Call(xSetTypeInfoRef);
        }
        for (int j = 0; j < xEmittedMethods.Count; j++) {
          MethodBase xMethod = xEmittedMethods.Keys[j];
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
            //Move(GetService<IMetaDataInfoService>().GetMethodIdLabel(xMethod), xMethodId);
            //Assembler.DataMembers.Add(
            //    new DataMember(GetService<IMetaDataInfoService>().GetMethodIdLabel(xMethod),
            //                   new int[] { xMethodId }));

            Push((uint)aGetTypeID(xType));
            Push((uint)j);

            Push((uint)xMethodId);
            if (xMethod.IsAbstract) {
              // abstract methods dont have bodies, oiw, are not emitted
              Push(0);
            } else {
              //var xTest = GetService<IMetaDataInfoService>().GetMethodInfo(xMethod, false);
              //Push(xTest.LabelName);
            }
            //xDataValue = Encoding.ASCII.GetBytes(GetFullName(xMethod)).Aggregate("", (b, x) => b + x + ",") + "0";
            //xDataName = "____SYSTEM____METHOD___" + DataMember.FilterStringForIncorrectChars(GetFullName(xMethod));
            //mAssembler.DataMembers.Add(new DataMember(xDataName, "db", xDataValue));
            //Push(xDataName);
            Push(0);
            //Call(SetMethodInfoRef);
          }
        }
      }
    }

    public void ProcessField(FieldInfo aField) {
      string xFieldName = MethodInfoLabelGenerator.GetFullName(aField);
      xFieldName = DataMember.GetStaticFieldName(aField);
      if (DataMembers.Count(x => x.Name == xFieldName) == 0) {
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
            xTheSize = ILOp.SizeOfType(aField.FieldType);
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
          DataMembers.Add(new DataMember(xFieldName, xData));
        }
      }      
    }

    public abstract void EmitEntrypoint(MethodBase aEntrypoint, IEnumerable<MethodBase> aMethods);

    protected abstract void Ldarg(MethodInfo aMethod, int aIndex);
    protected abstract void Ldflda(MethodInfo aMethod, string aFieldId);
    protected abstract void Call(MethodInfo aMethod, MethodInfo aTargetMethod);

    internal void GenerateMethodForward(MethodInfo aFrom, MethodInfo aTo) {
// todo: completely get rid of this kind of trampoline code
      MethodBegin(aFrom);
      {
        var xParams = aTo.MethodBase.GetParameters().AsQueryable();
        
        int xCurParamIdx = 0;
        if (!aFrom.MethodBase.IsStatic) {
          Ldarg(aFrom, 0);
          xParams = xParams.Skip(1);
          xCurParamIdx++;
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