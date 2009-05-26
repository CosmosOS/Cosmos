using System;
namespace IL2CPU.PostAssembler
{
    interface IOpCodeInstruction
    {
        string OpCode{ get; }

       // bool ContainsLabel(out string label);
    }
}
