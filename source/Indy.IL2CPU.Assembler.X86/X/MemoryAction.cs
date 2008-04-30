using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.X {
    public class MemoryAction : Action {
        protected byte mSize = 0;

        // This form used for reading memory - Addresses are passed in
        public MemoryAction(string aValue) {
            mValue = aValue;
        }

        public MemoryAction(string aValue, byte aSize) {
            mValue = aValue;
            mSize = aSize;
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
                    throw new Exception("Invalid size: " + aSize.ToString());
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
