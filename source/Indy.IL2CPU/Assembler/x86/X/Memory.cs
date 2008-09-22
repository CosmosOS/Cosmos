using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.X {
    public class Memory {
        public MemoryAction this[Address aAddress] {
            get {
                return new MemoryAction(aAddress.ToString());
            }
            set {
                if (value.IsRegister) {
                    new X86.Move(aAddress.ToString(), value.ToString());
                } else {
                    throw new Exception("For non register assignments to memory, a size must be specified.");
                }
            }
        }

        public MemoryAction this[Address aAddress, byte aSize] {
            get {
                return new MemoryAction(aAddress.ToString(), aSize);
            }
            set {
                // ++ operators return ++
                // Maybe later change ++ etc to return actions?
                if (value != null) {
                    new X86.Move(MemoryAction.SizeToString(aSize), aAddress.ToString(), value.ToString());
                }
            }
        }
    }
}
