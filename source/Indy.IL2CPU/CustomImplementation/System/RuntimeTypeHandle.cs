using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.CustomImplementation.System
{
    [Plug(Target = typeof(global::System.RuntimeTypeHandle), IsMicrosoftdotNETOnly = true)]
    public static class RuntimeTypeHandle
    {
        [PlugMethod(Signature = "System_Void__System_RuntimeTypeHandle__ctor_System_Void__")]
        public static void CTor(uint aValue)
        {
            
        }
    }
}