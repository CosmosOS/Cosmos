using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Assembler.X86
{
    /// <summary>
    /// This class is filled with data we get from the ILOps
    /// </summary>
    public class InstructionData : Cosmos.Assembler.X86.IInstructionData
    {

        public List<Operand> Operands { get; set; }

    }
}
