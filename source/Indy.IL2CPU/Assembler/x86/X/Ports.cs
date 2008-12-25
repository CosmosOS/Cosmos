using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.X {
    public class Ports {
       public PortNumber this[RegisterDX aDX] {
            get {
                return new PortNumber(aDX.GetId());
            }
            set {
                new X86.Out {DestinationReg=Registers.EAX};
            }
        }
    }
}
