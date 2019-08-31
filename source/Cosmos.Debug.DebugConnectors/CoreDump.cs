using System;
using System.Collections.Generic;

namespace Cosmos.Debug.DebugConnectors
{
    public class CoreDump
    {
        public uint EAX { get; }
        public uint EBX { get; }
        public uint ECX { get; }
        public uint EDX { get; }

        public uint ESI { get; }
        public uint EDI { get; }

        public uint EBP { get; }
        public uint ESP { get; }
        public uint EIP { get; }

        public Stack<uint> StackTrace { get; }

        public CoreDump(
            uint eax, uint ebx, uint ecx, uint edx,
            uint esi, uint edi,
            uint ebp, uint esp, uint eip,
            Stack<uint> stackTrace)
        {
            EAX = eax;
            EBX = ebx;
            ECX = ecx;
            EDX = edx;

            ESI = esi;
            EDI = edi;

            EBP = ebp;
            ESP = esp;
            EIP = eip;

            StackTrace = stackTrace;
        }

        internal static CoreDump FromStackArray(byte[] stackBytes)
        {
            var stack = new Stack<uint>(stackBytes.Length / 4);

            for (int i = 0; i < stackBytes.Length; i += 4)
            {
                stack.Push(BitConverter.ToUInt32(stackBytes, i));
            }

            return new CoreDump(
                stack.Pop(), stack.Pop(), stack.Pop(), stack.Pop(),
                stack.Pop(), stack.Pop(),
                stack.Pop(), stack.Pop(), stack.Pop(),
                stack);
        }
    }
}
