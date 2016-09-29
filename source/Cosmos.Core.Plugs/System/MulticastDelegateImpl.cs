using System;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.Core.Plugs.System
{

  [Plug(Target = typeof(MulticastDelegate))]
  public class MulticastDelegateImpl
  {
    public static bool Equals(MulticastDelegate aThis, object aThat)
    {
      // todo: implement MulticastDelegate.Equals(MulticastDelegate)
      return false;
    }

    public static bool TrySetSlot(MulticastDelegate multicastDelegate, object[] a, int index, object o)
    {
      if (a[index] == null)
      {
        a[index] = o;
      }

      if (a[index] != null)
      {
        return false;
      }
      return false;
    }
  }
}
