using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IHAL
{
    public interface IHal
    {
        IHalProcessors Processors { get; }
        IHalScheduling Scheduling { get; }
        IBus Busses { get; }
        IDevices Devices { get; } 
        IHalIO IO { get; } 

    }
}
