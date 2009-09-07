using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Indy.IL2CPU.Plugs; 

namespace Cosmos.IL2CPU {
  public class ILScanner {
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
    private Dictionary<MethodBase, uint> mKnownMethods = new Dictionary<MethodBase, uint>();
    // We need a separate list because we cannot iterate keys by index, and any functions
    // to get a list of keys will do a on demand copy, which won't meet our needs either
    // becuase we have to walk the list dynamically as it grows, which is also why we need to
    // index it rather than enumerate it with foreach.
    // We also need a separate list becuase Execute is called multiple
    // times to process plugs and so known methods accumulates,
    // but we dont want to reproces old methods from previous Execute calls.
    private List<MethodBase> mMethodsToProcess = new List<MethodBase>();
    // ExecuteInternal is called multiple times, we don't want to rescan
    // ones that are "finished" so we update this "pointer"
    private int mMethodsToProcessStart;

    //TODO: Likely change this to be like Methods to be more efficient. Might only need Dictionary
    private HashSet<Type> mTypesSet = new HashSet<Type>();
    private List<Type> mTypes = new List<Type>();

    protected ILReader mReader;
    protected Assembler mAsmblr;

    public ILScanner(Assembler aAsmblr) {
      mAsmblr = aAsmblr;
      mReader = new ILReader();
    }

    public void Execute(System.Reflection.MethodInfo aStartMethod) {
      // Scan plugs first, so when we scan from 
      // entry point plugs will be found.
      //TODO: Move plug scans etc into Scanner
      foreach (var xAsm in AppDomain.CurrentDomain.GetAssemblies()) {
        foreach (var xType in xAsm.GetTypes()) {
          foreach (var xMethod in xType.GetMethods()) {
            foreach (var xAttrib in xMethod.GetCustomAttributes(false)) {
              if (xAttrib is PlugMethodAttribute) {
                var x = (PlugMethodAttribute)xAttrib;
                if (x.Enabled) {
                  //TODO: Check this against build options
                  //TODO: Two exclusive IsOnly's dont make sense
                  // refactor these as a positive rather than negative
                  if (!x.IsMonoOnly) {
                    //TODO: public string Signature = null;
                    // Do we need to check signature?
                    //TODO: public Type Assembler = null;
                    ExecuteInternal(xMethod);
                  }
                }
              }
            }
          }
          //TODO: Look for Field plugs
        }
      }

      // Scan from entry point of this program
      //TODO: Now that we scan plugs first, we might need to put a jump
      // in the asm to jump to the entry point?
      ExecuteInternal(aStartMethod);
    }

    private void ExecuteInternal(System.Reflection.MethodInfo aStartMethod) {
      // See comment at mMethodsToProcessStart declaration
      mMethodsToProcessStart = mMethodsToProcess.Count;
      QueueMethod(aStartMethod);

      // Cannot use foreach, the list changes as we go
      // and we dont start at 0
      for (int i = mMethodsToProcessStart; i < mMethodsToProcess.Count; i++) {
        ScanMethod(mMethodsToProcess[i], (UInt32)i);
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
          var xMethod = mMethodsToProcess[i];
          if (xMethod.IsVirtual) {
            foreach (var xType in mTypes) {
              // Find ancestors and descendants
              if (xType.IsSubclassOf(xMethod.DeclaringType) || xMethod.DeclaringType.IsSubclassOf(xType)) {
                var xParams = xMethod.GetParameters();
                var xParamTypes = new Type[xParams.Length];
                for (int j = 0; j < xParams.Length; j++) {
                  xParamTypes[j] = xParams[j].ParameterType;
                }
                var xNewMethod = xType.GetMethod(xMethod.Name, xParamTypes);
                if (xNewMethod != null) {
                  QueueMethod(xNewMethod);
                }
              }
            }
          }
        }
      } while (xMethodCount != mMethodsToProcess.Count);
    }

    private void ScanMethod(MethodBase aMethodBase, UInt32 aMethodUID) {
      if ((aMethodBase.Attributes & MethodAttributes.PinvokeImpl) != 0) {
        // pinvoke methods dont have an embedded implementation
        return;
      } else if (aMethodBase.IsAbstract) {
        // abstract methods dont have an implementation
        return;
      }

      var xImplFlags = aMethodBase.GetMethodImplementationFlags();
      if ((xImplFlags & MethodImplAttributes.Native) != 0) {
        // native implementations cannot be compiled
        return;
      }
      
      var xOpCodes = mReader.ProcessMethod(aMethodBase);
      if (xOpCodes != null) {
        // Call ProcessMethod first, in a threaded environment it will
        // allow more threads to work slightly sooner
        var xMethod = new MethodInfo(aMethodBase, aMethodUID);

        foreach (var xOpCode in xOpCodes) {
          //InstructionCount++;
          if (xOpCode is ILOpCodes.OpMethod) {
            ((ILOpCodes.OpMethod)xOpCode).ValueUID = QueueMethod(((ILOpCodes.OpMethod)xOpCode).Value);
          } else if (xOpCode is ILOpCodes.OpType) {
            QueueType(((ILOpCodes.OpType)xOpCode).Value);
          }
        }

        // Assemble the method
        mAsmblr.ProcessMethod(xMethod, xOpCodes);
      }
    }

    public uint QueueMethod(MethodBase aMethod) {
      uint xResult;

      // If already queued, skip it
      if (mKnownMethods.TryGetValue(aMethod, out xResult)) {
        return xResult;
      }
      
      xResult = (uint)mMethodsToProcess.Count;
      mKnownMethods.Add(aMethod, xResult);
      mMethodsToProcess.Add(aMethod);

      //TODO: Might still need this one, see after we get assembly output again
      //Im hoping the operand walking we have now ill include this on its own.
      //QueueType(aMethod.DeclaringType);

      //var xMethodInfo = aMethod as MethodInfo;
      //if (xMethodInfo != null) {
      //  QueueType(xMethodInfo.ReturnType);
      //}
      //foreach (var xParam in aMethod.GetParameters()) {
      //  QueueType(xParam.ParameterType);
      //}
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

    protected void QueueType(Type aType) {
      if (!mTypesSet.Contains(aType)) {
        mTypesSet.Add(aType);
        mTypes.Add(aType);
        if (aType.BaseType != null) {
          QueueType(aType.BaseType);
        }
        // queue static constructor
        foreach (var xCctor in aType.GetConstructors(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)) {
          if (xCctor.DeclaringType == aType) {
            QueueMethod(xCctor);
          }
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
