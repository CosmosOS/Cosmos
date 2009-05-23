using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Indy.IL2CPU.Compiler
{
    public class QueuedMethodInformation
    {
        public bool Processed;
        public bool PreProcessed;
        public int Index;
        public MLDebugSymbol[] Instructions;
        public readonly SortedList<string, object> Info = new SortedList<string, object>(StringComparer.InvariantCultureIgnoreCase);
        public MethodBase Implementation;
        public string FullName;
    }
}