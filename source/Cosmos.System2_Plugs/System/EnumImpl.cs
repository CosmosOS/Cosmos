using System.Runtime.CompilerServices;
using Cosmos.Core;
using Cosmos.Debug.Kernel;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System
{
    [Plug(Target = typeof(Enum))]
    public static class EnumImpl
    {
        //		[PlugMethod(Signature = "System_Void__System_Enum__cctor__")]
        public static void Cctor()
        {
            //
        }

        public static bool Equals(Enum aThis, object aEquals)
        {
            throw new NotSupportedException("Enum.Equals not supported yet!");
        }

        public static string ToString(Enum aThis) {
            unsafe {
                Debugger debugger = new("enum.tostring");

                byte* addr = (byte*)&aThis;
                IntPtr ptr = new IntPtr(addr);

                for(long i = ptr.ToInt64() - 32; i < ptr.ToInt64() + 32; i++) {
                    debugger.Send((*((byte*)(new IntPtr(i)))).ToString() + " << Content of offset " + (i - ptr.ToInt64()).ToString() + "to aThis");
                }

                //var vtableTypeId = (uint)VTablesImpl.GetType(aThis.GetType().Name);
                //var value = (uint)(object)aThis;
                var value = (uint)VTablesImpl.GetType(aThis.GetType().Name);
                var vtableTypeId = (uint)(object)aThis;

                debugger.Send("=== Enum.ToString ===");
                debugger.Send("VTableId: " + vtableTypeId.ToString());
                debugger.Send("VTableId Expected: " + VTablesImpl.GetType("TestEnum"));

                debugger.Send("GetType().Name: " + aThis.GetType().Name);
                debugger.Send("GetType().AssemblyQualifiedName: " + aThis.GetType().AssemblyQualifiedName);
                debugger.Send("VTable.GetName: " + VTablesImpl.GetName(vtableTypeId));

                debugger.Send("Value: " + value.ToString());

                return VTablesImpl.GetEnumValueString(vtableTypeId, value);
            }
        }

        public static string ToString(Enum aThis, string format) => aThis.ToString();

        public static int GetHashCode(Enum aThis)
        {
            throw new NotImplementedException("Enum.GetHashCode()");
        }
        [PlugMethod(Signature = "System_RuntimeType__System_Enum_InternalGetUnderlyingType_System_RuntimeType_")]
        public static object InternalGetUnderlyingType(object aRuntimeType)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Reflection_CorElementType__System_Enum_InternalGetCorElementType__")]
        public static object InternalGetCorElementType(object aThis)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Object__System_Enum_InternalBoxEnum_System_RuntimeType__System_Int64_")]
        public static object InternalBoxEnum(object aRuntimeType, long aLong)
        {
            throw new NotImplementedException();
        }
        [PlugMethod(Signature = "System_Void__System_Enum_GetEnumValuesAndNames_System_Runtime_CompilerServices_QCallTypeHandle__System_Runtime_CompilerServices_ObjectHandleOnStack__System_Runtime_CompilerServices_ObjectHandleOnStack__Interop_BOOL_")]
        public static void GetEnumValuesAndNames(object aQCallTypeHandle, object aObjectHandleOnStack, object aObjectHandleOnStack1, bool aBool)
        {
            throw new NotImplementedException();
        }
    }
}