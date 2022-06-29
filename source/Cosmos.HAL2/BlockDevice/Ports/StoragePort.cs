using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.HAL.BlockDevice.Registers;

namespace Cosmos.HAL.BlockDevice.Ports
{
    public abstract class StoragePort : BlockDevice
    {
        public abstract PortType mPortType { get; }
        public abstract string mPortName { get; }
        public abstract uint mPortNumber { get; }
    }
}
