using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Core;

namespace Cosmos.Kernel.ManagedMemory {
    public unsafe class ManagedUInt32Array {
        public ManagedUInt32Array(int aSize):this(aSize, 4) {
        }

        public ManagedUInt32Array(int aSize, int aAlignment) {
            if (aAlignment > 1) {
                Address = Heap.MemAlloc((uint)((aSize * 4) + aAlignment));
                mUnalignedData = (uint*)Address;
                mAlignmentCorrection = (int)(aAlignment - (Address % aAlignment));
                Address += mAlignmentCorrection;
                mAlignedData = (uint*)Address;
            }else{
                Address = Heap.MemAlloc((uint)(aSize * 4));
                mUnalignedData = (uint*)Address;
                mAlignedData = mUnalignedData;
            }
        }

        public UInt32 this[int aIndex]{
            get{
                return mAlignedData[aIndex];
                }
            set {
                mAlignedData[aIndex] = value;
            }
        }

        private UInt32* mUnalignedData;
        private uint* mAlignedData;
        private int mAlignmentCorrection;
        public long Address;
    }
}