using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System.IO
{
    [Plug(TargetName = "System.IO.__Error")]
    public static class ErrorImpl
    {
        public static void WinIOError(int errorCode, string maybeFullPath)
        {
            throw new Exception("IO error.");
        }
    }
}
