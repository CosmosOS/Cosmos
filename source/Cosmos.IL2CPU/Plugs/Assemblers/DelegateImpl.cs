using System;

namespace Cosmos.IL2CPU.Plugs.Assemblers {
  [Plug(Target=typeof(Delegate), Inheritable=true)]
  [PlugField(FieldType = typeof(int), FieldId = "$$ArgSize$$")]
  public static class DelegateImpl {

    [PlugMethod(Assembler = typeof(CtorImplAssembler), IsWildcard=true, WildcardMatchParameters=true)]
    public static void Ctor(Delegate aThis, object aTarget, IntPtr aMethod) {
      //aFldTarget = aObject;
      //aFldMethod = aMethod;
      throw new NotImplementedException("Implemented by method assembler");
    }

    [PlugMethod(IsWildcard=true, Assembler=typeof(InvokeImplAssembler))]
    public static void Invoke() {
      throw new NotImplementedException("Implemented by method assembler");
    }
  }
}