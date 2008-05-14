using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;
using Microsoft.Win32;

namespace Indy.IL2CPU.CustomImplementation.Microsoft.Win32
{
    [Plug(Target=typeof(RegistryKey))]
    public static class RegistryKeyImpl
    {
        [PlugMethod(Signature="System_Void__Microsoft_Win32_RegistryKey__cctor__")]
        public static void CCtor() { }
    }
}
