using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.HAL
{
    public interface IPS2Device
    {
        byte PS2Port { get; }
        void Initialize();
    }
}
