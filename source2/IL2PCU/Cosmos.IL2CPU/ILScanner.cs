using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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
    // know they are already done. Currently HashSet uses a refernece though, so we
    // need to hash on some UID instead of the refernce. Do not use strings, they are
    // super slow.
    private HashSet<MethodBase> mMethodsSet = new HashSet<MethodBase>();
    private List<MethodBase> mMethods = new List<MethodBase>();
    private HashSet<Type> mTypesSet = new HashSet<Type>();
    private List<Type> mTypes = new List<Type>();

    //TODO: This consumes 64k x 4 = 256 k. Not much, but all the ops seem in the low range.
    // Are the 16 bit ones all modifiers / prefixes?
    protected Func<ILOp>[] mOps;

    public ILScanner(Type aOpBaseType) {
      LoadOps(aOpBaseType);
    }

    protected void LoadOps(Type aOpBaseType) {
      //TODO: We can shrink this down, since all calls are FE00 - 00FF
      // We can split it into 2 because the scanner has to read one byte at a time
      // or we can change it to a signed int, and then add x0200 to the value.
      // This will reduce array size down to 768 entries.
      mOps = new Func<ILOp>[0xFE1F];
      foreach (var xType in aOpBaseType.Assembly.GetExportedTypes()) {
        if (xType.IsSubclassOf(aOpBaseType)) {
          var xAttrib = xType.GetCustomAttributes(typeof(OpCodeAttribute), false).FirstOrDefault() as OpCodeAttribute;
          if (xAttrib != null) {
            var xTemp = new DynamicMethod("Create_" + xAttrib.OpCode + "_Obj", typeof(ILOp), new Type[0], true);
            var xGen = xTemp.GetILGenerator();
            var xCtor = xType.GetConstructor(new Type[0]);
            xGen.Emit(OpCodes.Newobj, xCtor);
            xGen.Emit(OpCodes.Ret);
            mOps[(ushort)xAttrib.OpCode] = (Func<ILOp>)xTemp.CreateDelegate(typeof(Func<ILOp>));
          }
        }
      }
    }

    public void Execute(MethodInfo aEntry) {
      QueueMethod(aEntry);
      // Cannot use foreach, the list changes as we go
      for (int i = 0; i < mMethods.Count; i++) {
        ScanMethod(mMethods[i]);
      }
    }

    private void ScanMethod(MethodBase aMethodBase) {
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

      var xBody = aMethodBase.GetMethodBody();
      if (xBody == null) {
        return;
      }
      var xReader = new ILReader(aMethodBase, xBody);
      while (xReader.Read()) {
        // Kudzu:
        // Uncomment for debugging - has a small but noticable 
        // impact on runtime. Could be coincidental, but ran
        // tests several times with and with out and without
        // was consistently 0.5 secs faster on the Atom.
        // Does not make much sense though as its only used 13000
        // times or so, so possibly the compiling in is affecting
        // some CPU cache hit or other?
        //InstructionCount++;
        var xCreate = mOps[(ushort)xReader.OpCode];
        if (xCreate == null) {
          throw new Exception("Unrecognized IL Operation");
        }
        var xOp = xCreate();
        xOp.Scan(xReader, this);
      }
    }

    public void QueueMethod(MethodBase aMethod) {
      if (!mMethodsSet.Contains(aMethod)) {
        mMethodsSet.Add(aMethod);
        mMethods.Add(aMethod);
        QueueType(aMethod.DeclaringType);
      }
    }

    public void QueueType(Type aType) {
      if (aType != null) {
        if (!mTypesSet.Contains(aType)) {
          mTypesSet.Add(aType);
          mTypes.Add(aType);
          QueueType(aType.BaseType);
          foreach (var xMethod in aType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
            if (xMethod.DeclaringType == aType) {
              if (xMethod.IsVirtual) {
                QueueMethod(xMethod);
              }
            }
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
