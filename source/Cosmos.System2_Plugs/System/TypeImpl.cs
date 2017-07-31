using System;

using Cosmos.IL2CPU.API;

namespace Cosmos.System_Plugs.System
{
    [Plug(Target = typeof(Type))]
    public static class TypeImpl
    {
        public static void CCtor()
        {
        }

        public static Type GetTypeFromHandle(RuntimeTypeHandle aHandle)
        {
            return null;
        }

        public static string ToString(Type aThis)
        {
            return "<type>";
        }

        [PlugMethod(Signature = "System_Boolean__System_Type_op_Equality_System_Type__System_Type_")]
        public static bool op_Equality([ObjectPointerAccess]uint left, [ObjectPointerAccess]uint right)
        {
            // for now, type info is the type id.
            return left == right;
        }

        [PlugMethod(Signature = "System_Boolean__System_Type_op_Inequality_System_Type__System_Type_")]
        public static bool op_Inequality([ObjectPointerAccess]uint left, [ObjectPointerAccess]uint right)
        {
            return left != right;
        }
    }
}
