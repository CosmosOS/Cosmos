using System;
using System.Collections.Generic;
using Cosmos.HAL.BlockDevice;

namespace DuNodes.HAL.FileSystem.Base
{
    public class Devices
    {
        public static List<Devices.device> dev = new List<Devices.device>();

        public static BlockDevice getDevice(string name)
        {
            for (int index = 0; index < Devices.dev.Count; ++index)
            {
                if (Devices.dev[index].name == name)
                    return Devices.dev[index].dev;
            }
            throw new Exception("Device not found!");
        }

        public class device
        {
            public string name;
            public BlockDevice dev;
        }
    }
}
