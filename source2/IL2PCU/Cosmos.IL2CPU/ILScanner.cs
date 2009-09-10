using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Indy.IL2CPU;
using Indy.IL2CPU.Plugs;

namespace Cosmos.IL2CPU {
  public class ILScanner : IDisposable {
    // Here are old comments - we moved to a Dictionary + List, which is much better esp
    // now that we need lookups to the indexes
    // List is needed for processing.
    //
    //Note: We have both HashSet and List because HashSet.Contains is much faster
    // than List.Contains. Also in the future we may remove items from the List
    // which have already been processed yet need to keep them in HashSet.
    //TODO: When we go threaded, these two should be encapselated into a single
    // class with thread safety.
    //TODO: These store the MethodBase which also have the IL for the body in memory
    // For large asms this could eat lot of RAM. Should convert this to remove
    // items from the list after they are processed but keep them in HashSet so we
    // know they are already done. Currently HashSet uses a reference though, so we
    // need to hash on some UID instead of the refernce. Do not use strings, they are
    // super slow.
    //	TODO: We need to scan for static fields too. 
    protected Dictionary<MethodBase, uint> mKnownMethods = new Dictionary<MethodBase, uint>();
    // We need a separate list because we cannot iterate keys by index, and any functions
    // to get a list of keys will do a on demand copy, which won't meet our needs either
    // becuase we have to walk the list dynamically as it grows, which is also why we need to
    // index it rather than enumerate it with foreach.
    // We also need a separate list becuase Execute is called multiple
    // times to process plugs and so known methods accumulates,
    // but we dont want to reproces old methods from previous Execute calls.
    protected List<MethodInfo> mMethodsToProcess = new List<MethodInfo>();
    // ExecuteInternal is called multiple times, we don't want to rescan
    // ones that are "finished" so we update this "pointer"
    protected int mMethodsToProcessStart;
    // List of plug implementations.
    // Key: MethodBase of targetted method
    // Value: index into mMethodsToProcess
    protected Dictionary<MethodBase, uint> mMethodPlugs = new Dictionary<MethodBase, uint>();

    //TODO: Likely change this to be like Methods to be more efficient. Might only need Dictionary
    protected HashSet<Type> mTypesSet = new HashSet<Type>();
    protected List<Type> mTypes = new List<Type>();

    // Logging
    // Only use for debugging and profiling.
    protected bool mLogEnabled = false;
    protected string mMapPathname;
    protected TextWriter mLogWriter;
    protected struct LogItem {
      public string SrcType;
      public object Item;
    }
    protected Dictionary<object, List<LogItem>> mLogMap;

    public void EnableLogging(string aPathname) {
      mLogMap = new Dictionary<object, List<LogItem>>();
      mMapPathname = aPathname;
      mLogEnabled = true;
    }

    protected string LogItemText(object aItem) {
      if (aItem is MethodBase) {
        var x = (MethodBase)aItem;
        return "Method: " + x.GetFullName();
      } else if (aItem is Type) {
        var x = (Type)aItem;
        return "Type: " + x.FullName;
      } else {
        return "Other: " + aItem.ToString();
      }
    }

    public void Dispose() {
      if (mLogEnabled) {
        // Create bookmarks, but also a dictionary that
        // we can find the items in
        var xBookmarks = new Dictionary<object, int>();
        int xBookmark = 0;
        foreach (var xList in mLogMap) {
          xBookmarks.Add(xList.Key, xBookmark);
          xBookmark++;
        }

        //TODO: Change to output HTML with src each item hyper linked to where
        // it is listed under another source
        using (mLogWriter = new StreamWriter(mMapPathname, false)) {
          mLogWriter.WriteLine("<html><body>");
          foreach (var xList in mLogMap) {
            mLogWriter.WriteLine("<p></p>");
            mLogWriter.WriteLine("<p><a href=\"#Item" + xBookmarks[xList.Key].ToString() + "\">");
            if (xList.Key == null) {
              mLogWriter.WriteLine("Unspecified Source");
            } else {
              mLogWriter.WriteLine(LogItemText(xList.Key));
            }
            mLogWriter.WriteLine("</a></p>");

            mLogWriter.WriteLine("<ul>");
            foreach (var xItem in xList.Value) {
              int xHref;
              if (!xBookmarks.TryGetValue(xItem.Item, out xHref)) {
                xHref = -1;
              }
              mLogWriter.Write("<li>");
              if (xHref >= 0) {
                mLogWriter.Write("<a name=\"Item" + xHref.ToString() + "\">");
              }
              mLogWriter.Write(LogItemText(xItem.Item));
              if (xHref >= 0) {
                mLogWriter.Write("</a>");
              }
              mLogWriter.WriteLine("</li>");

              mLogWriter.WriteLine("<ul>");
              mLogWriter.WriteLine("<li>" + xItem.SrcType + "</<li>");
              mLogWriter.WriteLine("</ul>");
            }
            mLogWriter.WriteLine("</ul>");
          }
          mLogWriter.WriteLine("</body></html>");
        }
      }
    }

