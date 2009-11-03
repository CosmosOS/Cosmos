using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Debug.HostProcess
{
    /// <summary>
    /// Commands sent to the hostprocess
    /// </summary>
    public enum HostProcessCommandEnum: int
    {

    }

    public enum DebugEngineCommandEnum : int
    {
        ApplicationTerminated = 0
    }
}