using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Cosmos.Kernel;

namespace Cosmos.Hardware2 {
    //TODO: Move these back to be nested classes of IRQ when kernel is fully imported

    [StructLayout(LayoutKind.Explicit, Size = 512)]
    public struct MMXContext {
    }

    [StructLayout(LayoutKind.Explicit, Size = 80)]
    public struct IRQContext {
        [FieldOffset(0)]
        public unsafe MMXContext* MMXContext;

        [FieldOffset(4)]
        public uint EDI;

        [FieldOffset(8)]
        public uint ESI;

        [FieldOffset(12)]
        public uint EBP;

        [FieldOffset(16)]
        public uint ESP;

        [FieldOffset(20)]
        public uint EBX;

        [FieldOffset(24)]
        public uint EDX;

        [FieldOffset(28)]
        public uint ECX;

        [FieldOffset(32)]
        public uint EAX;

        [FieldOffset(36)]
        public uint Interrupt;

        [FieldOffset(40)]
        public uint Param;

        [FieldOffset(44)]
        public uint EIP;

        [FieldOffset(48)]
        public uint CS;

        [FieldOffset(52)]
        public EFlagsEnum EFlags;

        [FieldOffset(56)]
        public uint UserESP;
    }
}
