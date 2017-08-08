using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Cosmos.Assembler.x86 {
    [Cosmos.Assembler.OpCode("ret")]
	public class Return: Instruction {
        public Return() {
            DestinationValue = 0;
        }

        public uint DestinationValue
        {
            get;
            set;
        }

        public override void WriteText(Assembler aAssembler, TextWriter aOutput)
        {
            if (DestinationValue == 0)
            {
                aOutput.WriteLine("Ret");
            }
            else
            {
                aOutput.WriteLine("Ret 0x" + DestinationValue.ToString("X"));
            }
        }
	}
}
