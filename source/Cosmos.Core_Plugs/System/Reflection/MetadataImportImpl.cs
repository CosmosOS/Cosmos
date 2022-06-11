using System;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Reflection;

[Plug("System.Reflection.MetadataImport, System.Private.CoreLib")]
internal class MetadataImportImpl
{
    [PlugMethod(Signature =
        "System_Void__System_Reflection_MetadataImport__GetGenericParamProps_System_IntPtr__System_Int32___System_Int32_")]
    public static unsafe void __GetGenericParamProps(IntPtr aPtr1, int aInt, int* aPtr2) =>
        throw new NotImplementedException();

    public static void _GetParentToken(IntPtr aIntPtr, int aInt, ref int aInt1) => throw new NotImplementedException();
}
