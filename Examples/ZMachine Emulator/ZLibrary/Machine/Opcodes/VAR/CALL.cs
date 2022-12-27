using System.Collections.Generic;
using ZLibrary.Constants;

namespace ZLibrary.Machine.Opcodes.VAR
{
    /// <summary>
    /// Call a routine and store its result.
    /// </summary>
    public class CALL : Opcode
    {
        public CALL(ZMachine machine)
            : base(machine)
        {
            Name = "VAR:0x00 call routine [arg1, arg2, arg3] -> (result) call";
        }

        public override void Execute(ushort aRoutineAddress, ushort aArg1, ushort aArg2, ushort aArg3, ushort aArg4, ushort aArg5, ushort aArg6, ushort aArg7, ushort aArgCount)
        {
            if (aRoutineAddress != 0)
            {
                aArgCount--;
                var xArgs = new List<ushort>();
                if (aArgCount > 0)
                {
                    xArgs.Add(aArg1);
                }
                if (aArgCount > 1)
                {
                    xArgs.Add(aArg2);
                }
                if (aArgCount > 2)
                {
                    xArgs.Add(aArg3);
                }
                if (aArgCount > 3)
                {
                    xArgs.Add(aArg4);
                }
                if (aArgCount > 4)
                {
                    xArgs.Add(aArg5);
                }
                if (aArgCount > 5)
                {
                    xArgs.Add(aArg6);
                }
                if (aArgCount > 6)
                {
                    xArgs.Add(aArg7);
                }

                Call(aRoutineAddress, xArgs, CallType.CallStore);
            }
            else
            {
                Store(0);
            }
        }
    }
}
