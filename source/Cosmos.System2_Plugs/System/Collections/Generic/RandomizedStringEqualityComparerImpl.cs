using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Collections.Generic
{
    [Plug(TargetName = "System.Collections.Generic.RandomizedStringEqualityComparer, System.Private.CoreLib")]
    class RandomizedStringEqualityComparerImpl
    {
        public static void ctor(object aThis, IEqualityComparer<string> aComparer)
        {
            throw new NotImplementedException();
        }
    }
}
