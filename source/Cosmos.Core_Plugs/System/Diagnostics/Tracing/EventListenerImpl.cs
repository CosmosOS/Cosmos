using System;
using System.Diagnostics.Tracing;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Diagnostics.Tracing;

[Plug(typeof(EventListener))]
internal class EventListenerImpl
{
    public static void AddEventSource(EventSource aEventSource) => throw new NotImplementedException();

    public static void RemoveReferencesToListenerInEventSources(EventListener aEventListener) =>
        throw new NotImplementedException();
}