    protected ILReader mReader;
    protected Assembler mAsmblr;

    public ILScanner(Assembler aAsmblr) {
      mAsmblr = aAsmblr;
      mReader = new ILReader();
      mThrowHelper = typeof(object).Assembly.GetType("System.ThrowHelper");
    }

    public void Execute(System.Reflection.MethodInfo aStartMethod) {
      // TODO: New plug system, common plug base which all descend from
      // It can have a "this" member and then we
      // can separate static from instance by the static keyword
      // and ctors can be static "ctor" by name
      // Will still need plug attrib though to specify target
      // Also need to handle asm plugs, but those will be different anyways
      //
      // Scan plugs first, so when we scan from 
      // entry point plugs will be found.
      foreach (var xAsm in AppDomain.CurrentDomain.GetAssemblies()) {
        foreach (var xType in xAsm.GetTypes()) {
          foreach (var xAttrib1 in xType.GetCustomAttributes(false)) {
            // Find all classes marked as a Plug
            if (xAttrib1 is PlugAttribute) {
              var xTypeAttrib = (PlugAttribute)xAttrib1;

              var xTargetType = xTypeAttrib.Target;
              if (xTargetType == null) {
                xTargetType = Type.GetType(xTypeAttrib.TargetName, true);
              }

              // See if there is a custom PlugMethod attribute
              // Plug implementations must be static and public, so 
              // we narrow the search to meet these requirements
              foreach (var xMethod in xType.GetMethods(BindingFlags.Static | BindingFlags.Public)) {
                PlugMethodAttribute xMethodAttrib = null;
                foreach (var xAttrib2 in xMethod.GetCustomAttributes(false)) {
                  if (xAttrib2 is PlugMethodAttribute) {
                    xMethodAttrib = (PlugMethodAttribute)xAttrib2;
                  }
                }

                // See if we need to disable this plug
                bool xEnabled = true;
                if (xMethodAttrib != null) {
                  //TODO: Check this against build options
                  //TODO: Two exclusive IsOnly's dont make sense
                  // refactor these as a positive rather than negative
                  if (xMethodAttrib.IsMonoOnly) {
                    xEnabled = false;
                  } else if (xMethodAttrib.Signature != null) {
                    // System_Void__Indy_IL2CPU_Assembler_Assembler__cctor__
                    // If signature exists, the search is slow. Signatures
                    // are infrequent though, so for now we just go slow method
                    // and have not optimized or cached this info. When we
                    // redo the plugs, we can fix this.
                    //
                    // This merges methods and ctors, improve this later
                    var xTargetMethods = xTargetType.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Cast<MethodBase>().AsQueryable();
                    xTargetMethods = xTargetMethods.Union(xTargetType.GetConstructors(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic));
                    foreach (var xTargetMethod in xTargetMethods) {
                      string sName = DataMember.FilterStringForIncorrectChars(MethodInfoLabelGenerator.GenerateFullName(xTargetMethod));
                      if (string.Compare(sName, xMethodAttrib.Signature, true) == 0) {
                        uint xUID = ExecuteInternal(xType, "Plug", xMethod, true);
                        mMethodPlugs.Add(xTargetMethod, xUID);
                        // Mark as disabled, because we already handled it
                        xEnabled = false;
                        break;
                      }
                    }
                    // if still enabled, we didn't find our method
                    if (xEnabled) {
                      // todo: more precise error: imagine having a 100K line project, and this error happens...
                      throw new Exception("Plug target method not found.");
                    }
                  } else {
                    xEnabled = xMethodAttrib.Enabled;
                  }
                }

                if (xEnabled) {
                  // for PlugMethodAttribute:
                  //TODO: public string Signature;
                  //[PlugMethod(Signature = "System_Void__Indy_IL2CPU_Assembler_Assembler__cctor__")]
                  //TODO: public Type Assembler = null;
                  // Scan the plug implementation
                  uint xUID = ExecuteInternal(xType, "Plug", xMethod, true);

                  // Add the method to the list of plugged methods
                  var xParams = xMethod.GetParameters();
                  //TODO: Static method plugs dont seem to be separated 
                  // from instance ones, so the only way seems to be to try
                  // to match instance first, and if no match try static.
                  // I really don't like this and feel we need to find
                  // an explicit way to determine or mark the method 
                  // implementations.
                  //
                  // Plug implementations take this as first argument
                  // so when matching we don't include it in the search
                  Type[] xTypesInst = null;
                  Type[] xTypesStatic = new Type[xParams.Length];
                  // If 0 params, has to be a static plug so we skip
                  // any copying and leave xTypesInst = null
                  // If 1 params, xTypesInst must be converted to Type[0]
                  if (xParams.Length == 1) {
                    xTypesInst = new Type[0];
                    xTypesStatic[0] = xParams[0].ParameterType;
                  } else if (xParams.Length > 1) {
                    xTypesInst = new Type[xParams.Length - 1];
                    for (int i = 0; i <= xTypesInst.Length - 1; i++) {
                      xTypesInst[i] = xParams[i + 1].ParameterType;
                    }
                    for (int i = 0; i <= xTypesStatic.Length - 1; i++) {
                      xTypesStatic[i] = xParams[i].ParameterType;
                    }
                  }
                  System.Reflection.MethodBase xTargetMethod = null;
                  // TODO: In future make rule that all ctor plugs are called
                  // ctor by name, or use a new attrib
                  //TODO: Document all the plug stuff in a document on website
                  //TODO: To make inclusion of plugs easy, we can make a plugs master
                  // that references the other default plugs so user exes only 
                  // need to reference that one.
                  // TODO: Skip FieldAccessAttribute if in impl
                  if (xTypesInst != null) {
                    if (string.Compare(xMethod.Name, "ctor", true) == 0) {
                      xTargetMethod = xTargetType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, CallingConventions.Any, xTypesInst, null);
                    } else {
                      xTargetMethod = xTargetType.GetMethod(xMethod.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, CallingConventions.Any, xTypesInst, null);
                    }
                  }
                  // Not an instance method, try static
                  if (xTargetMethod == null) {
                    if (string.Compare(xMethod.Name, "ctor", true) == 0) {
                      xTargetMethod = xTargetType.GetConstructor(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, CallingConventions.Any, xTypesStatic, null);
                    } else {
                      xTargetMethod = xTargetType.GetMethod(xMethod.Name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, CallingConventions.Any, xTypesStatic, null);
                    }
                  }
                  if (xTargetMethod == null) {
                    throw new Exception("Plug target method not found.");
                  }
                  if (mMethodPlugs.ContainsKey(xTargetMethod)) {
                    var xTheMethod = mMethodsToProcess[(int)mMethodPlugs[xTargetMethod]];
                    Console.Write("");
                  }
                  mMethodPlugs.Add(xTargetMethod, xUID);
                }
              }
            }
          }
          //TODO: Look for Field plugs
        }
      }

