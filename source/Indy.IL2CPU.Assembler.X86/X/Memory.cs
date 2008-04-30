using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.X {
    public class Memory {
        public Action this[Address aAddress] {
            get {
                return new Action(aAddress.ToString());
            }
            set {
                if (value.IsRegister) {
                    new X86.Move(aAddress.ToString(), value.ToString());
                } else {
                    throw new Exception("For non register assignments to memory, a size must be specified.");
                }
            }
        }

        public Action this[Address aAddress, byte aSize] {
            get {
                return new Action(aAddress.ToString());
            }
            set {
                new X86.Move(Action.SizeToString(aSize), aAddress.ToString(), value.ToString());
            }
        }
    }
}
