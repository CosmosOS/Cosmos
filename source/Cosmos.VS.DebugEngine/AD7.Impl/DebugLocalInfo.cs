using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Cosmos.VS.DebugEngine.AD7.Impl
{
    public class DebugLocalInfo
    {
        public bool IsLocal { get; set; }

        public bool IsReference { get; set; }

        public string Type { get; set; }

        public string Name { get; set; }

        public int Offset { get; set; }

        [SuppressMessage("Naming", "CA1720:Identifier contains type name", Scope = "member")]
        public uint Pointer { get; set; }

        public int Index { get; set; }

        public List<IDebugProperty2> Children { get; } = new List<IDebugProperty2>();
    }
}
