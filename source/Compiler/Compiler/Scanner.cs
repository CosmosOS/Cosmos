using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Cosmos.Compiler.IL;
using Cosmos.Compiler.ILScanner;

namespace Cosmos.Compiler
{
    /// <summary>
    /// New scanner engine. will scan the given entrypoint method, and all neccessary stuff inside. the vmt scan performed will include all methods on a per-type
    /// base.
    /// </summary>
    public partial class Scanner
    {
      private HashSet<MethodBase> mMethodsSet = new HashSet<MethodBase>();
      private List<MethodBase> mMethods = new List<MethodBase>();
      private HashSet<Type> mTypesSet = new HashSet<Type>();
      private List<Type> mTypes = new List<Type>();

        private Func<Op>[] mOps;
        public Func<Op>[] Ops
        {
            get
            {
                return mOps;
            }
            set
            {
                if(value != mOps) {
                    if(value == null) {
                        throw new Exception("Cannot set Ops to null");
                    } else if (value.Length != 0xFE1F) {
                        throw new Exception("Element count mismatch!");
                    }
                    mOps = value;
                }
            }
        }
        public void Execute(MethodInfo aEntry)
        {
            InitDebug();
            QueueMethod(aEntry);
            ScanList();
            //File.WriteAllLines(@"e:\cosmos.dbg", mMethodNames.ToArray());
        }

        private void ScanList() {
          // Cannot use foreach, the list changes as we go
            for(int i = 0; i < mMethods.Count; i++)
            {
                ScanMethod(mMethods[i]);
            }
        }

        private void ScanMethod(MethodBase aMethodBase)
        {
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

            try
            {
                var xBody = aMethodBase.GetMethodBody();
                if (xBody == null) {
                    return;
                }
                using(var xReader = new ILReader(aMethodBase, xBody)) {
                    while(xReader.Read()) {
                        // Kudzu:
                        // Uncomment for debugging - has a small but noticable 
                        // impact on runtime. Could be coincidental, but ran
                        // tests several times with and with out and without
                        // was consistently 0.5 secs faster on the Atom.
                        // Does not make much sense though as its only used 13000
                        // times or so, so possibly the compiling in is affecting
                        // some CPU cache hit or other?
                        //InstructionCount++;
                        var xCreate = mOps[(ushort) xReader.OpCode];
                        if(xCreate == null) {
                            LogMissingOp(xReader.OpCode);
                            continue;
                        }
                        var xOp = xCreate();
                        xOp.Scan(xReader, this);
                        // TEMP
                        //if (xReader.OperandValueMethod!=null)
                        //{
                        //    if(QueueMethodCallCount== 0)
                        //    {
                        //        throw new Exception("Instruction " + xReader.OpCode + " should have queued a method");
                        //    }
                        //}
                    }
                }
            } catch(Exception E) {
                throw new Exception("Error getting body!", E);
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

        public int MethodCount
        {
            get
            {
                return mMethods.Count;
            }
        }

        public int InstructionCount;

        //private static MethodBase GetUltimateBaseMethod(MethodBase aMethod,
        //                                        Type[] aMethodParams,
        //                                        Type aCurrentInspectedType)
        //{
        //    MethodBase xBaseMethod = null;
        //    //try {
        //    while (true)
        //    {
        //        if (aCurrentInspectedType.BaseType == null)
        //        {
        //            break;
        //        }
        //        aCurrentInspectedType = aCurrentInspectedType.BaseType;
        //        MethodBase xFoundMethod = aCurrentInspectedType.GetMethod(aMethod.Name,
        //                                                                  BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
        //                                                                  Type.DefaultBinder,
        //                                                                  aMethodParams,
        //                                                                  new ParameterModifier[0]);
        //        if (xFoundMethod == null)
        //        {
        //            break;
        //        }
        //        ParameterInfo[] xParams = xFoundMethod.GetParameters();
        //        bool xContinue = true;
        //        for (int i = 0; i < xParams.Length; i++)
        //        {
        //            if (xParams[i].ParameterType != aMethodParams[i])
        //            {
        //                xContinue = false;
        //                break;
        //            }
        //        }
        //        if (!xContinue)
        //        {
        //            continue;
        //        }
        //        //if (xFoundMethod != null)
        //        //{
        //        //    xBaseMethod = xFoundMethod;

        //        //    if (xFoundMethod.IsVirtual == aMethod.IsVirtual && xFoundMethod.IsPrivate == false && xFoundMethod.IsPublic == aMethod.IsPublic && xFoundMethod.IsFamily == aMethod.IsFamily && xFoundMethod.IsFamilyAndAssembly == aMethod.IsFamilyAndAssembly && xFoundMethod.IsFamilyOrAssembly == aMethod.IsFamilyOrAssembly && xFoundMethod.IsFinal == false)
        //        //    {
        //        //        var xFoundMethInfo = xFoundMethod as MethodInfo;
        //        //        var xBaseMethInfo = xBaseMethod as MethodInfo;
        //        //        if (xFoundMethInfo == null && xBaseMethInfo == null)
        //        //        {
        //        //            xBaseMethod = xFoundMethod;
        //        //        }
        //        //        if (xFoundMethInfo != null && xBaseMethInfo != null)
        //        //        {
        //        //            if (xFoundMethInfo.ReturnType.AssemblyQualifiedName.Equals(xBaseMethInfo.ReturnType.AssemblyQualifiedName))
        //        //            {
        //        //                xBaseMethod = xFoundMethod;
        //        //            }
        //        //        }
        //        //        //xBaseMethod = xFoundMethod;
        //        //    }
        //        //}
        //        //else
        //        //{
        //        xBaseMethod = xFoundMethod;
        //        //}
        //    }
        //    //} catch (Exception) {
        //    // todo: try to get rid of the try..catch
        //    //}
        //    return xBaseMethod ?? aMethod;
        //}
    }
}