      // Pull in extra implementations, GC etc.
      ExecuteInternal(null, "Explicit Entry", (System.Reflection.MethodInfo)RuntimeEngineRefs.InitializeApplicationRef, true);
      ExecuteInternal(null, "Explicit Entry", (System.Reflection.MethodInfo)RuntimeEngineRefs.FinalizeApplicationRef, true);
      ////xScanner.QueueMethod(typeof(CosmosAssembler).GetMethod("PrintException"), true);
      ExecuteInternal(null, "Explicit Entry", (System.Reflection.MethodInfo)VTablesImplRefs.LoadTypeTableRef, true);
      ExecuteInternal(null, "Explicit Entry", (System.Reflection.MethodInfo)VTablesImplRefs.SetMethodInfoRef, true);
      ExecuteInternal(null, "Explicit Entry", (System.Reflection.MethodInfo)VTablesImplRefs.IsInstanceRef, true);
      ExecuteInternal(null, "Explicit Entry", (System.Reflection.MethodInfo)VTablesImplRefs.SetTypeInfoRef, true);
      ExecuteInternal(null, "Explicit Entry", (System.Reflection.MethodInfo)VTablesImplRefs.GetMethodAddressForTypeRef, true);
      ExecuteInternal(null, "Explicit Entry", (System.Reflection.MethodInfo)GCImplementationRefs.IncRefCountRef, true);
      ExecuteInternal(null, "Explicit Entry", (System.Reflection.MethodInfo)GCImplementationRefs.DecRefCountRef, true);
      ExecuteInternal(null, "Explicit Entry", (System.Reflection.MethodInfo)GCImplementationRefs.AllocNewObjectRef, true);
      //xScanner.Execute( ( System.Reflection.MethodInfo )RuntimeEngineRefs.InitializeApplicationRef );
      //xScanner.Execute( ( System.Reflection.MethodInfo )RuntimeEngineRefs.FinalizeApplicationRef );
      ////xScanner.QueueMethod(typeof(CosmosAssembler).GetMethod("PrintException"));
      //xScanner.Execute( ( System.Reflection.MethodInfo )VTablesImplRefs.LoadTypeTableRef );
      //xScanner.Execute( ( System.Reflection.MethodInfo )VTablesImplRefs.SetMethodInfoRef );
      //xScanner.Execute( ( System.Reflection.MethodInfo )VTablesImplRefs.IsInstanceRef );
      //xScanner.Execute( ( System.Reflection.MethodInfo )VTablesImplRefs.SetTypeInfoRef );

