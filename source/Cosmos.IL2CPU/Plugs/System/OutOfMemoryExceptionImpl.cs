using Cosmos.IL2CPU.Plugs;
using System;

namespace Cosmos.IL2CPU.X86.Plugs.System
{
    [Plug(Target = typeof(OutOfMemoryException))]
    public static class OutOfMemoryExceptionImpl
    {
        public static void Ctor(OutOfMemoryException aThis)
        {
            //
        }
    }
}