﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using System.Threading;

namespace Cosmos.IL2CPU.IL.CustomImplementations.System.Threading
{
    [Plug(Target=typeof(Interlocked))]
    public static class InterlockedImpl
    {
        public static int Decrement(ref int aData) {
            return aData -= 1;
        }
    }
}
