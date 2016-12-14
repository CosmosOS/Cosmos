using System;
using Cosmos.IL2CPU.Plugs;
using Cosmos.IL2CPU.Plugs.Assemblers;
using Cosmos.IL2CPU.Plugs.Assemblers.Delegate;

namespace Cosmos.Core.Plugs.Asm
{
    [Plug(Target = typeof(Delegate), Inheritable = true)]
    [PlugField(FieldType = typeof(int), FieldId = "$$ArgSize$$")]
    [PlugField(FieldType = typeof(int), FieldId = "$$ReturnsValue$$")]
    public static class DelegateImpl
    {
        [PlugMethod(Assembler = typeof(DelegateCtorAsm), IsWildcard = true, WildcardMatchParameters = true)]
        public static void Ctor(Delegate aThis, object aTarget, IntPtr aMethod)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(IsWildcard = true, Assembler = typeof(DelegateInvokeAsm))]
        public static void Invoke()
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Assembler = typeof(DelegateGetMulticastInvokeAsm))]
        public static IntPtr GetMulticastInvoke(Delegate aThis)
        {
            throw new NotImplementedException();
        }

        public static bool Equals(Delegate aThis, object aThat)
        {
            throw new NotImplementedException();
        }
    }
}
