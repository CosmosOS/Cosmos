using System;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.IL2CPU.CustomImplementation.System
{
  [Plug(Target = typeof(UIntPtr))]
  public static class UIntPtrImpl
  {
    //  //[PlugMethod(Signature="System_String___System_IntPtr_ToString____")]
    public static string ToString(UIntPtr aThis)
    {
      return "<UIntPtr>";
    }
    //}
  }
}
