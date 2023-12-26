using System;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System
{
    [Plug(Target = typeof(MulticastDelegate))]
    public class MulticastDelegateImpl
    {
        public static bool Equals(MulticastDelegate aThis, object aThat)
        {
            // todo: implement MulticastDelegate.Equals(MulticastDelegate)
            return false;
        }
    }
}