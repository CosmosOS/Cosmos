using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.X86.Plugs.NEW_PLUGS {
  [Plug(Target=typeof(Delegate), Inheritable=true)]
  [PlugField(FieldType = typeof(int), FieldId = "$$ArgSize$$")]
  public static class DelegateImpl {
    public static void Ctor(Delegate aThis, object aObject, IntPtr aMethod, [FieldAccess(Name = "System.Object System.Delegate._target")] ref object aFldTarget, [FieldAccess(Name = "System.IntPtr System.Delegate._methodPtr")] ref IntPtr aFldMethod) {
      aFldTarget = aObject;
      aFldMethod = aMethod;
    }

    [PlugMethod(IsWildcard=true, Assembler=typeof(InvokeImplAssembler))]
    public static void Invoke() {
      throw new NotImplementedException("Implemented by method assembler");
    }
    //System.Void  Cosmos.Hardware.HandleKeyboardDelegate.Invoke(System.Byte, System.Boolean)
  }
}