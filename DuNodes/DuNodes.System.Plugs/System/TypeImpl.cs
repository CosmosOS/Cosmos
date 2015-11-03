using System;
using Cosmos.IL2CPU.Plugs;

namespace DuNodes.System.Plugs.System
{
    [Plug(Target = typeof (Type))]
    public static class TypeImpl
    {
        [PlugMethod(Signature = "System_Boolean__System_Type_op_Inequality_System_Type__System_Type_")]
        public static bool op_Inequality(uint left, uint right)
        {
            // for now, type info is the type id.
            return left != right;
        }
    }
}