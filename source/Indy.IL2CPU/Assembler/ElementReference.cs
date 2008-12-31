using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Indy.IL2CPU.Assembler {
    public class ElementReference {
        public ElementReference(string aName, int aOffset)
            : this(aName) {
            Offset = aOffset;
        }

        public ElementReference(string aName) {
            if (aName == "00h") {
                Console.Write("");
            }
            if (aName.StartsWith(".")) {
                Name = Label.LastFullLabel + aName;
            } else {
                Name = aName;
            }
        }

        private ulong? mActualAddress;

        private static ReaderWriterLockSlim mCacheLocker = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private static Dictionary<string, BaseAssemblerElement> mCache;// = new SortedList<string, BaseAssemblerElement>(StringComparer.InvariantCultureIgnoreCase);
        private static int? mThreadId = null;

        private static BaseAssemblerElement DoResolve(Assembler aAssembler, string aName) {
            if(!mThreadId.HasValue) {
                mThreadId = Thread.CurrentThread.ManagedThreadId;
            }else {
                if(mThreadId.Value != Thread.CurrentThread.ManagedThreadId) {
                    throw new Exception("Called from multiple threads");
                }
            }
            mCacheLocker.EnterReadLock();
            try {
                if (mCache != null) {
                    BaseAssemblerElement xTempResult;
                    if (mCache.TryGetValue(aName, out xTempResult)) {
                        return xTempResult;
                    }
                }
            } finally {
                mCacheLocker.ExitReadLock();
            }
            mCacheLocker.EnterWriteLock();
            try {
                if(mCache == null) {
                    mCache = new Dictionary<string, BaseAssemblerElement>(StringComparer.InvariantCultureIgnoreCase);
                    foreach (var xInstruction in aAssembler.mAllAssemblerElements) {
                        var xLabel = xInstruction as Label;
                        if (xLabel != null) {
                            mCache.Add(xLabel.QualifiedName, xLabel);
                        }
                        var xDataMember = xInstruction as DataMember;
                        if (xDataMember != null) {
                            if (mCache.ContainsKey(xDataMember.Name)) {
                                Console.Write("");
                            }
                            mCache.Add(xDataMember.Name, xDataMember);
                        }
                    }
                }
                BaseAssemblerElement xTempResult;
                if (mCache.TryGetValue(aName, out xTempResult)) {
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
            } finally {
                mCacheLocker.ExitWriteLock();
            }
        }

        public bool Resolve(Assembler aAssembler, out ulong aAddress) {
            //
            if (mActualAddress != null) {
                aAddress = mActualAddress.Value;
                return true;
            }
            var xElement = DoResolve(aAssembler, Name);
            if (xElement != null) {
                if (xElement.ActualAddress.HasValue) {
                    mActualAddress = (ulong)((long)xElement.ActualAddress.Value + Offset);
                    aAddress = mActualAddress.Value;
                    return true;
                }
            }

            aAddress = 0;
            return false;
        }

        public int Offset {
            get;
            set;
        }

        public string Name {
            get;
            set;
        }

        public override string ToString() {
            if (Offset != 0) {
                return Label.FilterStringForIncorrectChars(Name) + " + " + Offset;
            }else {
                return Label.FilterStringForIncorrectChars(Name);
            }
        }
    }
}
