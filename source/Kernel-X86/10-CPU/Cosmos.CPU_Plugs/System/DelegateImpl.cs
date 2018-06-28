using System;

using IL2CPU.API;
using IL2CPU.API.Attribs;

namespace Cosmos.CPU_Plugs.System {
    [Plug(Target = typeof(Delegate), Inheritable = true)]
    [PlugField(FieldType = typeof(int), FieldId = "$$ArgSize$$")]
    [PlugField(FieldType = typeof(int), FieldId = "$$ReturnsValue$$")]
    public static class DelegateImpl {
        public static bool Equals(Delegate aThis, object aThat) {
            // todo: implement proper Delegate.Equals(object)
            return false;
        }

        public static unsafe bool InternalEqualTypes([ObjectPointerAccess] uint** a, [ObjectPointerAccess] uint** b) {
            var xTypeA = a[0][0];
            var xTypeB = b[0][0];

            return xTypeA == xTypeB;
        }

        [PlugMethod(Signature = "System_MulticastDelegate__System_Delegate_InternalAllocLike_System_Delegate_")]
        public static unsafe uint InternalAllocLike(uint* aDelegate) {
            uint xNeededSize = 1024; // 24 is needed fields for Multicast Delegate
            xNeededSize += 12;
            uint xResultAddr = GCImplementationImpl.AllocNewObject(xNeededSize);
            byte* xResult = (byte*)xResultAddr;
            byte* xDelegateAsByte = (byte*)aDelegate;
            for (int i = 0; i < 1024; i++) {
                xResult[i] = xDelegateAsByte[i];
            }
            return xResultAddr;
        }
    }
}
