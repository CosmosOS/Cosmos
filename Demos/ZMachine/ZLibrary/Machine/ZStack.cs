using System;

namespace ZLibrary.Machine
{
    public class ZStack
    {
        private ushort[] _data;

        private ZMemory _memory;

        public ushort SP { get; private set; }

        public ushort BP { get; private set; }

        public ushort Frames { get; private set; }

        public ZStack(ZMemory aMemory)
        {
            _memory = aMemory;
            _data = new ushort[32768];
            SP = (ushort)(_data.Length);
            BP = SP;
            Frames = 0;
        }

        public ushort this[int index]
        {
            get
            {
                ZDebug.Output($"{index} -> {_data[index].ToString()}");
                return _data[index];
            }
            set
            {
                ZDebug.Output($"{index} <- {value}");
                _data[index] = value;
            }
        }

        public void AddFrame(int aArgCount, int aCallType)
        {
            long pc = _memory.PC;

            Push((ushort)(pc >> 9));
            Push((ushort)(pc & 0x1ff));
            Push((ushort)(BP - 1));
            Push((ushort)(aArgCount | (aCallType << 8)));

            BP = SP;
            Frames++;

            ZDebug.Output($"Added Frame: {Frames} -> {this[SP + 0]}:{this[SP + 1]}:{this[SP + 2]}:{this[SP + 3]}");
        }

        public int RemoveFrame()
        {
            ZDebug.Output($"Removing Frame: {Frames}");

            SP = BP;
            Frames--;

            int callType = Pop();
            callType = callType >> 8;

            BP = Pop();
            BP++;

            long lowPC = Pop();
            long highPC = Pop();
            highPC = highPC << 9;
            long pc = highPC | lowPC;
            _memory.PC = pc;
            if (_memory.PC < _memory.StartPC)
            {
                throw new Exception("Program counter is less than the start location!");
            }
            return callType;
        }

        public void Push(ushort aValue)
        {
            SP--;
            this[SP] = aValue;
        }

        public ushort Pop()
        {
            ushort xValue = this[SP];
            SP++;
            return xValue;
        }
    }
}
