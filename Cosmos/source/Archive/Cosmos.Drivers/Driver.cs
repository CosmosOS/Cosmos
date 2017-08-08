using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Drivers
{
    /// <summary>
    /// Represents a generic device driver.
    /// </summary>
    public abstract class Driver
    {
        public abstract void Initialise(HardwareManager manager);

    }
}
