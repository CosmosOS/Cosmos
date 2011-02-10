using System;
using System.Collections.Generic;
using System.Linq;

namespace Cosmos.Debug.VSDebugEngine
{
    public class DebugLocalInfo
    {
        public bool IsLocal
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
    }
}