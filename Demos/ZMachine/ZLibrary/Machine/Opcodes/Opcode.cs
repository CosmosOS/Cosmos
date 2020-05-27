using System;
using System.Collections.Generic;
using ZLibrary.Constants;

namespace ZLibrary.Machine.Opcodes
{
    public abstract class Opcode
    {
        protected string Name { get; set; }

        protected ZMachine Machine { get; }

        protected Opcode(ZMachine machine)
        {
            Machine = machine;
        }

        public override string ToString() => Name;

        public virtual void Execute()
        {
            throw new NotImplementedException();
        }

        public virtual void Execute(ushort aArg0)
        {
            throw new NotImplementedException();
        }

        public virtual void Execute(ushort aArg0, ushort aArg1)
        {
            throw new NotImplementedException();
        }

        public virtual void Execute(ushort aArg0, ushort aArg1, ushort aArg2, ushort aArg3, ushort aArg4, ushort aArg5, ushort aArg6, ushort aArg7, ushort aArgCount)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Call a routine.
        /// Save the PC and FP.
        /// Load new PC and initialize a new stack frame.
        ///
        /// Note:
        /// The caller may provide more or less arguments than the routine has.
        /// </summary>
        /// <param name="aRoutine"></param>
        /// <param name="aArgs"></param>
        /// <param name="aCallType"></param>
        protected void Call(ushort aRoutine, List<ushort> aArgs, int aCallType)
        {
            ZDebug.Output($"PC = {Machine.Memory.PC}");
            ZDebug.Output($"BP = {Machine.Memory.Stack.BP}");
            ZDebug.Output($"SP = {Machine.Memory.Stack.SP}");

            long pc = Machine.Memory.PC;
            int i;

            int argc = aArgs.Count;
            
            Machine.Memory.Stack.AddFrame(aArgs.Count, aCallType);

            if (Machine.Story.Header.Version <= FileVersion.V3)
            {
                pc = (long)aRoutine << 1;
            }
            else if (Machine.Story.Header.Version <= FileVersion.V5)
            {
                pc = (long)aRoutine << 2;
            }

            Machine.Memory.PC = pc;

            Machine.Memory.CodeByte(out byte count);

            ushort value = 0;

            for (i = 0; i < count; i++)
            {
                // V1 to V4 games provide default values for all locals.
                if (Machine.Story.Header.Version <= FileVersion.V4)
                {
                    Machine.Memory.CodeWord(out value);
                }

                Machine.Memory.Stack.Push((argc-- > 0) ? aArgs[i] : value);
            }
        }

        /// <summary>
        /// Return from the current routine and restore the previous stack frame.
        /// The result may be stored (o), thrown away (1), or pushed on the stack (2).
        /// In the latter case a direct call has been finished and we must exit the interpreter loop.
        /// </summary>
        /// <param name="value"></param>
        protected void Return(ushort value)
        {
            ZDebug.Output($"PC = {Machine.Memory.PC}");
            ZDebug.Output($"BP = {Machine.Memory.Stack.BP}");
            ZDebug.Output($"SP = {Machine.Memory.Stack.SP}");

            int callType = Machine.Memory.Stack.RemoveFrame();

            if (callType == CallType.CallStore)
            {
                Store(value);
            }
        }

        protected void Branch(bool flag)
        {
            ZDebug.Output($"PC = {Machine.Memory.PC}");
            ZDebug.Output($"BP = {Machine.Memory.Stack.BP}");
            ZDebug.Output($"SP = {Machine.Memory.Stack.SP}");

            ushort offset;
            byte specifier;
            byte off1;
            byte off2;

            Machine.Memory.CodeByte(out specifier);

            off1 = (byte)(specifier & 0x3f);

            if (!flag)
            {
                specifier ^= 0x80;
            }

            if ((specifier & 0x40) == 0)
            {
                if ((off1 & 0x20) > 0)
                {
                    off1 |= 0xc0;
                }

                Machine.Memory.CodeByte(out off2);
                offset = (ushort)((off1 << 8) | off2);
                ZDebug.Output($"long branch: {offset}");
            }
            else
            {
                offset = off1;
                ZDebug.Output($"short branch: {offset}");
            }

            if ((specifier & 0x80) > 0)
            {
                if (offset > 1)
                {
                    long pc = Machine.Memory.PC;
                    pc += (short)offset - 2;
                    Machine.Memory.PC = pc;
                    ZDebug.Output($"normal branch: {pc}");
                }
                else
                {
                    ZDebug.Output($"special branch: {offset}");
                    Return(offset);
                }
            }
        }

        /// <summary>
        /// Store an operand, either as a variable or pushed on the stack.
        /// </summary>
        /// <param name="value"></param>
        protected void Store(ushort value)
        {
            ZDebug.Output($"PC = {Machine.Memory.PC}");
            ZDebug.Output($"BP = {Machine.Memory.Stack.BP}");
            ZDebug.Output($"SP = {Machine.Memory.Stack.SP}");

            byte variable;

            Machine.Memory.CodeByte(out variable);

            if (variable == 0)
            {
                Machine.Memory.Stack.Push(value);
                ZDebug.Output($"  Storing {value} on stack at {Machine.Memory.Stack.SP}");
            }
            else if (variable < 16)
            {
                Machine.Memory.Stack[Machine.Memory.Stack.BP - variable] = value;
                ZDebug.Output($"  Storing {value} on stack as Variable {variable} at {Machine.Memory.Stack.SP}");
            }
            else
            {
                ushort addr = (ushort)(Machine.Story.Header.GlobalsOffset + 2 * (variable - 16));
                Machine.Memory.SetWord(addr, value);
                ZDebug.Output($"  Storing {value} at {addr}");
            }
        }
    }
}