      // Scan from entry point of this program
      ExecuteInternal(null, "Entry Point", aStartMethod, false);
      mAsmblr.GenerateVMTCode(mTypes, (from item in mMethodsToProcess
                                       select item.MethodBase).ToArray());
    }

    public uint ExecuteInternal(object aSrc, string aSrcType, System.Reflection.MethodInfo aStartMethod, bool aIsPlug) {
      // See comment at mMethodsToProcessStart declaration
      mMethodsToProcessStart = mMethodsToProcess.Count;
      uint xResult = QueueMethod(aSrc, aSrcType, aStartMethod, aIsPlug);

      // Cannot use foreach, the list changes as we go
      // and we dont start at 0
      for (int i = mMethodsToProcessStart; i < mMethodsToProcess.Count; i++) {
        var xMethod = mMethodsToProcess[i];
        if (xMethod.Type != MethodInfo.TypeEnum.NeedsPlug) {
          ScanMethod(xMethod);
        }
      }

      // ie 
      //   var xSB = new StringBuilder("test");
      //   object x = xSB;
      //   string y = xSB.ToString();
      //
      // Now that we did a full normal scan, rescan and find all virtuals
      // and for each virtual scan all included types and include descendant overrides.
      // I think we need to scan for ancestor calls too...
      // This process will add more classes etc.. so the process will need to be repeated
      // until no more new methods are found.
      //
      //TODO: Speed this up somehow....
      int xMethodCount;
      do {
        xMethodCount = mMethodsToProcess.Count;
        // Cannot use foreach, the list changes as we go
        for (int i = mMethodsToProcessStart; i < mMethodsToProcess.Count; i++) {
          var xMethodBase = mMethodsToProcess[i].MethodBase;
          if (xMethodBase.IsVirtual) {
            foreach (var xType in mTypes) {
              // Find ancestors and descendants
              if (xType.IsSubclassOf(xMethodBase.DeclaringType) || xMethodBase.DeclaringType.IsSubclassOf(xType)) {
                var xParams = xMethodBase.GetParameters();
                var xParamTypes = new Type[xParams.Length];
                for (int j = 0; j < xParams.Length; j++) {
                  xParamTypes[j] = xParams[j].ParameterType;
                }
                var xNewMethod = xType.GetMethod(xMethodBase.Name, xParamTypes);
                if (xNewMethod != null) {
                  if (!xNewMethod.IsAbstract) {
                    // abstract methods dont have an implementation
                    QueueMethod(xMethodBase.DeclaringType, "SubClass", xNewMethod, false);
                  }
                }
              }
            }
          }
        }
      } while (xMethodCount != mMethodsToProcess.Count);
      return xResult;
    }

    private void ScanMethod(MethodInfo aMethodInfo) {
      var xMethodBase = aMethodInfo.MethodBase;

      // Call ProcessMethod first, later in a threaded environment it will
      // allow more threads to work slightly sooner
      var xOpCodes = mReader.ProcessMethod(xMethodBase);
      if (xOpCodes != null) {
        foreach (var xOpCode in xOpCodes) {
          //InstructionCount++;
          if (xOpCode is ILOpCodes.OpMethod) {
            ((ILOpCodes.OpMethod)xOpCode).ValueUID = QueueMethod(aMethodInfo.MethodBase, "Call", ((ILOpCodes.OpMethod)xOpCode).Value, false);
          } else if (xOpCode is ILOpCodes.OpType) {
            QueueType(aMethodInfo.MethodBase, "OpCode Value", ((ILOpCodes.OpType)xOpCode).Value);
          }
        }

        // Assemble the method
        if (aMethodInfo.MethodBase.DeclaringType != mThrowHelper) {
          mAsmblr.ProcessMethod(aMethodInfo, xOpCodes);
        }
      }
    }

