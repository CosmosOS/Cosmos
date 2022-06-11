using System;
using System.Threading;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Threading;

[Plug(typeof(EventWaitHandle))]
internal class EventWaitHandleImpl
{
    public static bool Set(EventWaitHandle aThis) => throw new NotImplementedException();
}
