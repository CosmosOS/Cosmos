using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using X86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.Assembler.X86.X {
    public class Y86 {
        public RegisterEAX EAX = RegisterEAX.Instance;
        public RegisterAL AL = RegisterAL.Instance;

        public RegisterDX DX = RegisterDX.Instance;

        public PortSource Port(RegisterDX aDX) {
            return new PortSource("DX");
        }

        public string Label {
            set { 
                new Indy.IL2CPU.Assembler.Label(value); 
            }
        }

        public void JumpIfEqual(string aLabel) {
            new X86.JumpIfEqual(aLabel);
        }

    }
}
