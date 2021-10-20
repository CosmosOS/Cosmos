using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Diagnostics.Tracing
{
    [Plug(typeof(global::System.Diagnostics.Tracing.EventSource))]
    class EventSourceImpl
    {
        public static void EnsureDescriptorsInitialized(global::System.Diagnostics.Tracing.EventSource aThis)
        {
            throw new NotImplementedException();
        }
    }
}
