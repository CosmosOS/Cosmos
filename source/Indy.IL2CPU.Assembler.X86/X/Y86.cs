using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using X86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.Assembler.X86.X {
    public class Y86 {
        public enum Flags { 
            Zero, NotZero, Equal, NotEqual // Zero is synonym for Equal
        };

        public RegisterEAX EAX = RegisterEAX.Instance;
        public RegisterAL AL = RegisterAL.Instance;

        public RegisterDX DX = RegisterDX.Instance;

        public RegisterESP ESP = RegisterESP.Instance;

        public RegisterEBP EBP = RegisterEBP.Instance;

        public readonly Ports Port = new Ports();
        public readonly Memory Memory = new Memory();

        public string Label {
            set { 
                new Indy.IL2CPU.Assembler.Label(value); 
            }
        }

        public void Call(string aLabel) {
            new X86.Call(aLabel);
        }

        public void Jump(string aLabel) {
            new X86.Jump(aLabel);
        }

        public void JumpIf(Flags aFlags, string aLabel) {
            switch (aFlags) {
                case Flags.Zero:
                case Flags.Equal:
                    new X86.JumpIfZero(aLabel);
                    break;
                case Flags.NotZero:
                case Flags.NotEqual:
                    new X86.JumpIfNotZero(aLabel);
                    break;
            }
        }

        public void PopAll32() {
            new Popad();
        }

        public void PushAll32() {
            new Pushad();
        }
        
        public void Return() {
            new X86.Ret();
        }
        
        public void Return(UInt16 aBytes) {
            new X86.Ret(aBytes);
        }

    }
}
