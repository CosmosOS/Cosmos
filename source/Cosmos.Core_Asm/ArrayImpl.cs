#define COSMOSDEBUG
using System;
using Cosmos.Debug.Kernel;
using IL2CPU.API.Attribs;
using Cosmos.Core;

namespace Cosmos.Core_Asm
{
    [Plug(Target = typeof(Array))]
    public unsafe class ArrayImpl
    {
        [PlugMethod(Assembler = typeof(ArrayGetLengthAsm))]
        public static int get_Length(Array aThis)
        {
            throw new NotImplementedException();
        }

        public static void Copy(Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length)
        {
            Copy(sourceArray, sourceIndex, destinationArray, destinationIndex, length, false);
        }

        public static void Copy(Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length, bool reliable)
        {
            fixed (byte* sourceArrayPtr = sourceArray, destinationArrayPtr = destinationArray)
            {
                MemoryOperations.Copy(sourceArrayPtr + sourceIndex, destinationArray + destinationIndex, length);
            }
        }

        [PlugMethod(Assembler = typeof(ArrayClearAsm))]
        public static void Clear(Array array)
        {
            throw new NotImplementedException();
        }
    }
}
