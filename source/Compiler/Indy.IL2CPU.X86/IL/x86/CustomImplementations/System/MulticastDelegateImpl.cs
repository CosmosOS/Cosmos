using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.IL.X86.CustomImplementations.System {
    [Plug(Target=typeof(MulticastDelegate))]
    [PlugField(FieldType=typeof(int), FieldId="$$ArgSize$$")]
	public  class MulticastDelegateImpl {
        [PlugMethod(Assembler=typeof(Assemblers.MulticastDelegate_Invoke))]
        public static void InvokeMulticast(MulticastDelegate aThis)
        {
            throw new Exception("This method should be implemented using a MethodAssembler");
        }

        public static bool Equals(MulticastDelegate aThis, object aThat) {
            // todo: implement MulticastDelegate.Equals(MulticastDelegate)
            return false;
        }
	}
}
