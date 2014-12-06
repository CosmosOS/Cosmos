using System;

namespace Cosmos.Assembler.x86
{
    [Flags]
    public enum InstructionPrefixes
    {
        None,
        Lock,
        Repeat,
        RepeatTillEqual
    }

    public interface IInstructionWithPrefix
    {
        InstructionPrefixes Prefixes
        {
            get;
            set;
        }
    }
}