using System;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Runtime.InteropServices;

[Plug("System.Runtime.InteropServices.NativeLibrary, System.Private.CoreLib")]
internal class NativeLibraryImpl
{
    [PlugMethod(Signature =
        "System_IntPtr__System_Runtime_InteropServices_NativeLibrary_LoadByName_System_String__System_Runtime_CompilerServices_QCallAssembly__System_Boolean__System_UInt32__System_Boolean_")]
    public static IntPtr LoadByName(string aString, object aQCallAssembly, bool aBool, uint aUint, bool aBool2) =>
        throw new NotImplementedException();
}
