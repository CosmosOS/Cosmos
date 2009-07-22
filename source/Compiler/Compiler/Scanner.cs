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
    public partial class Scanner
    {
        private HashSet<string> mMethodNames = new HashSet<string>(StringComparer.InvariantCulture);
        private List<MethodBase> mMethods = new List<MethodBase>();

        private Func<Op>[] mOps;
        public Func<Op>[] Ops
        {
            get
            {
                return mOps;
            }
            set
            {
                if(value != mOps)
                {
                    if(value == null)
                    {
                        throw new Exception("Cannot set Ops to null");
                    }
                    if(value.Length != 0xFE1F)
                    {
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
            File.WriteAllLines(@"e:\cosmos.dbg", mMethodNames.ToArray());
        }

        private void ScanList()
        {
            for(int i = 0; i < mMethods.Count; i++)
            {
                ScanMethod(mMethods[i]);
            }
        }

        private void ScanMethod(MethodBase aMethodBase)
        {
            // pinvoke methods dont have an embedded implementation
            if ((aMethodBase.Attributes & MethodAttributes.PinvokeImpl) != 0)
            {
                // pinvoke
                return;
            }

            // abstract methods dont have an implementation
            if(aMethodBase.IsAbstract)
            {
                return;
            }

            var xImplFlags = aMethodBase.GetMethodImplementationFlags();
            if ((xImplFlags & MethodImplAttributes.Native) != 0)
            {
                // native implementations cannot be compiled
                return;
            }

            try
            {
                var xBody = aMethodBase.GetMethodBody();
                if (xBody == null)
                {
                    return;
                }
                using(var xReader = new ILReader(aMethodBase, xBody))
                {
                    while(xReader.Read())
                    {
                        var xCreate = mOps[(ushort) xReader.OpCode];
                        if(xCreate==null)
                        {
                            LogMissingOp(xReader.OpCode);
                            continue;
                        }
                        var xOp = xCreate();
                        QueueMethodCallCount = 0;
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
            }catch(Exception E)
            {
                throw new Exception("Error getting body!", E);
            }
        }

        private int QueueMethodCallCount = 0;
        public void QueueMethod(MethodBase aMethod)
        {
            QueueMethodCallCount++;
            var xName = aMethod.GetFullName();
            if (!mMethodNames.Contains(xName))
            {
                mMethodNames.Add(xName);
                mMethods.Add(aMethod);
            }
        }

        //private void

        public int MethodCount
        {
            get
            {
                return mMethods.Count;
            }
        }

        public int InstructionCount;
    }
}