namespace ZLibrary.Machine.Opcodes.VAR
{
    /// <summary>
    /// Pop a value off the game stack and
    /// store it at address (V6)
    /// store it in a variable (!V6) 
    /// </summary>
    public class PULL : Opcode
    {
        public PULL(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "VAR:0x09 pull(variable), pull stack -> (result)";
        }

        public override void Execute(ushort aAddressOrVariable, ushort aArg1, ushort aArg2, ushort aArg3, ushort aArg4, ushort aArg5, ushort aArg6, ushort aArg7, ushort aArgCount)
        {
            ushort xValue = Machine.Memory.Stack.Pop();

            if (aAddressOrVariable == 0)
            {
                Machine.Memory.Stack[Machine.Memory.Stack.SP] = xValue;
            }
            else if (aAddressOrVariable < 16)
            {
                Machine.Memory.Stack[Machine.Memory.Stack.BP - aAddressOrVariable] = xValue;
            }
            else
            {
                ushort xAddress = (ushort) (Machine.Story.Header.GlobalsOffset + 2 * (aAddressOrVariable - 16));
                Machine.Memory.SetWord(xAddress, xValue);
            }
        }
    }
}
