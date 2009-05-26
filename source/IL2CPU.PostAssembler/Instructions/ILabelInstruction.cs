using System;
namespace IL2CPU.PostAssembler
{
    interface ILabelInstruction
    {
        string Label  { get; }

       // bool ContainsLabel(out string label);
    }
}
