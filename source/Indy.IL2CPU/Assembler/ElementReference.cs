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
        private static SortedList<string, BaseAssemblerElement> mCache = new SortedList<string, BaseAssemblerElement>();

        private static BaseAssemblerElement DoResolve(Assembler aAssembler, string aName) {
            mCacheLocker.EnterReadLock();
            try {
                BaseAssemblerElement xTempResult;
                if (mCache.TryGetValue(aName, out xTempResult)) {
                    return xTempResult;
                }
            } finally {
                mCacheLocker.ExitReadLock();
            }
            mCacheLocker.EnterWriteLock();
            try {
                BaseAssemblerElement xTempResult;
                if (mCache.TryGetValue(aName, out xTempResult)) {
                    return xTempResult;
                }
                xTempResult = (from item in aAssembler.Instructions
                               let xLabel = item as Label
                               where xLabel != null && xLabel.QualifiedName.Equals(aName, StringComparison.InvariantCultureIgnoreCase)
                               select item).SingleOrDefault();
                if (xTempResult == null) {
                    xTempResult = (from item in aAssembler.DataMembers
                                where item.Name.Equals(aName, StringComparison.InvariantCultureIgnoreCase)
                                select item).SingleOrDefault();
                }
                mCache.Add(aName, xTempResult);
                return xTempResult;
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
