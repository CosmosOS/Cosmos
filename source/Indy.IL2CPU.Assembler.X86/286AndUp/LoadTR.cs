using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86
{
    [OpCode(0xFFFFFFFF, "ltr")]
    public class LoadTR: Instruction
    {
        public string Selector;
        public LoadTR(string aSelector)
        {
            Selector = aSelector;
        }
        public override string ToString()
        {

            return "ltr " + Selector; 
        }
    }
}