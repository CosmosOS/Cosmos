using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System.Security
{
    [Plug(Target = typeof(global::System.Security.SecurityElement))]
    public static class SecurityElementImpl
    {
        public static string ToString(global::System.Security.SecurityElement aThis)
        {
            return "";
        }
    }

}
