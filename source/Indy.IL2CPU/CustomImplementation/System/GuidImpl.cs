using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;
namespace Indy.IL2CPU.CustomImplementation
{

    [Plug(Target = typeof(Guid))]
    public static class GuidImpl
    {
        [PlugMethod(Signature="System_Void__System_Guid__ctor_System_String_")]
        public static void ctor(String aStr){  }
    }
}