    // System.ThrowHelper exists in MS .NET twice... 
    // Its an internal class that exists in both mscorlib and system assemblies.
    // They are separate types though, so normally the scanner scans both and
    // then we get conflicting labels. MS included it twice to make exception 
    // throwing code smaller. They are internal though, so we cannot
    // reference them directly and only via finding them as they come along.
    // We find it here, not via QueueType so we only check it here. Later
    // we might have to checkin QueueType also.
    // For now we accept both types, and just emit code for only one. This works
    // with the current Nasm assembler as we resolve by name in the assembler.
    // However with other assemblers this approach may not work.
    // If AssemblerNASM adds assembly name to the label, this will allow
    // both to exist as they do in BCL.
    // So in the future we might be able to remove this hack, or change
    // how it works.
    private Type mThrowHelper;

    public uint QueueMethod(object aSrc, string aSrcType, MethodBase aMethodBase
      , bool aIsPlug)
    {
      uint xResult;

      // If already queued, skip it and return reference to it
      if (mKnownMethods.TryGetValue(aMethodBase, out xResult)) {
        return xResult;
      } else if (mLogEnabled) {
        LogMapPoint(aSrc, aSrcType, aMethodBase);
      }

      xResult = (uint)mMethodsToProcess.Count;
      mKnownMethods.Add(aMethodBase, xResult);

      MethodInfo.TypeEnum xMethodType;
      MethodInfo xPlug = null;
      if (aIsPlug) {
        xMethodType = MethodInfo.TypeEnum.Plug;
      } else {
        xMethodType = MethodInfo.TypeEnum.Normal;
        if ((aMethodBase.Attributes & MethodAttributes.PinvokeImpl) != 0) {
          // pinvoke methods dont have an embedded implementation
          xMethodType = MethodInfo.TypeEnum.NeedsPlug;
        } else {
          var xImplFlags = aMethodBase.GetMethodImplementationFlags();
          if ((xImplFlags & MethodImplAttributes.Native) != 0) {
            // native implementations cannot be compiled
            xMethodType = MethodInfo.TypeEnum.NeedsPlug;
          }
        }

        // See if method has a plug
        uint xPlugId = 0;
        if (mMethodPlugs.TryGetValue(aMethodBase, out xPlugId)) {
          xPlug = mMethodsToProcess[(int)xPlugId];
        }

        // Queue Types directly related to method
        QueueType(aMethodBase, "Declaring Type", aMethodBase.DeclaringType);
        if (aMethodBase is System.Reflection.MethodInfo) {
          QueueType(aMethodBase, "Return Type", ((System.Reflection.MethodInfo)aMethodBase).ReturnType);
        }
        foreach (var xParam in aMethodBase.GetParameters()) {
          QueueType(aMethodBase, "Parameter", xParam.ParameterType);
        }
      }

      var xMethod = new MethodInfo(aMethodBase, xResult, xMethodType, xPlug);
      mMethodsToProcess.Add(xMethod);

      return xResult;
    }

    //protected void QueueStaticField(FieldInfo aFieldInfo) {
    //  if (!mFieldsSet.Contains(aFieldInfo)) {
    //    if (!aFieldInfo.IsStatic) {
    //      throw new Exception("Cannot queue instance fields!");
    //    }
    //    mFieldsSet.Add(aFieldInfo);
    //    QueueType(aFieldInfo.DeclaringType);
    //    QueueType(aFieldInfo.FieldType);
    //  }
    //}

    protected void LogMapPoint(object aSrc, string aSrcType, object aItem) {
      var xLogItem = new LogItem() {
        SrcType = aSrcType,
        Item = aItem
      };
      List<LogItem> xList;
      if (!mLogMap.TryGetValue(aSrc, out xList)) {
        xList = new List<LogItem>();
        mLogMap.Add(aSrc, xList);
      }
      xList.Add(xLogItem);
    }

    protected void QueueType(object aSrc, string aSrcType, Type aType) {
      if (mTypesSet.Contains(aType)) {
        return;
      } else if (mLogEnabled) {
        LogMapPoint(aSrc, aSrcType, aType);
      }

      //+		aType	{Name = "TextInfo" FullName = "System.Globalization.TextInfo"}	System.Type {System.RuntimeType}
      mTypesSet.Add(aType);
      mTypes.Add(aType);
      if (aType.BaseType != null) {
        QueueType(aType, "Base Type", aType.BaseType);
      }
      // queue static constructor
      foreach (var xCctor in aType.GetConstructors(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)) {
        if (xCctor.DeclaringType == aType) {
          QueueMethod(aType, "Static Constructor", xCctor, false);
        }
      }
    }

    public int MethodCount {
      get {
        return mMethodsToProcess.Count;
      }
    }
  }
}
