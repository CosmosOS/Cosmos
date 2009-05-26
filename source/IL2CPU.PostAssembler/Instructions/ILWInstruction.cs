using System;
namespace IL2CPU.PostAssembler
{
    interface ILWInstruction
    {
        byte[] ToBinary();
        string ToString();


        LWInstructionType InstructionType { get; }

       // bool ContainsLabel(out string label);


        
    }
}
