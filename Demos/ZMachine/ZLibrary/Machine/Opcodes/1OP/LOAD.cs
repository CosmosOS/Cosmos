using System;

namespace ZLibrary.Machine.Opcodes._1OP
{
    /// <summary>
    /// Load and store the value of a variable.
    /// </summary>
    public class LOAD : Opcode
    {
        public LOAD(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "1OP:0x0E load (variable) -> (result)";
        }

        public override void Execute(ushort aVariable)
        {
            throw new NotImplementedException();
        }
    }
}
