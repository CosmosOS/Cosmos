using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Resources;

[Plug(TargetName = "System.Resources.ManifestBasedResourceGroveler, System.Private.CoreLib")]
internal class ManifestBasedResourceGrovelerImpl
{
    public static void HandleSatelliteMissing(object aThis) => throw new NotImplementedException();

    public static ResourceSet GrovelForResourceSet(object aThis, CultureInfo aCultureInfo,
        Dictionary<string, ResourceSet> aDictionary, bool aBool1, bool aBool2) =>
        throw new NotImplementedException();
}
