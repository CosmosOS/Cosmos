using System;
using System.Globalization;
using System.Reflection;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Reflection
{
    [Plug("System.Reflection.RuntimeAssembly, System.Private.CoreLib")]
    class RuntimeAssemblyImpl
    {
        [PlugMethod(Signature = "System_Void__System_Reflection_RuntimeAssembly_GetFullName_System_Runtime_CompilerServices_QCallAssembly__System_Runtime_CompilerServices_StringHandleOnStack_")]
        public static void GetFullName(object aQCallAssembly, object aStringHandleOnStack)
        {
            throw new NotImplementedException();
        }

        public static object[] GetCustomAttributes(object aThis, Type aType, bool aBool)
        {
            throw new NotImplementedException();
        }
        
        [PlugMethod(Signature = "System_Void__System_Reflection_RuntimeAssembly__GetTypeCoreIgnoreCase_System_Runtime_CompilerServices_QCallAssembly__System_UInt16#__System_IntPtr#__System_Int32__System_Runtime_CompilerServices_ObjectHandleOnStack_")]
        public static void GetTypeCoreIgnoreCase(object assembly, string typeName, object nestedTypeNames, int nestedTypeNamesLength, object retType)
        {
            throw new NotImplementedException();
        }
                
        [PlugMethod(Signature = "System_Void__System_Reflection_RuntimeAssembly__GetTypeCore_System_Runtime_CompilerServices_QCallAssembly__System_UInt16#__System_IntPtr#__System_Int32__System_Runtime_CompilerServices_ObjectHandleOnStack_")]
        public static void GetTypeCore(object assembly, string typeName, object nestedTypeNames, int nestedTypeNamesLength, object retType)
        {
            throw new NotImplementedException();
        }
                        
        [PlugMethod(Signature = "System_Void__System_Reflection_RuntimeAssembly__InternalLoad_System_Reflection_NativeAssemblyNameParts#__System_Runtime_CompilerServices_ObjectHandleOnStack__System_Runtime_CompilerServices_StackCrawlMarkHandle__System_Int32__System_Runtime_CompilerServices_ObjectHandleOnStack__System_Runtime_CompilerServices_ObjectHandleOnStack_")]
        public static void InternalLoad(object nameParts, object requestingAssembly, object stackMark, bool throwOnFileNotFound, object assemblyLoadContext, object retAssembly)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Boolean__System_Reflection_RuntimeAssembly_FCallIsDynamic_System_Reflection_RuntimeAssembly_")]
        public static bool FCallIsDynamic(object assembly)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Void__System_Reflection_RuntimeAssembly_GetLocation_System_Runtime_CompilerServices_QCallAssembly__System_Runtime_CompilerServices_StringHandleOnStack_")]
        public static void GetLocation(object assembly, object stringHandleOnStack)
        {
            throw new NotImplementedException();
        }

        public static string GetSimpleName(object aThis)
        {
            return "Reflection is not supported";
        }

        public static CultureInfo GetLocale(object aThis)
        {
            throw new NotImplementedException();
        }

        public static byte[] GetPublicKey(object aThis)
        {
            throw new NotImplementedException();
        }

        public static AssemblyNameFlags GetFlags(object aThis)
        {
            throw new NotImplementedException();
        }

        public static global::System.Configuration.Assemblies.AssemblyHashAlgorithm GetHashAlgorithm(object aThis)
        {
            throw new NotImplementedException();
        }

        public static Version GetVersion(object aThis)
        {
            throw new NotImplementedException();
        }

        public static string GetCodeBase(object aThis)
        {
            return "Reflection is not supported in Cosmos";
        }

        public static Module get_ManifestModule(object aThis)
        {
            throw new NotImplementedException();
        }
    }
}
