using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.System
{
    public class LockSystem
    {
        public static Dictionary<string, Lock> definedLocks = new Dictionary<string, Lock>();
        public class Lock
        {
            internal Lock() { }
            public Core.Processing.ProcessContext.Context LockContext = null;
            public List<Core.Processing.ProcessContext.Context> queue = new List<Core.Processing.ProcessContext.Context>();
        }
        public static Lock DefineLock(string LockName)
        {
            definedLocks.Add(LockName, new Lock());
            return definedLocks[LockName];
        }
    }
}
