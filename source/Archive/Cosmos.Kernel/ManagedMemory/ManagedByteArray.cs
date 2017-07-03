using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Kernel.ManagedMemory {
    public unsafe class ManagedByteArray {
        public ManagedByteArray(int aSize):this(aSize, 1) {
        }

        public ManagedByteArray(int aSize, int aAlignment) {
            mUnalignedData = new byte[aSize + aAlignment];
            if (aAlignment > 1) {
                fixed (byte* xFirstElemAddress = &mUnalignedData[0]) {
                    var xPtr = new IntPtr(xFirstElemAddress);
                    Address = xPtr.ToInt64();
                    if (Address % aAlignment != 0) {
                        mAlignmentCorrection = (int)(aAlignment - (Address % aAlignment));
                        Address += mAlignmentCorrection;
                    }
                }
            }
        }

        public byte this[int aIndex]{
            get{
                return mUnalignedData[aIndex + mAlignmentCorrection];
                }
            set {
                mUnalignedData[aIndex + mAlignmentCorrection] = value;
            }
        }

        private byte[] mUnalignedData;
        private int mAlignmentCorrection;
        public long Address {
            get;
            private set;
        }


        public IEnumerable<byte> Data {
            get {
                return mUnalignedData.Skip(mAlignmentCorrection);
            }
        }
    }
}