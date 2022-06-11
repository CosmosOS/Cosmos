using System;
using System.Collections.Generic;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Collections.Generic;

[Plug(TargetName = "System.Collections.Generic.RandomizedStringEqualityComparer, System.Private.CoreLib")]
internal class RandomizedStringEqualityComparerImpl
{
    public static void ctor(object aThis, IEqualityComparer<string> aComparer) => throw new NotImplementedException();
}
