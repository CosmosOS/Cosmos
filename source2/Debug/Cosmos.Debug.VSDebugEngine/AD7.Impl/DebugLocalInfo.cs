using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Cosmos.Debug.VSDebugEngine
{
    public class DebugLocalInfo
    {
        public bool IsLocal
        {
            get;
            set;
        }

        public bool IsArrayElement
        {
            get;
            set;
        }

        public string ArrayElementType
        {
            get;
            set;
        }

        public int ArrayElementLocation
        {
            get;
            set;
        }

        public string Name
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