using System;
using Cosmos.IL2CPU;
using Cosmos.IL2CPU.Plugs;
using Cosmos.IL2CPU.Plugs.Assemblers;
using Cosmos.IL2CPU.Plugs.Assemblers.Delegate;
using XSharp.Compiler;

namespace Cosmos.Core.Plugs.System
{
    [Plug(Target = typeof(Delegate), Inheritable = true)]
    [PlugField(FieldType = typeof(int), FieldId = "$$ArgSize$$")]
    [PlugField(FieldType = typeof(int), FieldId = "$$ReturnsValue$$")]
    public static class DelegateImpl
    {
        [PlugMethod(Assembler = typeof(DelegateCtorAsm), IsWildcard = true, WildcardMatchParameters = true)]
        public static void Ctor(Delegate aThis, object aTarget, IntPtr aMethod)
        {
            throw new NotImplementedException("Implemented by method assembler");
        }

        [PlugMethod(IsWildcard = true, Assembler = typeof(DelegateInvokeAsm))]
        public static void Invoke()
        {
            throw new NotImplementedException("Implemented by method assembler");
        }

        [PlugMethod(Assembler = typeof(DelegateGetMulticastInvokeAsm))]
        public static IntPtr GetMulticastInvoke(Delegate aThis)
        {
            return IntPtr.Zero;
        }

        public static bool Equals(Delegate aThis, object aThat)
        {
            // todo: implement proper Delegate.Equals(object)
            return false;
        }

        public static unsafe bool InternalEqualTypes([ObjectPointerAccess] uint** a, [ObjectPointerAccess] uint** b)
        {
            var xTypeA = a[0][0];
            var xTypeB = b[0][0];

            return xTypeA == xTypeB;
        }

        [PlugMethod(Signature = "System_MulticastDelegate__System_Delegate_InternalAllocLike_System_Delegate_")]
        public static unsafe uint InternalAllocLike(uint* aDelegate)
        {
            uint xNeededSize = 1024; // 24 is needed fields for Multicast Delegate
            xNeededSize += 12;
            uint xResultAddr = GCImplementation.AllocNewObject(xNeededSize);
            byte* xResult = (byte*) xResultAddr;
            byte* xDelegateAsByte = (byte*) aDelegate;
            for (int i = 0; i < 1024; i++)
            {
                xResult[i] = xDelegateAsByte[i];
            }
            return xResultAddr;
        }
    }
}
