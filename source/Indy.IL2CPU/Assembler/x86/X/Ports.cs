using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.X {
    public class Ports {
        public PortNumber this[byte aPort] {
            get { 
                return new PortNumber(aPort.ToString());
            }
            set {
                new X86.Out("DX", aPort.ToString());
            }
        }

        public PortNumber this[RegisterDX aDX] {
            get {
                return new PortNumber("DX");
            }
            set { 
                new X86.Out("DX", value.ToString());  
            }
        }
    }
}
