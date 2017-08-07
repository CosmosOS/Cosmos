using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Cosmos.Assembler
{
    public class ElementReference
    {
        public static ElementReference New(string aName, int aOffset)
        {
            ElementReference xResult;
            if (aName.StartsWith("."))
            {
                aName = Label.LastFullLabel + aName;
            }

            if (aOffset == 0)
            {
                xResult = new ElementReference(aName);
            }
            else
            {
                xResult = new ElementReference(aName, aOffset);
            }

            return xResult;
        }

        public static ElementReference New(string aName)
        {
            return New(aName, 0);
        }

        private ElementReference(string aName, int aOffset)
            : this(aName)
        {
            Offset = aOffset;
        }

        private ElementReference(string aName)
        {
            if (String.IsNullOrEmpty(aName))
            {
                throw new ArgumentNullException("aName");
            }
            if (aName == "00h")
            {
                Console.Write("");
            }
            if (aName.StartsWith("."))
            {
                Name = Label.LastFullLabel + aName;
            }
            else
            {
                Name = aName;
            }
        }

        private ulong? mActualAddress;

        private static ReaderWriterLockSlim mCacheLocker = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private static Dictionary<string, BaseAssemblerElement> mCache; // = new SortedList<string, BaseAssemblerElement>(StringComparer.InvariantCultureIgnoreCase);
        private static int? mThreadId = null;

        private static BaseAssemblerElement DoResolve(Cosmos.Assembler.Assembler aAssembler, string aName)
        {
            if (!mThreadId.HasValue)
            {
                mThreadId = Thread.CurrentThread.ManagedThreadId;
            }
            else
            {
                if (mThreadId.Value != Thread.CurrentThread.ManagedThreadId)
                {
                    throw new Exception("Called from multiple threads");
                }
            }
            mCacheLocker.EnterReadLock();
            try
            {
                if (mCache != null)
                {
                    BaseAssemblerElement xTempResult;
                    if (mCache.TryGetValue(aName, out xTempResult))
                    {
                        return xTempResult;
                    }
                }
            }
            finally
            {
                mCacheLocker.ExitReadLock();
            }
            mCacheLocker.EnterWriteLock();
            try
            {
                if (mCache == null)
                {
                    mCache = new Dictionary<string, BaseAssemblerElement>(StringComparer.OrdinalIgnoreCase);
                    int xMax = aAssembler.AllAssemblerElementCount;
                    for (int i = 0; i < xMax; i++)
                    {
                        var xInstruction = aAssembler.GetAssemblerElement(i);
                        var xLabel = xInstruction as Label;
                        if (xLabel != null)
                        {
                            mCache.Add(xLabel.QualifiedName, xLabel);
                        }
                        var xDataMember = xInstruction as DataMember;
                        if (xDataMember != null)
                        {
                            if (mCache.ContainsKey(xDataMember.Name))
                            {
                                Console.Write("");
                            }
                            mCache.Add(xDataMember.Name, xDataMember);
                        }
                    }
                }
                BaseAssemblerElement xTempResult;
                if (mCache.TryGetValue(aName, out xTempResult))
                {
                    return xTempResult;
                }
                throw new Exception("Cannot resolve ElementReference to '" + aName + "'!");
                //foreach(var xInstruction in aAssembler.Instructions ) {
                //    var xLabel = xInstruction as Label;
                //    if(xLabel!=null) {
                //        if(aName.Equals(xLabel.Name, StringComparison.InvariantCultureIgnoreCase)) {
                //            xTempResult = xLabel;
                //            break;
                //        }
                //    }
                //}
                //if (xTempResult == null) {
                //    foreach (var xDataMember in aAssembler.DataMembers) {
                //        if (aName.Equals(xDataMember.Name, StringComparison.InvariantCultureIgnoreCase)) {
                //            xTempResult = xDataMember;
                //            break;
                //        }
                //    }
                //}
            }
            finally
            {
                mCacheLocker.ExitWriteLock();
            }
        }

        public bool Resolve(Cosmos.Assembler.Assembler aAssembler, out ulong aAddress)
        {
            //
            if (mActualAddress != null)
            {
                aAddress = mActualAddress.Value;
                return true;
            }
            var xElement = DoResolve(aAssembler, Name);
            if (xElement != null)
            {
                if (xElement.ActualAddress.HasValue)
                {
                    mActualAddress = (ulong) ((long) xElement.ActualAddress.Value + Offset);
                    aAddress = mActualAddress.Value;
                    return true;
                }
            }

            aAddress = 0;
            return false;
        }

        public int Offset { get; set; }

        public string Name { get; set; }

        public override string ToString()
        {
            if (Offset != 0)
            {
                return Name + " + " + Offset;
            }
            else
            {
                return Name;
            }
        }
    }
}
