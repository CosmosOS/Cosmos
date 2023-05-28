using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Resources
{
    [Plug(TargetName = "System.Resources.ManifestBasedResourceGroveler, System.Private.CoreLib")]
    class ManifestBasedResourceGrovelerImpl
    {
        public static void HandleSatelliteMissing(object aThis)
        {
            throw new NotImplementedException();
        }

        public static global::System.Resources.ResourceSet GrovelForResourceSet(object aThis, global::System.Globalization.CultureInfo aCultureInfo,
            Dictionary<string, global::System.Resources.ResourceSet> aDictionary, bool aBool1, bool aBool2)
        {
            throw new NotImplementedException();
        }
    }
}