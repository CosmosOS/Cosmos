using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Cosmos.IL2CPU {
	public class ILScanner {
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
    private HashSet<MethodBase> mMethodsSet = new HashSet<MethodBase>();
    private List<MethodBase> mMethods = new List<MethodBase>();
    private HashSet<Type> mTypesSet = new HashSet<Type>();
    private List<Type> mTypes = new List<Type>();

    protected ILReader mReader;
    protected Assembler mAsmblr;

    public ILScanner(Assembler aAsmblr) {
      mAsmblr = aAsmblr;
      mReader = new ILReader();
    }

    public void Execute(MethodInfo aEntry) {
      QueueMethod(aEntry);

      // Cannot use foreach, the list changes as we go
      for (int i = 0; i < mMethods.Count; i++) {
				ScanMethod(mMethods[i], (UInt32)i);
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
        xMethodCount = mMethods.Count;
        // Cannot use foreach, the list changes as we go
        for (int i = 0; i < mMethods.Count; i++) {
          var xMethod = mMethods[i];
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
      } while (xMethodCount != mMethods.Count);
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
        mAsmblr.ProcessMethod(aMethodUID, xOpCodes);
        foreach (var xOpCode in xOpCodes) {
          //InstructionCount++;
          if (xOpCode is ILOpCodes.OpMethod) {
            QueueMethod(((ILOpCodes.OpMethod)xOpCode).Value);
          } else if (xOpCode is ILOpCodes.OpType) {
            QueueType(((ILOpCodes.OpType)xOpCode).Value);
          }
        }
      }
    }

    protected void QueueMethod(MethodBase aMethod) {
      if (!mMethodsSet.Contains(aMethod)) {
        mMethodsSet.Add(aMethod);
        mMethods.Add(aMethod);
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
      }
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
        return mMethods.Count;
      }
    }

  }
}
