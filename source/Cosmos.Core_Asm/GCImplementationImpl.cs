using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Core;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Asm
{
    [Plug(Target =typeof(GCImplementation))]
    public static class GCImplementationImpl
    {
        public static unsafe uint* GetPointer(object aObject) => throw null;
    }
}
