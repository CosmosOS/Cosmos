using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.IL2CPU.X86.Plugs.CustomImplementations.System {

  [Plug(Target = typeof(MulticastDelegate))]
//  [PlugField(FieldType = typeof(int), FieldId = "$$ArgSize$$")]
  public class MulticastDelegateImpl {

    // there is no such thing as MulticastDelegate.InvokeMulticase. New scanner is more strict
    // so I commented this out for now.
    //[PlugMethod(Assembler = typeof(Assemblers.MulticastDelegate_Invoke, IsWildcard=true))]
    //public static void InvokeMulticast(MulticastDelegate aThis) {
    //  throw new Exception("This method should be implemented using a MethodAssembler");
    //}

    public static bool Equals(MulticastDelegate aThis, object aThat) {
      // todo: implement MulticastDelegate.Equals(MulticastDelegate)
      return false;
    }
  }
}
