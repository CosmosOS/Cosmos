using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.HAL;

namespace Cosmos.System
{
    public class OpticalDrive
    {
        public static List<OpticalDrive> OpticalDrives = new List<OpticalDrive>();
        private HAL.BlockDevice.ATAPI Device;

        public OpticalDrive(HAL.BlockDevice.ATAPI aDevice)
        {
            Device = aDevice;
            Device.Test();
            OpticalDrives.Add(this);
        }

        public void Test()
        {
            Device.Test();
        }

        public void Eject()
        {
            Device.Eject();
        }
    }
}
