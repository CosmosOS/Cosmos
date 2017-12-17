using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using IL2CPU.API;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Globalization
{
    [Plug(Target = typeof(CompareInfo))]
    public static class CompareInfoImpl
    {
        public static void Ctor(CompareInfo aThis, CultureInfo aCulture)
        {
        }
    }

}
