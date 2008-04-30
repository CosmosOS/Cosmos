using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.X {
    public class MemoryAction {
        protected string mValue;
        public bool IsRegister = false;

        public override string ToString() {
            return mValue;
        }

        public static implicit operator MemoryAction(UInt32 aValue) {
            return new MemoryAction(aValue);
        }

        public static implicit operator MemoryAction(Register aRegister) {
            return new MemoryAction(aRegister.ToString(), true);
        }

        protected byte mSize = 0;

        // For registers
        public MemoryAction(string aValue, bool aIsRegister) {
            mValue = aValue;
            IsRegister = aIsRegister;
        }
        // This form used for reading memory - Addresses are passed in
        public MemoryAction(string aValue) {
            mValue = aValue;
        }
        public MemoryAction(string aValue, byte aSize) {
            mValue = aValue;
            mSize = aSize;
        }

        // For constants/literals
        public MemoryAction(UInt32 aValue) {
            mValue = aValue.ToString();
        }

        public static string SizeToString(byte aSize) {
            switch (aSize) {
                case 8:
                    return "byte";
                case 16:
                    return "word";
                case 32:
                    return "dword";
                case 64:
                    return "qword";
                default:
                    throw new Exception("Invalid size: " + aSize);
            }
        }

        //TODO: Put memory compare here - will later have to limit it to the size
        // variant and if possible to self usage, ie no assignments. May however result 
        // in too many class variants to be worth while.
        public void Compare(UInt32 aValue) {
            new Compare(SizeToString(mSize), ToString(), aValue);
        }

    }
}
