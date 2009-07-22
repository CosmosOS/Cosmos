using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Cosmos.Compiler.IL;

namespace Cosmos.Compiler
{
    partial class Scanner
    {
        private void InitDebug()
        {
#if DEBUG
            mOpNeedsLogging = new Dictionary<OpCodeEnum, bool>();
            foreach(OpCodeEnum xOp in Enum.GetValues(typeof(OpCodeEnum)))
            {
                mOpNeedsLogging.Add(xOp, true);
            }
#endif
        }
#if DEBUG
        private Dictionary<OpCodeEnum, bool> mOpNeedsLogging;
#endif
        [Conditional("DEBUG")]
        private void LogMissingOp(OpCodeEnum aOpCode)
        {
#if DEBUG
            if(mOpNeedsLogging[aOpCode])
            {
                Console.WriteLine("Needs {0} op", aOpCode);
                mOpNeedsLogging[aOpCode] = false;
            }
#endif
        }
    }
}
