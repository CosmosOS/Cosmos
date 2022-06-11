using System;
using System.Diagnostics.Tracing;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Diagnostics.Tracing;

[Plug(typeof(EventSource))]
internal class EventSourceImpl
{
    public static void EnsureDescriptorsInitialized(EventSource aThis) => throw new NotImplementedException();
}
