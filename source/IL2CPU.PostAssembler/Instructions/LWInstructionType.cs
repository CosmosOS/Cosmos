using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IL2CPU.PostAssembler
{
    public enum LWInstructionType
    {
        Label,
        Comment, 
        BinaryInstruction,
        BinaryInstructionWithLabel,
        AsmInstruction,
        AsmInstructionWithLabel,
    }
}
