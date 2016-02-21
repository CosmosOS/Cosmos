#define COSMOSDEBUG

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cosmos.Common;
using Cosmos.Debug.Kernel;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.Core.Plugs.System.Globalization
{
    [Plug(Target = typeof(CompareInfo))]
    public static class CompareInfoImpl
    {
        static Debugger mDebugger = new Debugger("Core", "Compare Info Plug");

        public static void Ctor(CompareInfo aThis, CultureInfo culture)
        {
            mDebugger.SendInternal("CompareInfo::Ctor");
        }

        public static int Compare(CompareInfo aThis, string aString1, string aString2, CompareOptions aOptions)
        {
#warning TODO: Implement CompareOptions
            mDebugger.SendInternal("CompareInfo.Compare");
            return StringHelper.Compare(aString1, 0, aString2, 0, aString1.Length, aString2.Length);
        }
    }
}
