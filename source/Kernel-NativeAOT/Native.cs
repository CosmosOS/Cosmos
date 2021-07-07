using System;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Internal.Runtime.CompilerHelpers;

namespace Kernel
{
    public static class Native
    {
        [DllImport("*")]
        public static extern void Out8(ushort port, byte value);

        [DllImport("*")]
        public static extern void Out16(ushort port, ushort value);

        [DllImport("*")]
        public static extern void Out32(ushort port, uint value);

        [DllImport("*")]
        public static extern byte In8(ushort port);

        [DllImport("*")]
        public static extern ushort In16(ushort port);

        [DllImport("*")]
        public static extern uint In32(ushort port);
    }

    /*
    public static unsafe class Memory
    {
        private static long _heapBase;

        public static void Init(long heapBase)
        {
            _heapBase = heapBase;
        }

        public static T* Alloc<T>() where T : unmanaged
        {
            return (T*)Alloc(sizeof(T));
        }

        public static IntPtr Alloc(long size)
        {
            var pos = (IntPtr)_heapBase;
            _heapBase = _heapBase + size;
            return pos;
        }

        public static IntPtr Alloc(int size)
        {
            var pos = (IntPtr)_heapBase;
            _heapBase = _heapBase + size;
            return pos;
        }

        public static unsafe void Zero(IntPtr ptr, ulong len)
        {
            var count = len / 8;
            var rem = len % 8;

            for (var i = 0U; i < count; i++)
                ((ulong*)ptr)[i] = 0;

            for (var i = 0U; i < rem; i++)
                ((byte*)ptr)[count + i] = 0;
        }

        public static unsafe void Copy(IntPtr dst, IntPtr src, ulong len)
        {
            var count = len / 8;
            var rem = len % 8;

            for (var i = 0U; i < count; i++)
                ((ulong*)dst)[i] = ((ulong*)src)[i];

            for (var i = 0U; i < rem; i++)
                ((byte*)dst)[count + i] = ((byte*)src)[count + i];
        }
    }*/
}
