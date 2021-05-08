using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Diagnostics.Tracing
{
    [Plug(typeof(EventListener))]
    class EventListenerImpl
    {
        public static void AddEventSource(EventSource aEventSource)
        {
            throw new NotImplementedException();
        }

        public static void RemoveReferencesToListenerInEventSources(EventListener aEventListener)
        {
            throw new NotImplementedException();
        }
    }
}
