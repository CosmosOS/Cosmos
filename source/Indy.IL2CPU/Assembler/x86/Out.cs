using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	[OpCode(0xFFFFFFFF, "out")]
	public class Out: Instruction {
        public byte Size {
            get;
            set;
        }

        public byte? Port {
            get;
            set;
        }

        public override string ToString() {
            string xData = "";
            switch(Size){
                case 8: xData = "al"; break;
                case 16: xData = "ax"; break;
                case 32: xData = "eax"; break;
                default: throw new Exception("Size " + Size + " not supported in OUT instruction");
            }
            return "out " + (Port.HasValue ? Port.ToString() : "DX") + ", " + xData;
        }
	}
}
