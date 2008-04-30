using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.X {
    public class Memory {
        public Action this[Address aAddress] {
            get {
                return new MemoryAction(aAddress.ToString());
            }
            set {
                // Future - need to handle registers, but should reject
                // any memory, memory should force size usage below
                if (value is RegisterAction) {
                    new X86.Move(aAddress.ToString(), value.ToString());
                } else {
                    throw new Exception("For non register assignments to memory, a size must be specified.");
                }
            }
        }

        public Action this[Address aAddress, byte aSize] {
            get {
                return new MemoryAction(aAddress.ToString());
            }
            set {
                string xSizeString;
                switch (aSize) {
                    case 8:
                        xSizeString = "byte";
                        break;
                    case 16:
                        xSizeString = "word";
                        break;
                    case 32:
                        xSizeString = "dword";
                        break;
                    case 64:
                        xSizeString = "qword";
                        break;
                    default:
                        throw new Exception("Invalid size: " + aSize.ToString());
                }
                new X86.Move(xSizeString, aAddress.ToString(), value.ToString());
            }
        }
    }
}
