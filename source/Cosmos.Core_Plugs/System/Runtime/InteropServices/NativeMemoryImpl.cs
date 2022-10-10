using IL2CPU.API.Attribs;
using Cosmos.Core.Memory;
using Cosmos.Core;

namespace System.Runtime.InteropServices
{
    [Plug("System.Runtime.InteropServices.NativeMemory, System.Private.CoreLib")]
    public static unsafe class NativeMemory
	{
        public static void* Realloc(void* ptr, nuint byteCount)
		{
            return Heap.Realloc(ptr, byteCount);
		}

        public static void* Alloc(nuint elementCount, nuint elementSize)
		{
            return Heap.Alloc((uint)(elementCount * elementSize));
		}
        public static void* Alloc(nuint byteCount)
		{
            return Heap.Alloc((uint)byteCount);
		}

        public static void Free(void* ptr)
		{
            Heap.Free(ptr);
		}
	}
}
