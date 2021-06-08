using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Reflection
{
    [Plug(Target =typeof(Assembly))]
    class AssemblyImpl
    {
        public static object[] GetCustomAttributes(Assembly aThis, Type aType, bool aBool)
        {
            throw new NotImplementedException();
        }
    }
}
