using System.Collections.Generic;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Cosmos.VS.DebugEngine.AD7.Impl
{
    public class DebugLocalInfo
    {
        public bool IsLocal
        {
            get;
            set;
        }

        public bool IsReference
        {
            get;
            set;
        }

        public string Type
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public int Offset
        {
            get;
            set;
        }

        public uint Pointer
        {
            get;
            set;
        }

        public int Index
        {
            get;
            set;
        }

        private List<IDebugProperty2> m_children = new List<IDebugProperty2>();
        public List<IDebugProperty2> Children
        {
            get { return m_children; }
            set { m_children = value; }
        }
    }
}