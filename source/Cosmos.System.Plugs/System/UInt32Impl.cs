﻿using System;

using Cosmos.Common;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System
{
    [Plug(Target = typeof(UInt32))]
    public static class UInt32Impl
    {
        public static string ToString(ref uint aThis)
        {
            return StringHelper.GetNumberString(aThis);
        }
    }
}
