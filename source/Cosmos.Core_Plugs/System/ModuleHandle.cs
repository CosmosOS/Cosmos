using System;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System
{
    [Plug(Target = typeof(ModuleHandle))]
    public static class ModuleHandleImpl
    {
        [PlugMethod(Signature = "System_IntPtr__System_ModuleHandle__GetMetadataImport_System_Reflection_RuntimeModule_")]
        public static IntPtr _GetMetadataImport(object aRuntimeType)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Void__System_ModuleHandle_ResolveType_System_Runtime_CompilerServices_QCallModule__System_Int32__System_IntPtr#__System_Int32__System_IntPtr#__System_Int32__System_Runtime_CompilerServices_ObjectHandleOnStack_")]
        public static unsafe IntPtr ResolveType(object module,
            int typeToken,
            IntPtr* typeInstArgs,
            int typeInstCount,
            IntPtr* methodInstArgs,
            int methodInstCount,
            object type)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_RuntimeMethodHandleInternal__System_ModuleHandle_ResolveMethod_System_Runtime_CompilerServices_QCallModule__System_Int32__System_IntPtr#__System_Int32__System_IntPtr#__System_Int32_")]
        public static unsafe object ResolveMethod(object module,
            int methodToken,
            IntPtr* typeInstArgs,
            int typeInstCount,
            IntPtr* methodInstArgs,
            int methodInstCount)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Int32__System_ModuleHandle_GetMDStreamVersion_System_Reflection_RuntimeModule_")]
        public static int get_MDStreamVersion(object aThis)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Void__System_ModuleHandle_GetPEKind_System_Runtime_CompilerServices_QCallModule__System_Int32#__System_Int32#_")]
        public static void GetPEKind(object aThis, out int kind, out int machine)
        {
            kind = (int)global::System.Reflection.PortableExecutableKinds.NotAPortableExecutableImage;
            machine = (int)global::System.Reflection.ImageFileMachine.I386;
        }
    }
}