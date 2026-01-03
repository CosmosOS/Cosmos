using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Reflection
{
    [Plug(typeof(global::System.Reflection.Module))]
    class ModuleImpl
    {
        public static string ToString(global::System.Reflection.Module aThis)
        {
            return "";
        }
    }
}
