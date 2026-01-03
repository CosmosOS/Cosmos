using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Reflection.Metadata
{
    [Plug("System.Reflection.Metadata.MetadataUpdater, System.Private.CoreLib")]
    public class MetadataUpdaterImpl
    {

        [PlugMethod(Signature = "System_Int32__System_Reflection_Metadata_MetadataUpdater__IsApplyUpdateSupported_g____PInvoke|1_0__")]
        public static bool IsApplyUpdateSupported() => false; // hot reload is not supported

    }
}