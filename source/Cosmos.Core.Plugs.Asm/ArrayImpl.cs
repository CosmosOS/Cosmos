﻿using System;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.Core.Plugs.Asm
{
    [Plug(Target = typeof(Array))]
    public class ArrayImpl
    {
        [PlugMethod(Assembler = typeof(ArrayGetLengthAsm))]
        public static int get_Length(Array aThis)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Assembler = typeof(ArrayInternalCopyAsm))]
        public static void Copy(Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length, bool reliable)
        {
            throw new NotImplementedException();
        }
    }
}
