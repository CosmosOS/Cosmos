using System;
using System.Collections.Generic;
namespace Cosmos.Assembler.X86
{
    public interface IInstructionData
    {

        List<Operand> Operands { get; set; }

    }
}
