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
    [Plug(Target = typeof(TextInfo))]
    public static class TextInfoImpl
    {
        static Debugger mDebugger = new Debugger("Core", "Compare Info Plug");

        public static void Ctor(TextInfo aThis, object cultureData)
        {
        }

        public static int InternalCompareStringOrdinalIgnoreCase(string aString1, int aIndex1, string aString2, int aIndex2, int aLength1, int aLength2)
        {
            mDebugger.SendInternal("InternalCompareStringOrdinalIgnoreCase");
            mDebugger.SendInternal(aString1);
            mDebugger.SendInternal(aString2);

            string xString1 = aString1.ToLower();
            string xString2 = aString2.ToLower();
            return StringHelper.Compare(xString1, aIndex1, xString2, aIndex2, aLength1, aLength2);
        }
    }
}
